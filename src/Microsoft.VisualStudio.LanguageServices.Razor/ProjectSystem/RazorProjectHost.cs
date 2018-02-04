// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.Threading;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    // Somewhat similar to https://github.com/dotnet/project-system/blob/fa074d228dcff6dae9e48ce43dd4a3a5aa22e8f0/src/Microsoft.VisualStudio.ProjectSystem.Managed/ProjectSystem/LanguageServices/LanguageServiceHost.cs
    //
    // This class is responsible for intializing the Razor ProjectSnapshotManager
    [Export]
    internal class RazorProjectHost : OnceInitializedOnceDisposedAsync
    {
        private readonly IUnconfiguredProjectCommonServices _commonServices;
        private readonly Workspace _workspace;
        private readonly AsyncSemaphore _lock;

        private ProjectSnapshotManagerBase _projectManager;
        private HostProject _current;
        private IDisposable _subscription;

        [ImportingConstructor]
        public RazorProjectHost(
            IUnconfiguredProjectCommonServices commonServices,
            [Import(typeof(VisualStudioWorkspace))] Workspace workspace)
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

            _commonServices = commonServices;
            _workspace = workspace;

            _lock = new AsyncSemaphore(initialCount: 1);
        }

        // Internal for testing
        internal RazorProjectHost(
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

            _commonServices = commonServices;
            _workspace = workspace;
            _projectManager = projectManager;

            _lock = new AsyncSemaphore(initialCount: 1);
        }


        [AppliesTo("DotNetCoreRazor")]
        [ProjectAutoLoad(ProjectLoadCheckpoint.ProjectFactoryCompleted)]
        public Task LoadAsync()
        {
            return InitializeAsync();
        }

        // Should only be called from the UI thread.
        private ProjectSnapshotManagerBase GetProjectManager()
        {
            _commonServices.ThreadingService.VerifyOnUIThread();

            if (_projectManager == null)
            {
                _projectManager = (ProjectSnapshotManagerBase)_workspace.Services.GetLanguageServices(RazorLanguage.Name).GetRequiredService<ProjectSnapshotManager>();
            }

            return _projectManager;
        }

        protected override Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            // Don't try to evaluate any properties here since the project is still loading and we require access
            // to the UI thread to push our updates.
            //
            // Just subscribe and handle the notification later.
            var receiver = new ActionBlock<IProjectVersionedValue<IProjectSubscriptionUpdate>>(OnProjectChanged);
            _subscription = _commonServices.ActiveConfiguredProjectSubscription.JointRuleSource.SourceBlock.LinkTo(
                receiver,
                initialDataAsNew: true,
                suppressVersionOnlyUpdates: true,
                ruleNames: new string[] { RazorGeneral.SchemaName, });

            return Task.CompletedTask;
        }

        protected override async Task DisposeCoreAsync(bool initialized)
        {
            if (initialized)
            {
                _subscription.Dispose();

                await ExecuteWithLock(async () =>
                {
                    if (_current != null)
                    {
                        await _commonServices.ThreadingService.SwitchToUIThread();

                        var projectManager = GetProjectManager();
                        projectManager.HostProjectRemoved(_current);
                        _current = null;
                    }
                });
            }
        }

        // Internal for testing
        internal async Task OnProjectChanged(IProjectVersionedValue<IProjectSubscriptionUpdate> update)
        {
            await ExecuteWithLock(async () =>
            {
                if (IsDisposing || IsDisposed)
                {
                    return;
                }

                await _commonServices.ThreadingService.SwitchToUIThread();
                var projectManager = GetProjectManager();

                var razorProperties = await _commonServices.ActiveConfiguredProjectRazorProperties.GetRazorGeneralPropertiesAsync().ConfigureAwait(false);
                var languageVersion = await razorProperties.RazorLangVersion.GetEvaluatedValueAtEndAsync().ConfigureAwait(false);

                if (string.IsNullOrEmpty(languageVersion))
                {
                    languageVersion = "2.1";
                }

                var hostProject = new HostProject(_commonServices.UnconfiguredProject.FullPath, languageVersion);
                if (_current == null)
                {
                    _current = hostProject;
                    projectManager.HostProjectAdded(_current);
                }
                else
                {
                    _current = hostProject;
                    projectManager.HostProjectChanged(_current);
                }
            });
        }

        private async Task ExecuteWithLock(Func<Task> func)
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
    }
}