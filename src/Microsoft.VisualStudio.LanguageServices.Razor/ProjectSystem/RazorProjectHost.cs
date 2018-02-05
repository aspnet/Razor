// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.AspNetCore.Razor.Language;
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
        private const string MvcAssemblyName = "Microsoft.AspNetCore.Mvc.Razor";
        private const string MvcAssemblyFileName = "Microsoft.AspNetCore.Mvc.Razor.dll";
        private const string RazorV1AssemblyName = "Microsoft.AspNetCore.Razor";
        private const string RazorV1AssemblyFileName = "Microsoft.AspNetCore.Razor.dll";
        private const string RazorV2AssemblyName = "Microsoft.AspNetCore.Razor.Language";
        private const string RazorV2AssemblyFileName = "Microsoft.AspNetCore.Razor.Language.dll";

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
                ruleNames: new string[] { Rules.RazorGeneral.SchemaName, Rules.RazorConfiguration.SchemaName, Rules.RazorExtension.SchemaName });

            return Task.CompletedTask;
        }

        protected override async Task DisposeCoreAsync(bool initialized)
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

                var languageVersion = update.Value.CurrentState[Rules.RazorGeneral.SchemaName].Properties[Rules.RazorGeneral.RazorLangVersionProperty];
                var defaultConfiguration = update.Value.CurrentState[Rules.RazorGeneral.SchemaName].Properties[Rules.RazorGeneral.RazorDefaultConfigurationProperty];

                RazorConfiguration configuration = null;
                if (!string.IsNullOrEmpty(languageVersion) && !string.IsNullOrEmpty(defaultConfiguration))
                {
                    if (!RazorLanguageVersion.TryParse(languageVersion, out var parsedVersion))
                    {
                        parsedVersion = RazorLanguageVersion.Latest;
                    }

                    var extensions = update.Value.CurrentState[Rules.RazorExtension.PrimaryDataSourceItemType].Items.Select(e =>
                    {
                        return new ProjectSystemRazorExtension(e.Key);
                    }).ToArray();

                    var configurations = update.Value.CurrentState[Rules.RazorConfiguration.PrimaryDataSourceItemType].Items.Select(c =>
                    {
                        var includedExtensions = c.Value[Rules.RazorConfiguration.ExtensionsProperty]
                            .Split(';')
                            .Select(name => extensions.Where(e => e.ExtensionName == name).FirstOrDefault())
                            .Where(e => e != null)
                            .ToArray();

                        return new ProjectSystemRazorConfiguration(parsedVersion, c.Key, includedExtensions);
                    }).ToArray();

                    configuration = configurations.Where(c => c.ConfigurationName == defaultConfiguration).FirstOrDefault();
                }
               
                if (configuration == null)
                {
                    // Versions of ASP.NET Core prior to 2.1 were released before the Razor SDK, and so don't provide the
                    // metadata we use to detect the RazorConfiguration. In these cases we fall back a somewhat primitive
                    // "assembly sniffing" technique.
                    configuration = await DetectFallbackLanguageConfiguration().ConfigureAwait(false);
                }

                if (configuration == null)
                {
                    // Ok we really can't find a language version. Let's assume this project isn't using Razor then.
                    if (_current != null)
                    {
                        projectManager.HostProjectRemoved(_current);
                        _current = null;
                    }

                    return;
                }

                var hostProject = new HostProject(_commonServices.UnconfiguredProject.FullPath, configuration);
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

        private async Task<RazorConfiguration> DetectFallbackLanguageConfiguration()
        {
            var references = await _commonServices.ActiveConfiguredProjectAssemblyReferences.GetResolvedReferencesAsync().ConfigureAwait(false);
            foreach (var reference in references)
            {
                var fullPath = await reference.GetFullPathAsync().ConfigureAwait(false);
                if (fullPath.EndsWith(MvcAssemblyFileName, StringComparison.OrdinalIgnoreCase))
                {
                    var assemblyName = await reference.GetAssemblyNameAsync().ConfigureAwait(false);
                    if (assemblyName.Name == MvcAssemblyName)
                    {
                        return FallbackRazorConfiguration.SelectConfiguration(assemblyName.Version);
                    }
                }
            }

            return null;
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