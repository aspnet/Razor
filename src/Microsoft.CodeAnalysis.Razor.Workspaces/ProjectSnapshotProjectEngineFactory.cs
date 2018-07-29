// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;

namespace Microsoft.CodeAnalysis.Razor
{
    internal abstract class ProjectSnapshotProjectEngineFactory : IWorkspaceService
    {
        public abstract IProjectEngineFactory FindFactory(ProjectSnapshot project);

        public abstract IProjectEngineFactory FindSerializableFactory(ProjectSnapshot project);

        public RazorProjectEngine Create(ProjectSnapshot project)
        {
            RazorProjectFileSystem fileSystem;
            if (project.FilePath != null)
            {
                fileSystem = RazorProjectFileSystem.Create(Path.GetDirectoryName(project.FilePath));
            }
            else
            {
                fileSystem = RazorProjectFileSystem.Empty;
            }

            return Create(project, fileSystem, null);
        }

        public RazorProjectEngine Create(ProjectSnapshot project, RazorProjectFileSystem fileSystem)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (fileSystem == null)
            {
                throw new ArgumentNullException(nameof(fileSystem));
            }

            return Create(project, fileSystem, null);
        }

        public RazorProjectEngine Create(ProjectSnapshot project, Action<RazorProjectEngineBuilder> configure)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            RazorProjectFileSystem fileSystem;
            if (project.FilePath != null)
            {
                fileSystem = RazorProjectFileSystem.Create(Path.GetDirectoryName(project.FilePath));
            }
            else
            {
                fileSystem = RazorProjectFileSystem.Empty;
            }

            return Create(project, fileSystem, configure);
        }

        public abstract RazorProjectEngine Create(ProjectSnapshot project, RazorProjectFileSystem fileSystem, Action<RazorProjectEngineBuilder> configure);

    }
}
