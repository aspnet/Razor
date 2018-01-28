// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    // All of the public state of this is immutable - we create a new instance and notify subscribers
    // when it changes. 
    //
    // However we use the private state to track things like dirty/clean.
    //
    // See the private constructors... When we update the snapshot we either are processing a Workspace
    // change (Project) or updating the computed state (ProjectSnapshotUpdateContext). We don't do both
    // at once. 
    internal class DefaultProjectSnapshot : ProjectSnapshot
    {
        public DefaultProjectSnapshot(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            FilePath = filePath;
        }

        public DefaultProjectSnapshot(HostProject hostProject)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            HostProject = hostProject;
            FilePath = hostProject.FilePath;
        }

        public DefaultProjectSnapshot(Project workspaceProject)
        {
            if (workspaceProject == null)
            {
                throw new ArgumentNullException(nameof(workspaceProject));
            }

            WorkspaceProject = workspaceProject;
            FilePath = workspaceProject.FilePath;
        }

        private DefaultProjectSnapshot(HostProject hostProject, DefaultProjectSnapshot other)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            ComputedVersion = other.ComputedVersion;
            Configuration = other.Configuration;
            FilePath = other.FilePath;
            HostProject = hostProject;
            WorkspaceProject = other.WorkspaceProject;
        }

        private DefaultProjectSnapshot(Project workspaceProject, DefaultProjectSnapshot other)
        {
            if (workspaceProject == null)
            {
                throw new ArgumentNullException(nameof(workspaceProject));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            ComputedVersion = other.ComputedVersion;
            Configuration = other.Configuration;
            FilePath = other.FilePath;
            HostProject = other.HostProject;
            WorkspaceProject = workspaceProject;
        }

        private DefaultProjectSnapshot(ProjectSnapshotUpdateContext update, DefaultProjectSnapshot other)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            ComputedVersion = update.WorkspaceProject.Version;
            Configuration = update.Configuration;
            FilePath = other.FilePath;
            HostProject = other.HostProject;
            WorkspaceProject = other.WorkspaceProject;
        }

        public override ProjectExtensibilityConfiguration Configuration { get; }

        public override string FilePath { get; }

        public HostProject HostProject { get; }

        public override bool IsInitialized => HostProject != null && WorkspaceProject != null;

        public override bool IsUnloaded => HostProject == null && WorkspaceProject == null;

        public override Project WorkspaceProject { get; }

        // This is the version that the computed state is based on.
        public VersionStamp? ComputedVersion { get; set; }

        // We know the project is dirty if we don't have a computed result, or it was computed for a different version.
        // Since the PSM updates the snapshots synchronously, the snapshot can never be older than the computed state.
        public bool IsDirty => ComputedVersion == null || ComputedVersion.Value != WorkspaceProject.Version;

        public DefaultProjectSnapshot RemoveHostProject()
        {
            if (WorkspaceProject == null)
            {
                // If we've removed the workspace project and host project then this project is unloading.
                return new DefaultProjectSnapshot(HostProject.FilePath);
            }
            else
            {
                // We want to get rid of all of the computed state since it's not really valid.
                return new DefaultProjectSnapshot(WorkspaceProject);
            }
        }

        public DefaultProjectSnapshot WithHostProject(HostProject hostProject)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            return new DefaultProjectSnapshot(hostProject, this);
        }

        public DefaultProjectSnapshot RemoveWorkspaceProject()
        {
            if (HostProject == null)
            {
                // If we've removed the workspace project and host project then this project is unloading.
                return new DefaultProjectSnapshot(WorkspaceProject.FilePath);
            }
            else
            {
                // We want to get rid of all of the computed state since it's not really valid.
                return new DefaultProjectSnapshot(HostProject);
            }
        }

        public DefaultProjectSnapshot WithWorkspaceProject(Project workspaceProject)
        {
            if (workspaceProject == null)
            {
                throw new ArgumentNullException(nameof(workspaceProject));
            }

            return new DefaultProjectSnapshot(workspaceProject, this);
        }

        public DefaultProjectSnapshot WithComputedUpdate(ProjectSnapshotUpdateContext update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            return new DefaultProjectSnapshot(update, this);
        }

        public bool HasChangesComparedTo(ProjectSnapshot original)
        {
            if (original == null)
            {
                throw new ArgumentNullException(nameof(original));
            }

            return !object.Equals(Configuration, original.Configuration);
        }
    }
}
