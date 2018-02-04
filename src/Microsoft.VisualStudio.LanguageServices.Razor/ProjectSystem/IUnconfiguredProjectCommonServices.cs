// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    [Export(typeof(IUnconfiguredProjectCommonServices))]
    internal class UnconfiguredProjectCommonServices : IUnconfiguredProjectCommonServices
    {
        private readonly ActiveConfiguredProject<ConfiguredProject> _activeConfiguredProject;
        private readonly ActiveConfiguredProject<RazorProjectProperties> _activeConfiguredProjectProperties;

        [ImportingConstructor]
        public UnconfiguredProjectCommonServices(
            IProjectThreadingService threadingService,
            UnconfiguredProject unconfiguredProject,
            IActiveConfiguredProjectSubscriptionService activeConfiguredProjectSubscription,
            ActiveConfiguredProject<ConfiguredProject> activeConfiguredProject,
            ActiveConfiguredProject<RazorProjectProperties> activeConfiguredProjectRazorProperties)
        {
            if (threadingService == null)
            {
                throw new ArgumentNullException(nameof(threadingService));
            }

            if (unconfiguredProject == null)
            {
                throw new ArgumentNullException(nameof(unconfiguredProject));
            }

            if (activeConfiguredProjectSubscription == null)
            {
                throw new ArgumentNullException(nameof(ActiveConfiguredProjectSubscription));
            }

            if (activeConfiguredProject == null)
            {
                throw new ArgumentNullException(nameof(activeConfiguredProject));
            }

            if (activeConfiguredProjectRazorProperties == null)
            {
                throw new ArgumentNullException(nameof(activeConfiguredProjectRazorProperties));
            }

            ThreadingService = threadingService;
            UnconfiguredProject = unconfiguredProject;
            ActiveConfiguredProjectSubscription = activeConfiguredProjectSubscription;
            _activeConfiguredProject = activeConfiguredProject;
            _activeConfiguredProjectProperties = activeConfiguredProjectRazorProperties;
        }

        public ConfiguredProject ActiveConfiguredProject => _activeConfiguredProject.Value;

        public RazorProjectProperties ActiveConfiguredProjectRazorProperties => _activeConfiguredProjectProperties.Value;
        
        public IActiveConfiguredProjectSubscriptionService ActiveConfiguredProjectSubscription { get; }

        public IProjectThreadingService ThreadingService { get; }

        public UnconfiguredProject UnconfiguredProject { get; }
    }
}
