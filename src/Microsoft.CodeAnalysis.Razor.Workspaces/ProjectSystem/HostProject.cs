// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal class HostProject
    {
        public HostProject(ProjectId projectId, string projectFilePath, RazorConfiguration razorConfiguration)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            if (projectFilePath == null)
            {
                throw new ArgumentNullException(nameof(projectFilePath));
            }

            if (razorConfiguration == null)
            {
                throw new ArgumentNullException(nameof(razorConfiguration));
            }

            Id = projectId;
            FilePath = projectFilePath;
            Configuration = razorConfiguration;
        }

        public RazorConfiguration Configuration { get; }

        public string FilePath { get; }

        public ProjectId Id { get; }
    }
}