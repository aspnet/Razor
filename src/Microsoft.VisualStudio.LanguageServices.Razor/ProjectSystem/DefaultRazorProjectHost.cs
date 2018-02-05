// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    // Somewhat similar to https://github.com/dotnet/project-system/blob/fa074d228dcff6dae9e48ce43dd4a3a5aa22e8f0/src/Microsoft.VisualStudio.ProjectSystem.Managed/ProjectSystem/LanguageServices/LanguageServiceHost.cs
    //
    // This class is responsible for intializing the Razor ProjectSnapshotManager for cases where
    // MSBuild provides configuration support (>= 2.1).
    [AppliesTo("DotNetCoreRazor & DotNetCoreRazorConfiguration")]
    [Export(ExportContractNames.Scopes.UnconfiguredProject, typeof(IProjectDynamicLoadComponent))]
    internal class DefaultRazorProjectHost : RazorProjectHostBase
    {
        private IDisposable _subscription;

        [ImportingConstructor]
        public DefaultRazorProjectHost(
            IUnconfiguredProjectCommonServices commonServices,
            [Import(typeof(VisualStudioWorkspace))] Workspace workspace)
            : base(commonServices, workspace)
        {
        }

        // Internal for testing
        internal DefaultRazorProjectHost(
            IUnconfiguredProjectCommonServices commonServices,
             Workspace workspace,
             ProjectSnapshotManagerBase projectManager)
            : base(commonServices, workspace, projectManager)
        {
        }

        protected override async Task InitializeCoreAsync(CancellationToken cancellationToken)
        {
            await base.InitializeCoreAsync(cancellationToken).ConfigureAwait(false);

            // Don't try to evaluate any properties here since the project is still loading and we require access
            // to the UI thread to push our updates.
            //
            // Just subscribe and handle the notification later.
            var receiver = new ActionBlock<IProjectVersionedValue<IProjectSubscriptionUpdate>>(OnProjectChanged);
            _subscription = CommonServices.ActiveConfiguredProjectSubscription.JointRuleSource.SourceBlock.LinkTo(
                receiver,
                initialDataAsNew: true,
                suppressVersionOnlyUpdates: true,
                ruleNames: new string[] { RazorGeneral.SchemaName, });
        }

        protected override async Task DisposeCoreAsync(bool initialized)
        {
            await base.DisposeCoreAsync(initialized).ConfigureAwait(false);

            if (initialized)
            {
                _subscription.Dispose();
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

                var languageVersion = update.Value.CurrentState[RazorGeneral.SchemaName].Properties[RazorGeneral.RazorLangVersionProperty];

                if (string.IsNullOrEmpty(languageVersion))
                {
                    // Ok we can't find a language version. Let's assume this project isn't using Razor then.
                    await UpdateProjectUnsafeAsync(null).ConfigureAwait(false);
                    return;
                }

                var hostProject = new HostProject(CommonServices.UnconfiguredProject.FullPath, languageVersion);
                await UpdateProjectUnsafeAsync(hostProject).ConfigureAwait(false);
            });
        }
    }
}