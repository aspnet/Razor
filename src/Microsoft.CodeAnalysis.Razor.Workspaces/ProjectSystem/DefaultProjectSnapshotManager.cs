﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal class DefaultProjectSnapshotManager : ProjectSnapshotManagerBase
    {
        public override event EventHandler<ProjectChangeEventArgs> Changed;

        private readonly ErrorReporter _errorReporter;
        private readonly ForegroundDispatcher _foregroundDispatcher;
        private readonly ProjectSnapshotChangeTrigger[] _triggers;
        private readonly ProjectSnapshotUpdateListener _updateListener;
        private readonly ProjectSnapshotWorkerQueue _workerQueue;
        private readonly ProjectSnapshotWorker _worker;

        private readonly Dictionary<string, DefaultProjectSnapshot> _projects;

        public DefaultProjectSnapshotManager(
            ForegroundDispatcher foregroundDispatcher,
            ErrorReporter errorReporter,
            ProjectSnapshotWorker worker,
            IEnumerable<ProjectSnapshotChangeTrigger> triggers,
            Workspace workspace)
        {
            if (foregroundDispatcher == null)
            {
                throw new ArgumentNullException(nameof(foregroundDispatcher));
            }

            if (errorReporter == null)
            {
                throw new ArgumentNullException(nameof(errorReporter));
            }

            if (worker == null)
            {
                throw new ArgumentNullException(nameof(worker));
            }

            if (triggers == null)
            {
                throw new ArgumentNullException(nameof(triggers));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            _foregroundDispatcher = foregroundDispatcher;
            _errorReporter = errorReporter;
            _worker = worker;
            _triggers = triggers.ToArray();
            Workspace = workspace;

            _updateListener = workspace.Services.GetLanguageServices(RazorLanguage.Name).GetRequiredService<ProjectSnapshotUpdateListener>();

            _projects = new Dictionary<string, DefaultProjectSnapshot>();

            _workerQueue = new ProjectSnapshotWorkerQueue(_foregroundDispatcher, this, worker);

            for (var i = 0; i < _triggers.Length; i++)
            {
                _triggers[i].Initialize(this);
            }
        }

        public override IReadOnlyList<ProjectSnapshot> Projects
        {
            get
            {
                return _projects.Values.ToArray();
            }
        }

        public override Workspace Workspace { get; }

        public override void ProjectUpdated(ProjectSnapshotUpdateContext update)
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }
            
            if (_projects.TryGetValue(update.WorkspaceProject.FilePath, out var original))
            {
                // This is an update to the project's computed values, so everything should be overwritten
                var snapshot = original.WithComputedUpdate(update);
                _projects[update.WorkspaceProject.FilePath] = snapshot;

                _updateListener.OnProjectUpdated(snapshot);

                if (snapshot.IsDirty)
                {
                    // It's possible that the snapshot can still be dirty if we got a project update while computing state in
                    // the background. We need to trigger the background work to asynchronously compute the effect of the updates.
                    NotifyBackgroundWorker(snapshot.WorkspaceProject);
                }

                // Now we need to know if the changes that we applied are significant. If that's the case then 
                // we need to notify listeners.
                if (snapshot.HasChangesComparedTo(original))
                {
                    NotifyListeners(new ProjectChangeEventArgs(snapshot, ProjectChangeKind.Changed));
                }
            }
        }

        public override void HostProjectAdded(HostProject hostProject)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            // It's possible that the workspace project was already initialized, and the host project is showing up
            // a bit later. If that's the case then treat this as an update.
            if (_projects.ContainsKey(hostProject.FilePath))
            {
                HostProjectChanged(hostProject);
                return;
            }

            var snapshot = new DefaultProjectSnapshot(hostProject);
            _projects[hostProject.FilePath] = snapshot;

            // We expect new projects to always be dirty but the workspace project isn't yet set. We don't attempt to compute
            // any state until the project is initialized.
            //
            // We need to notify listeners about every project add.
            NotifyListeners(new ProjectChangeEventArgs(snapshot, ProjectChangeKind.Added));
        }

        public override void HostProjectChanged(HostProject hostProject)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            if (_projects.TryGetValue(hostProject.FilePath, out var original))
            {
                // Doing an update to the project should keep computed values, but mark the project as dirty if the
                // underlying project is newer.
                var snapshot = original.WithHostProject(hostProject);
                _projects[hostProject.FilePath] = snapshot;

                if (snapshot.IsInitialized && snapshot.IsDirty)
                {
                    // We don't need to notify listeners yet because we don't have any **new** computed state. However we do 
                    // need to trigger the background work to asynchronously compute the effect of the updates.
                    NotifyBackgroundWorker(snapshot.WorkspaceProject);
                }
            }
        }

        public override void HostProjectRemoved(HostProject hostProject)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            if (_projects.TryGetValue(hostProject.FilePath, out var snapshot))
            {
                snapshot = snapshot.RemoveHostProject();
                _projects[hostProject.FilePath] = snapshot;

                if (snapshot.IsUnloaded)
                {
                    _projects.Remove(hostProject.FilePath);

                    // We need to notify listeners about every project removal.
                    NotifyListeners(new ProjectChangeEventArgs(snapshot, ProjectChangeKind.Removed));
                }
            }
        }

        public override void WorkspaceProjectAdded(Project workspaceProject)
        {
            if (workspaceProject == null)
            {
                throw new ArgumentNullException(nameof(workspaceProject));
            }

            // It's possible that the host project was already initialized, and the workspace project is showing up
            // a bit later. If that's the case then treat this as an update.
            if (_projects.ContainsKey(workspaceProject.FilePath))
            {
                WorkspaceProjectChanged(workspaceProject);
                return;
            }

            var snapshot = new DefaultProjectSnapshot(workspaceProject);
            _projects[workspaceProject.FilePath] = snapshot;

            // We expect new projects to always be dirty but the host project isn't yet set. We don't attempt to compute
            // any state until the project is initialized.
            //
            // We need to notify listeners about every project add.
            NotifyListeners(new ProjectChangeEventArgs(snapshot, ProjectChangeKind.Added));
        }

        public override void WorkspaceProjectChanged(Project workspaceProject)
        {
            if (workspaceProject == null)
            {
                throw new ArgumentNullException(nameof(workspaceProject));
            }

            if (_projects.TryGetValue(workspaceProject.FilePath, out var original))
            {
                // Doing an update to the project should keep computed values, but mark the project as dirty if the
                // underlying project is newer.
                var snapshot = original.WithWorkspaceProject(workspaceProject);
                _projects[workspaceProject.FilePath] = snapshot;

                if (snapshot.IsInitialized && snapshot.IsDirty)
                {
                    // We don't need to notify listeners yet because we don't have any **new** computed state. However we do 
                    // need to trigger the background work to asynchronously compute the effect of the updates.
                    NotifyBackgroundWorker(snapshot.WorkspaceProject);
                }
            }
        }

        public override void WorkspaceProjectRemoved(Project workspaceProject)
        {
            if (workspaceProject == null)
            {
                throw new ArgumentNullException(nameof(workspaceProject));
            }
            
            if (_projects.TryGetValue(workspaceProject.FilePath, out var snapshot))
            {
                snapshot = snapshot.RemoveWorkspaceProject();
                _projects[workspaceProject.FilePath] = snapshot;

                if (snapshot.IsUnloaded)
                {
                    _projects.Remove(workspaceProject.FilePath);

                    // We need to notify listeners about every project removal.
                    NotifyListeners(new ProjectChangeEventArgs(snapshot, ProjectChangeKind.Removed));
                }
            }
        }

        public override void WorkspaceProjectsCleared()
        {
            foreach (var kvp in _projects.ToArray())
            {
                if (kvp.Value.WorkspaceProject != null)
                {
                    WorkspaceProjectRemoved(kvp.Value.WorkspaceProject);
                }
            }
        }

        // virtual so it can be overridden in tests
        protected virtual void NotifyBackgroundWorker(Project project)
        {
            _workerQueue.Enqueue(project);
        }

        // virtual so it can be overridden in tests
        protected virtual void NotifyListeners(ProjectChangeEventArgs e)
        {
            var handler = Changed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public override void ReportError(Exception exception)
        {
            _errorReporter.ReportError(exception);
        }

        public override void ReportError(Exception exception, Project project)
        {
            _errorReporter.ReportError(exception, project);
        }
    }
}
