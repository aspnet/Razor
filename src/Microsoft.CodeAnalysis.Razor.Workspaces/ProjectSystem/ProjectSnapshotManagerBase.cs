﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal abstract class ProjectSnapshotManagerBase : ProjectSnapshotManager
    {
        public abstract Workspace Workspace { get; }

        public abstract void DocumentAdded(HostProject hostProject, HostDocument hostDocument, TextLoader textLoader);

        public abstract void DocumentOpened(ProjectId projectId, string documentFilePath, SourceText sourceText);

        public abstract void DocumentClosed(ProjectId projectId, string documentFilePath, TextLoader textLoader);

        public abstract void DocumentChanged(ProjectId projectId, string documentFilePath, TextLoader textLoader);

        public abstract void DocumentChanged(ProjectId projectId, string documentFilePath, SourceText sourceText);

        public abstract void DocumentRemoved(HostProject hostProject, HostDocument hostDocument);

        public abstract void HostProjectAdded(HostProject hostProject);

        public abstract void HostProjectChanged(HostProject hostProject);

        public abstract void HostProjectRemoved(HostProject hostProject);

        public abstract void WorkspaceProjectAdded(Project workspaceProject);

        public abstract void WorkspaceProjectChanged(Project workspaceProject);

        public abstract void WorkspaceProjectRemoved(Project workspaceProject);

        public abstract void ReportError(Exception exception);

        public abstract void ReportError(Exception exception, ProjectSnapshot project);

        public abstract void ReportError(Exception exception, HostProject hostProject);

        public abstract void ReportError(Exception exception, Project workspaceProject);
    }
}