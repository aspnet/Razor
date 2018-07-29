﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Text;
using Microsoft.Extensions.Internal;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    // The implementation of project snapshot manager abstracts over the Roslyn Project (WorkspaceProject)
    // and information from the host's underlying project system (HostProject), to provide a unified and
    // immutable view of the underlying project systems.
    //
    // The HostProject support all of the configuration that the Razor SDK exposes via the project system
    // (language version, extensions, named configuration).
    //
    // The WorkspaceProject is needed to support our use of Roslyn Compilations for Tag Helpers and other
    // C# based constructs.
    //
    // The implementation will create a ProjectSnapshot for each HostProject. Put another way, when we
    // see a WorkspaceProject get created, we only care if we already have a HostProject for the same
    // filepath.
    //
    // Our underlying HostProject infrastructure currently does not handle multiple TFMs (project with
    // $(TargetFrameworks), so we just bind to the first WorkspaceProject we see for each HostProject.
    internal class DefaultProjectSnapshotManager : ProjectSnapshotManagerBase
    {
        public override event EventHandler<ProjectChangeEventArgs> Changed;

        private readonly ErrorReporter _errorReporter;
        private readonly ForegroundDispatcher _foregroundDispatcher;
        private readonly ProjectSnapshotChangeTrigger[] _triggers;

        // Each entry holds a ProjectState and an optional ProjectSnapshot. ProjectSnapshots are
        // created lazily.
        private readonly Dictionary<ProjectId, Entry> _projects;
        private readonly HashSet<string> _openDocuments;

        public DefaultProjectSnapshotManager(
            ForegroundDispatcher foregroundDispatcher,
            ErrorReporter errorReporter,
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
            _triggers = triggers.ToArray();
            Workspace = workspace;

            _projects = new Dictionary<ProjectId, Entry>();
            _openDocuments = new HashSet<string>(FilePathComparer.Instance);

            for (var i = 0; i < _triggers.Length; i++)
            {
                _triggers[i].Initialize(this);
            }
        }

        public override IReadOnlyList<ProjectSnapshot> Projects
        {
            get
            {
                _foregroundDispatcher.AssertForegroundThread();

                var i = 0;
                var projects = new ProjectSnapshot[_projects.Count];
                foreach (var entry in _projects)
                {
                    if (entry.Value.Snapshot == null)
                    {
                        entry.Value.Snapshot = new DefaultProjectSnapshot(entry.Value.State);
                    }

                    projects[i++] = entry.Value.Snapshot;
                }

                return projects;
            }
        }

        public override Workspace Workspace { get; }

        public override ProjectSnapshot GetLoadedProject(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            _foregroundDispatcher.AssertForegroundThread();

            if (TryGetEntryFromFilePath(filePath, out var entry))
            {
                if (entry.Snapshot == null)
                {
                    entry.Snapshot = new DefaultProjectSnapshot(entry.State);
                }

                return entry.Snapshot;
            }

            return null;
        }

        public override ProjectSnapshot GetLoadedProject(ProjectId projectId)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            _foregroundDispatcher.AssertForegroundThread();

            if (_projects.TryGetValue(projectId, out var entry))
            {
                if (entry.Snapshot == null)
                {
                    entry.Snapshot = new DefaultProjectSnapshot(entry.State);
                }

                return entry.Snapshot;
            }

            return null;
        }

        public override ProjectSnapshot GetOrCreateProject(string filePath)
        {
            _foregroundDispatcher.AssertForegroundThread();

            if (filePath == null)
            {
                return new MiscellaneousProjectSnapshot(Workspace.Services);
            }

            return GetLoadedProject(filePath) ?? new EphemeralProjectSnapshot(Workspace.Services, filePath);
        }

        public override bool IsDocumentOpen(string documentFilePath)
        {
            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            _foregroundDispatcher.AssertForegroundThread();

            return _openDocuments.Contains(documentFilePath);
        }

        public override void DocumentAdded(HostProject hostProject, HostDocument document, TextLoader textLoader)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            _foregroundDispatcher.AssertForegroundThread();

            if (_projects.TryGetValue(hostProject.Id, out var entry))
            {
                var loader = textLoader == null ? DocumentState.EmptyLoader : (Func<Task<TextAndVersion>>)(() =>
                {
                    return textLoader.LoadTextAndVersionAsync(Workspace, null, CancellationToken.None);
                });
                var state = entry.State.WithAddedHostDocument(document, loader);

                // Document updates can no-op.
                if (!object.ReferenceEquals(state, entry.State))
                {
                    _projects[hostProject.Id] = new Entry(state);
                    NotifyListeners(new ProjectChangeEventArgs(hostProject.Id, document.FilePath, ProjectChangeKind.DocumentAdded));
                }
            }
        }

        public override void DocumentRemoved(HostProject hostProject, HostDocument document)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            _foregroundDispatcher.AssertForegroundThread();
            if (_projects.TryGetValue(hostProject.Id, out var entry))
            {
                var state = entry.State.WithRemovedHostDocument(document);

                // Document updates can no-op.
                if (!object.ReferenceEquals(state, entry.State))
                {
                    _projects[hostProject.Id] = new Entry(state);
                    NotifyListeners(new ProjectChangeEventArgs(hostProject.Id, document.FilePath, ProjectChangeKind.DocumentRemoved));
                }
            }
        }

        public override void DocumentOpened(ProjectId projectId, string documentFilePath, SourceText sourceText)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            if (sourceText == null)
            {
                throw new ArgumentNullException(nameof(sourceText));
            }

            _foregroundDispatcher.AssertForegroundThread();
            if (_projects.TryGetValue(projectId, out var entry) &&
                entry.State.Documents.TryGetValue(documentFilePath, out var older))
            {
                ProjectState state;
                SourceText olderText;
                VersionStamp olderVersion;

                var currentText = sourceText;
                if (older.TryGetText(out olderText) &&
                    older.TryGetTextVersion(out olderVersion))
                {
                    var version = currentText.ContentEquals(olderText) ? olderVersion : olderVersion.GetNewerVersion();
                    state = entry.State.WithChangedHostDocument(older.HostDocument, currentText, version);
                }
                else
                {
                    state = entry.State.WithChangedHostDocument(older.HostDocument, async () =>
                    {
                        olderText = await older.GetTextAsync().ConfigureAwait(false);
                        olderVersion = await older.GetTextVersionAsync().ConfigureAwait(false);

                        var version = currentText.ContentEquals(olderText) ? olderVersion : olderVersion.GetNewerVersion();
                        return TextAndVersion.Create(currentText, version, documentFilePath);
                    });
                }

                _openDocuments.Add(documentFilePath);

                // Document updates can no-op.
                if (!object.ReferenceEquals(state, entry.State))
                {
                    _projects[projectId] = new Entry(state);
                    NotifyListeners(new ProjectChangeEventArgs(projectId, documentFilePath, ProjectChangeKind.DocumentChanged));
                }
            }
        }

        public override void DocumentClosed(ProjectId projectId, string documentFilePath, TextLoader textLoader)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            if (textLoader == null)
            {
                throw new ArgumentNullException(nameof(textLoader));
            }

            _foregroundDispatcher.AssertForegroundThread();
            if (_projects.TryGetValue(projectId, out var entry) &&
                entry.State.Documents.TryGetValue(documentFilePath, out var older))
            {
                var state = entry.State.WithChangedHostDocument(older.HostDocument, async () =>
                {
                    return await textLoader.LoadTextAndVersionAsync(Workspace, default, default);
                });

                _openDocuments.Remove(documentFilePath);

                // Document updates can no-op.
                if (!object.ReferenceEquals(state, entry.State))
                {
                    _projects[projectId] = new Entry(state);
                    NotifyListeners(new ProjectChangeEventArgs(projectId, documentFilePath, ProjectChangeKind.DocumentChanged));
                }
            }
        }

        public override void DocumentChanged(ProjectId projectId, string documentFilePath, SourceText sourceText)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            if (sourceText == null)
            {
                throw new ArgumentNullException(nameof(sourceText));
            }

            _foregroundDispatcher.AssertForegroundThread();
            if (_projects.TryGetValue(projectId, out var entry) &&
                entry.State.Documents.TryGetValue(documentFilePath, out var older))
            {
                ProjectState state;
                SourceText olderText;
                VersionStamp olderVersion;

                var currentText = sourceText;
                if (older.TryGetText(out olderText) &&
                    older.TryGetTextVersion(out olderVersion))
                {
                    var version = currentText.ContentEquals(olderText) ? olderVersion : olderVersion.GetNewerVersion();
                    state = entry.State.WithChangedHostDocument(older.HostDocument, currentText, version);
                }
                else
                {
                    state = entry.State.WithChangedHostDocument(older.HostDocument, async () =>
                    {
                        olderText = await older.GetTextAsync().ConfigureAwait(false);
                        olderVersion = await older.GetTextVersionAsync().ConfigureAwait(false);

                        var version = currentText.ContentEquals(olderText) ? olderVersion : olderVersion.GetNewerVersion();
                        return TextAndVersion.Create(currentText, version, documentFilePath);
                    });
                }

                // Document updates can no-op.
                if (!object.ReferenceEquals(state, entry.State))
                {
                    _projects[projectId] = new Entry(state);
                    NotifyListeners(new ProjectChangeEventArgs(projectId, documentFilePath, ProjectChangeKind.DocumentChanged));
                }
            }
        }

        public override void DocumentChanged(ProjectId projectId, string documentFilePath, TextLoader textLoader)
        {
            if (projectId == null)
            {
                throw new ArgumentNullException(nameof(projectId));
            }

            if (documentFilePath == null)
            {
                throw new ArgumentNullException(nameof(documentFilePath));
            }

            if (textLoader == null)
            {
                throw new ArgumentNullException(nameof(textLoader));
            }

            _foregroundDispatcher.AssertForegroundThread();
            if (_projects.TryGetValue(projectId, out var entry) &&
                entry.State.Documents.TryGetValue(documentFilePath, out var older))
            {
                var state = entry.State.WithChangedHostDocument(older.HostDocument, async () =>
                {
                    return await textLoader.LoadTextAndVersionAsync(Workspace, default, default);
                });

                // Document updates can no-op.
                if (!object.ReferenceEquals(state, entry.State))
                {
                    _projects[projectId] = new Entry(state);
                    NotifyListeners(new ProjectChangeEventArgs(projectId, documentFilePath, ProjectChangeKind.DocumentChanged));
                }
            }
        }

        public override void HostProjectAdded(HostProject hostProject)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            _foregroundDispatcher.AssertForegroundThread();

            // We don't expect to see a HostProject initialized multiple times for the same path. Just ignore it.
            if (_projects.ContainsKey(hostProject.Id))
            {
                return;
            }

            // It's possible that Workspace has already created a project for this, but it's not deterministic
            // So if possible find a WorkspaceProject.
            var workspaceProject = GetWorkspaceProject(hostProject.FilePath);

            var state = ProjectState.Create(Workspace.Services, hostProject, workspaceProject);
            _projects[hostProject.Id] = new Entry(state);

            // We need to notify listeners about every project add.
            NotifyListeners(new ProjectChangeEventArgs(hostProject.Id, ProjectChangeKind.ProjectAdded));
        }

        public override void HostProjectChanged(HostProject hostProject)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            _foregroundDispatcher.AssertForegroundThread();

            if (_projects.TryGetValue(hostProject.Id, out var entry))
            {
                var state = entry.State.WithHostProject(hostProject);

                // HostProject updates can no-op.
                if (!object.ReferenceEquals(state, entry.State))
                {
                    _projects[hostProject.Id] = new Entry(state);

                    NotifyListeners(new ProjectChangeEventArgs(hostProject.Id, ProjectChangeKind.ProjectChanged));
                }
            }
        }

        public override void HostProjectRemoved(HostProject hostProject)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            _foregroundDispatcher.AssertForegroundThread();

            if (_projects.TryGetValue(hostProject.Id, out var snapshot))
            {
                _projects.Remove(hostProject.Id);

                // We need to notify listeners about every project removal.
                NotifyListeners(new ProjectChangeEventArgs(hostProject.Id, ProjectChangeKind.ProjectRemoved));
            }
        }

        public override void WorkspaceProjectAdded(Project workspaceProject)
        {
            if (workspaceProject == null)
            {
                throw new ArgumentNullException(nameof(workspaceProject));
            }

            _foregroundDispatcher.AssertForegroundThread();

            if (!IsSupportedWorkspaceProject(workspaceProject))
            {
                return;
            }
            // The WorkspaceProject initialization never triggers a "Project Add" from out point of view, we
            // only care if the new WorkspaceProject matches an existing HostProject.
            if (TryGetEntryFromFilePath(workspaceProject.FilePath, out var entry))
            {
                // If this is a multi-targeting project then we are only interested in a single workspace project. If we already
                // found one in the past just ignore this one.
                if (entry.State.WorkspaceProject == null)
                {
                    var state = entry.State.WithWorkspaceProject(workspaceProject);
                    _projects[state.Id] = new Entry(state);

                    NotifyListeners(new ProjectChangeEventArgs(state.Id, ProjectChangeKind.ProjectChanged));
                }
            }
        }

        public override void WorkspaceProjectChanged(Project workspaceProject)
        {
            if (workspaceProject == null)
            {
                throw new ArgumentNullException(nameof(workspaceProject));
            }

            _foregroundDispatcher.AssertForegroundThread();

            if (!IsSupportedWorkspaceProject(workspaceProject))
            {
                return;
            }

            // We also need to check the projectId here. If this is a multi-targeting project then we are only interested
            // in a single workspace project. Just use the one that showed up first.
            if (TryGetEntryFromFilePath(workspaceProject.FilePath, out var entry) &&
                (entry.State.WorkspaceProject == null || entry.State.WorkspaceProject.Id == workspaceProject.Id) &&
                (entry.State.WorkspaceProject == null || entry.State.WorkspaceProject.Version.GetNewerVersion(workspaceProject.Version) == workspaceProject.Version))
            {
                var state = entry.State.WithWorkspaceProject(workspaceProject);

                // WorkspaceProject updates can no-op. This can be the case if a build is triggered, but we've
                // already seen the update.
                if (!object.ReferenceEquals(state, entry.State))
                {
                    _projects[state.Id] = new Entry(state);

                    NotifyListeners(new ProjectChangeEventArgs(state.Id, ProjectChangeKind.ProjectChanged));
                }
            }
        }

        public override void WorkspaceProjectRemoved(Project workspaceProject)
        {
            if (workspaceProject == null)
            {
                throw new ArgumentNullException(nameof(workspaceProject));
            }

            _foregroundDispatcher.AssertForegroundThread();

            if (!IsSupportedWorkspaceProject(workspaceProject))
            {
                return;
            }

            if (TryGetEntryFromFilePath(workspaceProject.FilePath, out var entry))
            {
                // We also need to check the projectId here. If this is a multi-targeting project then we are only interested
                // in a single workspace project. Make sure the WorkspaceProject we're using is the one that's being removed.
                if (entry.State.WorkspaceProject?.Id != workspaceProject.Id)
                {
                    return;
                }

                ProjectState state;

                // So if the WorkspaceProject got removed, we should double check to make sure that there aren't others
                // hanging around. This could happen if a project is multi-targeting and one of the TFMs is removed.
                var otherWorkspaceProject = GetWorkspaceProject(workspaceProject.FilePath);
                if (otherWorkspaceProject != null && otherWorkspaceProject.Id != workspaceProject.Id)
                {
                    // OK there's another WorkspaceProject, use that.
                    state = entry.State.WithWorkspaceProject(otherWorkspaceProject);
                    _projects[state.Id] = new Entry(state);

                    NotifyListeners(new ProjectChangeEventArgs(state.Id, ProjectChangeKind.ProjectChanged));
                }
                else
                {
                    state = entry.State.WithWorkspaceProject(null);
                    _projects[state.Id] = new Entry(state);

                    // Notify listeners of a change because we've removed computed state.
                    NotifyListeners(new ProjectChangeEventArgs(state.Id, ProjectChangeKind.ProjectChanged));
                }
            }
        }

        public override void ReportError(Exception exception)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _errorReporter.ReportError(exception);
        }

        public override void ReportError(Exception exception, ProjectSnapshot project)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _errorReporter.ReportError(exception, project);
        }

        public override void ReportError(Exception exception, HostProject hostProject)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            var snapshot = hostProject?.FilePath == null ? null : GetLoadedProject(hostProject.FilePath);
            _errorReporter.ReportError(exception, snapshot);
        }

        public override void ReportError(Exception exception, Project workspaceProject)
        {
            if (exception == null)
            {
                throw new ArgumentNullException(nameof(exception));
            }

            _errorReporter.ReportError(exception, workspaceProject);
        }

        // We're only interested in CSharp projects that have a FilePath. We rely on the FilePath to
        // unify the Workspace Project with our HostProject concept.
        private bool IsSupportedWorkspaceProject(Project workspaceProject) => workspaceProject.Language == LanguageNames.CSharp && workspaceProject.FilePath != null;

        private Project GetWorkspaceProject(string filePath)
        {
            var solution = Workspace.CurrentSolution;
            if (solution == null)
            {
                return null;
            }

            foreach (var workspaceProject in solution.Projects)
            {
                if (IsSupportedWorkspaceProject(workspaceProject) &&
                    FilePathComparer.Instance.Equals(filePath, workspaceProject.FilePath))
                {
                    // We don't try to handle mulitple TFMs anwhere in Razor, just take the first WorkspaceProject that is a match. 
                    return workspaceProject;
                }
            }

            return null;
        }

        // virtual so it can be overridden in tests
        protected virtual void NotifyListeners(ProjectChangeEventArgs e)
        {
            _foregroundDispatcher.AssertForegroundThread();

            var handler = Changed;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private bool TryGetEntryFromFilePath(string filePath, out Entry entry)
        {
            foreach (var item in _projects)
            {
                if (FilePathComparer.Instance.Equals(item.Value.State.HostProject.FilePath, filePath))
                {
                    entry = item.Value;
                    return true;
                }
            }

            entry = null;
            return false;
        }

        private class Entry
        {
            public ProjectSnapshot Snapshot;
            public readonly ProjectState State;

            public Entry(ProjectState state)
            {
                State = state;
            }
        }
    }
}