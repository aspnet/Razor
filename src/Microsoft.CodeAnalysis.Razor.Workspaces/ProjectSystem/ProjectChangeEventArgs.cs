// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal class ProjectChangeEventArgs : EventArgs
    {
        public ProjectChangeEventArgs(ProjectId projectId, ProjectChangeKind kind)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            ProjectId = projectId;
            Kind = kind;
        }

        public ProjectChangeEventArgs(ProjectId projectId, string documentFilePath, ProjectChangeKind kind)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            ProjectId = projectId;
            DocumentFilePath = documentFilePath;
            Kind = kind;
        }

        public ProjectId ProjectId { get; }

        public string DocumentFilePath { get; }

        public ProjectChangeKind Kind { get; }
    }
}
