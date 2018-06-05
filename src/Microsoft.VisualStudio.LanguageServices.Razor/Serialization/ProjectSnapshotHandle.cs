// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal sealed class ProjectSnapshotHandle
    {
        public ProjectSnapshotHandle(string filePath, ProjectId hostProjectId, RazorConfiguration configuration, ProjectId workspaceProjectId)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (hostProjectId == null)
            {
                throw new ArgumentNullException(nameof(hostProjectId));
            }

            FilePath = filePath;
            HostProjectId = hostProjectId;
            Configuration = configuration;
            WorkspaceProjectId = workspaceProjectId;
        }

        public RazorConfiguration Configuration { get; }

        public ProjectId HostProjectId { get; set; }

        public string FilePath { get; }

        public ProjectId WorkspaceProjectId { get; }
    }
}
