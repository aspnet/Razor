// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal class ProjectSnapshotUpdateContext
    {
        public ProjectSnapshotUpdateContext(Project workspaceProject)
        {
            if (workspaceProject == null)
            {
                throw new ArgumentNullException(nameof(workspaceProject));
            }

            WorkspaceProject = workspaceProject;
        }

        public Project WorkspaceProject { get; }

        public ProjectExtensibilityConfiguration Configuration { get; set; }
    }
}
