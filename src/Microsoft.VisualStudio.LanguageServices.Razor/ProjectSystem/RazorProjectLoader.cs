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
    [AppliesTo("RazorProjectSystem")]
    [Export]
    internal class RazorProjectLoader : OnceInitializedOnceDisposedAsync
    {
        private readonly UnconfiguredProject _unconfiguredProject;
        private readonly ActiveConfiguredProject<ProjectProperties> _projectProperties;
        private readonly IProjectThreadingService _threadingService;
        private readonly Workspace _workspace;

        private readonly AsyncSemaphore _lock;
        private HostProject _projectData;
        private IDisposable _subscription;
        private bool _unloaded;

        [ImportingConstructor]
        public RazorProjectLoader(
            UnconfiguredProject unconfiguredProject,
            ActiveConfiguredProject<ProjectProperties> projectProperties,
            IProjectThreadingService threadingService,
            VisualStudioWorkspace workspace)
            : base(threadingService.JoinableTaskContext)
        {
            if (unconfiguredProject == null)
            {
                throw new ArgumentNullException(nameof(unconfiguredProject));
            }

            if (projectProperties == null)
            {
                throw new ArgumentNullException(nameof(projectProperties));
            }

            if (threadingService == null)
            {
                throw new ArgumentNullException(nameof(threadingService));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            _unconfiguredProject = unconfiguredProject;
            _projectProperties = projectProperties;
            _threadingService = threadingService;
            _workspace = workspace;

            _lock = new AsyncSemaphore(initialCount: 1);
        }

        [AppliesTo("RazorProjectSystem")]
        [ProjectAutoLoad(ProjectLoadCheckpoint.ProjectFactoryCompleted)]
        public Task LoadAsync()
        {
            return InitializeAsync();
        }

        // Should only be called from the UI thread. It's possible that our call to get the project manager might be
        // the first call, which would try to initialize the foreground dispatcher on a background thread
        // if we don't take care.
        private ProjectSnapshotManagerBase GetProjectManager()
        {
            return (ProjectSnapshotManagerBase)_workspace.Services.GetLanguageServices(RazorLanguage.Name).GetRequiredService<ProjectSnapshotManager>();
        }

        protected override Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            // Don't try to evaluation any properties here since the project is still loading.
            var receiver = new ActionBlock<IProjectVersionedValue<IProjectSubscriptionUpdate>>(OnProjectChanged);
            _subscription = _unconfiguredProject.Services.ActiveConfiguredProjectSubscription.JointRuleSource.SourceBlock.LinkTo(
                receiver,
                initialDataAsNew: true,
                suppressVersionOnlyUpdates: true,
                ruleNames: new string[] { General.SchemaName, Component.SchemaName, View.SchemaName, });

            return Task.CompletedTask;
        }

        protected override async Task DisposeCoreAsync(bool initialized)
        {
            _subscription.Dispose();

            await ExecuteWithLock(async () =>
            {
                await _threadingService.SwitchToUIThread();
                GetProjectManager().HostProjectRemoved(_projectData);
                _projectData = null;

                _unloaded = true;
            });
        }

        private async Task OnProjectChanged(IProjectVersionedValue<IProjectSubscriptionUpdate> update)
        {
            await ExecuteWithLock(async () =>
            {
                if (_unloaded)
                {
                    return;
                }

                if (_projectData == null)
                {
                    var projectPropertiesProvider = _unconfiguredProject.Services.ActiveConfiguredProjectProvider.ActiveConfiguredProject.Services.ProjectPropertiesProvider;
                    var commonProperties = projectPropertiesProvider.GetCommonProperties();
                    var tfm = await commonProperties.GetEvaluatedPropertyValueAsync("TargetFramework").ConfigureAwait(false);

                    var razorProperties = await _projectProperties.Value.GetGeneralPropertiesAsync().ConfigureAwait(false);
                    var languageVersion = await razorProperties.RazorLangVersion.GetEvaluatedValueAtEndAsync().ConfigureAwait(false);

                    var documents = new List<HostDocument>();
                    var items = update.Value.CurrentState["View"].Items;
                    foreach (var item in items)
                    {
                        documents.Add(new HostDocument(item.Key, item.Value["GeneratedFile"]));
                    }

                    _projectData = new HostProject(_unconfiguredProject.FullPath, tfm, languageVersion, documents);

                    await _threadingService.SwitchToUIThread();
                    GetProjectManager().HostProjectAdded(_projectData);
                }

                // We don't handle updates right now, just the initial state.
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
