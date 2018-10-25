// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Composition;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.LanguageServices.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Threading;

#if WORKSPACE_PROJECT_CONTEXT_FACTORY
using IWorkspaceProjectContextFactory = Microsoft.VisualStudio.LanguageServices.ProjectSystem.IWorkspaceProjectContextFactory2;
#endif

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal abstract class RazorProjectHostBase : OnceInitializedOnceDisposedAsync, IProjectDynamicLoadComponent
    {
        private readonly Workspace _workspace;
        private readonly Lazy<IWorkspaceProjectContextFactory> _projectContextFactory;
        private readonly AsyncSemaphore _lock;

        protected Lazy<RazorDynamicFileInfoProvider> _provider;
        private ProjectSnapshotManagerBase _projectManager;
        private HostProject _current;
        private IWorkspaceProjectContext _projectContext;
        private Dictionary<string, HostDocument> _currentDocuments;

        public RazorProjectHostBase(
            IUnconfiguredProjectCommonServices commonServices,
            [Import(typeof(VisualStudioWorkspace))] Workspace workspace,
            Lazy<RazorDynamicFileInfoProvider> provider,
            Lazy<IWorkspaceProjectContextFactory> projectContextFactory)
            : base(commonServices.ThreadingService.JoinableTaskContext)
        {
            if (commonServices == null)
            {
                throw new ArgumentNullException(nameof(commonServices));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            CommonServices = commonServices;
            _workspace = workspace;
            _provider = provider;
            _projectContextFactory = projectContextFactory;

            _lock = new AsyncSemaphore(initialCount: 1);
            _currentDocuments = new Dictionary<string, HostDocument>(FilePathComparer.Instance);
        }

        // Internal for testing
        protected RazorProjectHostBase(
            IUnconfiguredProjectCommonServices commonServices,
             Workspace workspace,
             ProjectSnapshotManagerBase projectManager)
            : base(commonServices.ThreadingService.JoinableTaskContext)
        {
            if (commonServices == null)
            {
                throw new ArgumentNullException(nameof(commonServices));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (projectManager == null)
            {
                throw new ArgumentNullException(nameof(projectManager));
            }

            CommonServices = commonServices;
            _workspace = workspace;
            _projectManager = projectManager;

            _lock = new AsyncSemaphore(initialCount: 1);
            _currentDocuments = new Dictionary<string, HostDocument>(FilePathComparer.Instance);
        }

        protected HostProject Current => _current;

        protected IUnconfiguredProjectCommonServices CommonServices { get; }

        // internal for tests. The product will call through the IProjectDynamicLoadComponent interface.
        internal Task LoadAsync()
        {
            return InitializeAsync();
        }

        protected override Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            CommonServices.UnconfiguredProject.ProjectRenaming += UnconfiguredProject_ProjectRenaming;

            return Task.CompletedTask;
        }

        protected override async Task DisposeCoreAsync(bool initialized)
        {
            if (initialized)
            {
                CommonServices.UnconfiguredProject.ProjectRenaming -= UnconfiguredProject_ProjectRenaming;

                await ExecuteWithLock(async () =>
                {
                    if (_current != null)
                    {
                        await UpdateAsync(UninitializeProjectUnsafe).ConfigureAwait(false);
                    }
                });
            }
        }

        // Internal for tests
        internal async Task OnProjectRenamingAsync()
        {
            // When a project gets renamed we expect any rules watched by the derived class to fire.
            //
            // However, the project snapshot manager uses the project Fullpath as the key. We want to just
            // reinitialize the HostProject with the same configuration and settings here, but the updated
            // FilePath.
            await ExecuteWithLock(async () =>
            {
                if (_current != null)
                {
                    var old = _current;
                    var oldDocuments = _currentDocuments.Values.ToArray();

                    await UpdateAsync(UninitializeProjectUnsafe).ConfigureAwait(false);

                    await UpdateAsync(() =>
                    {
                        var filePath = CommonServices.UnconfiguredProject.FullPath;
                        UpdateProjectUnsafe(new HostProject(filePath, old.Configuration));

                        // This should no-op in the common case, just putting it here for insurance.
                        for (var i = 0; i < oldDocuments.Length; i++)
                        {
                            AddDocumentUnsafe(oldDocuments[i]);
                        }
                    }).ConfigureAwait(false);
                }
            });
        }

        // Should only be called from the UI thread.
        private ProjectSnapshotManagerBase GetProjectManager()
        {
            CommonServices.ThreadingService.VerifyOnUIThread();

            if (_projectManager == null)
            {
                _projectManager = (ProjectSnapshotManagerBase)_workspace.Services.GetLanguageServices(RazorLanguage.Name).GetRequiredService<ProjectSnapshotManager>();
            }

            return _projectManager;
        }

        private IWorkspaceProjectContextFactory GetProjectContextFactory()
        {
            CommonServices.ThreadingService.VerifyOnUIThread();
            return _projectContextFactory?.Value;
        }

        protected async Task UpdateAsync(Action action)
        {
            await CommonServices.ThreadingService.SwitchToUIThread();
            action();
        }

        protected void UninitializeProjectUnsafe()
        {
            ClearDocumentsUnsafe();
            UpdateProjectUnsafe(null);
        }

        protected void UpdateProjectUnsafe(HostProject project)
        {
            var projectManager = GetProjectManager();
            if (_current == null && project == null)
            {
                // This is a no-op. This project isn't using Razor.
            }
            else if (_current == null && project != null)
            {
                var projectContextFactory = GetProjectContextFactory();
                _projectContext = (IWorkspaceProjectContext)projectContextFactory.GetType().GetProperties().Single().GetMethod.Invoke(projectContextFactory, new object[] { CommonServices.UnconfiguredProject.FullPath });

                projectManager.HostProjectAdded(project);
            }
            else if (_current != null && project == null)
            {
                Debug.Assert(_currentDocuments.Count == 0);
                projectManager.HostProjectRemoved(_current);
                _projectContext?.Dispose();
                _projectContext = null;
            }
            else
            {
                projectManager.HostProjectChanged(project);
            }

            _current = project;
        }

        protected void AddDocumentUnsafe(HostDocument document)
        {
            var projectManager = GetProjectManager();

            if (_currentDocuments.ContainsKey(document.FilePath))
            {
                // Ignore duplicates
                return;
            }

            projectManager.DocumentAdded(_current, document, new FileTextLoader(document.FilePath, null));
            _projectContext?.AddDynamicFile(
                document.FilePath,
                folderNames: GetFolders(document));
            _currentDocuments.Add(document.FilePath, document);
        }

        protected void RemoveDocumentUnsafe(HostDocument document)
        {
            var projectManager = GetProjectManager();

            _projectContext?.RemoveDynamicFile(document.FilePath);

            projectManager.DocumentRemoved(_current, document);
            _currentDocuments.Remove(document.FilePath);
        }

        protected void ClearDocumentsUnsafe()
        {
            var projectManager = GetProjectManager();

            foreach (var kvp in _currentDocuments)
            {
                _projectContext?.RemoveDynamicFile(kvp.Value.FilePath);
                _projectManager.DocumentRemoved(_current, kvp.Value);
            }

            _currentDocuments.Clear();
        }
        
        protected async Task ExecuteWithLock(Func<Task> func)
        {
            using (JoinableCollection.Join())
            {
                using (await _lock.EnterAsync().ConfigureAwait(false))
                {
                    var task = JoinableFactory.RunAsync(func);
                    await task.Task.ConfigureAwait(false);
                }
            }
        }

        Task IProjectDynamicLoadComponent.LoadAsync()
        {
            return InitializeAsync();
        }

        Task IProjectDynamicLoadComponent.UnloadAsync()
        {
            return DisposeAsync();
        }

        private async Task UnconfiguredProject_ProjectRenaming(object sender, ProjectRenamedEventArgs args)
        {
            await OnProjectRenamingAsync().ConfigureAwait(false);
        }

        private static IEnumerable<string> GetFolders(HostDocument document)
        {
            var split = document.TargetPath.Split('/');
            return split.Take(split.Length - 1);
        }

        private class IUnconfiguredProjectHostObject
        {
            private readonly object _inner;

            public IUnconfiguredProjectHostObject(object inner)
            {
                _inner = inner;
            }

            public object ActiveIntellisenseProjectHostObject
            {
                get
                {
                    return  _inner.GetType().GetProperty(nameof(ActiveIntellisenseProjectHostObject)).GetValue(_inner);
                }
            }
        }
        
        private class IProjectHostProvider
        {
            private readonly object _inner;

            public IProjectHostProvider(object inner)
            {
                _inner = inner;
            }

            public IUnconfiguredProjectHostObject UnconfiguredProjectHostObject
            {
                get
                {
                    var inner = _inner.GetType().GetProperty(nameof(UnconfiguredProjectHostObject)).GetValue(_inner);
                    return new IUnconfiguredProjectHostObject(inner);
                }
            }
        }
    }
}