// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.ComponentModel.Composition;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.Properties;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    [Export]
    internal partial class ProjectProperties : StronglyTypedPropertyAccess
    {
        [ImportingConstructor]
        public ProjectProperties(ConfiguredProject configuredProject) 
            : base(configuredProject)
        {
        }

        public ProjectProperties(ConfiguredProject configuredProject, IProjectPropertiesContext projectPropertiesContext) 
            : base(configuredProject, projectPropertiesContext)
        {
        }

        public ProjectProperties(ConfiguredProject configuredProject, UnconfiguredProject unconfiguredProject) 
            : base(configuredProject, unconfiguredProject)
        {
        }

        public ProjectProperties(ConfiguredProject configuredProject, string file, string itemType, string itemName) 
            : base(configuredProject, file, itemType, itemName)
        {
        }
    }
}
