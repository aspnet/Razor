﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    // Internal tracker for DefaultProjectSnapshot
    internal class ProjectState
    {
        private static readonly ImmutableDictionary<string, DocumentState> EmptyDocuments = ImmutableDictionary.Create<string, DocumentState>(FilePathComparer.Instance);

        private readonly object _lock;
        
        private ProjectEngineTracker _projectEngine;
        private ProjectTagHelperTracker _tagHelpers;

        public static ProjectState Create(HostWorkspaceServices services, HostProject hostProject, Project workspaceProject = null)
        {
            if (services == null)
            {
                throw new ArgumentNullException(nameof(services));
            }

            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            return new ProjectState(services, hostProject, workspaceProject);
        }
        
        private ProjectState(
            HostWorkspaceServices services,
            HostProject hostProject,
            Project workspaceProject)
        {
            Services = services;
            HostProject = hostProject;
            WorkspaceProject = workspaceProject;
            Documents = EmptyDocuments;
            Version = VersionStamp.Create();

            _lock = new object();
        }

        private ProjectState(
            ProjectState older,
            ProjectDifference difference,
            HostProject hostProject,
            Project workspaceProject,
            ImmutableDictionary<string, DocumentState> documents)
        {
            if (older == null)
            {
                throw new ArgumentNullException(nameof(older));
            }

            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }

            Services = older.Services;
            Version = older.Version.GetNewerVersion();

            HostProject = hostProject;
            WorkspaceProject = workspaceProject;
            Documents = documents;

            _lock = new object();

            _projectEngine = older._projectEngine?.ForkFor(this, difference);
            _tagHelpers = older._tagHelpers?.ForkFor(this, difference);
        }

        // Internal set for testing.
        public ImmutableDictionary<string, DocumentState> Documents { get; internal set; }

        public HostProject HostProject { get; }

        public HostWorkspaceServices Services { get; }

        public Project WorkspaceProject { get; }

        public VersionStamp Version { get; }

        // Computed State
        public ProjectEngineTracker ProjectEngine
        {
            get
            {
                if (_projectEngine == null)
                {
                    lock (_lock)
                    {
                        if (_projectEngine == null)
                        {
                            _projectEngine = new ProjectEngineTracker(this);
                        }
                    }
                }

                return _projectEngine;
            }
        }

        // Computed State
        public ProjectTagHelperTracker TagHelpers
        {
            get
            {
                if (_tagHelpers == null)
                {
                    lock (_lock)
                    {
                        if (_tagHelpers == null)
                        {
                            _tagHelpers = new ProjectTagHelperTracker(this);
                        }
                    }
                }

                return _tagHelpers;
            }
        }

        public ProjectState WithAddedHostDocument(HostDocument hostDocument, Func<Task<TextAndVersion>> loader)
        {
            if (hostDocument == null)
            {
                throw new ArgumentNullException(nameof(hostDocument));
            }

            if (loader == null)
            {
                throw new ArgumentNullException(nameof(loader));
            }

            // Ignore attempts to 'add' a document with different data, we only
            // care about one, so it might as well be the one we have.
            if (Documents.ContainsKey(hostDocument.FilePath))
            {
                return this;
            }
            
            var documents = Documents.Add(hostDocument.FilePath, DocumentState.Create(Services, hostDocument, loader));
            var state = new ProjectState(this, ProjectDifference.DocumentAdded, HostProject, WorkspaceProject, documents);
            return state;
        }

        public ProjectState WithRemovedHostDocument(HostDocument hostDocument)
        {
            if (hostDocument == null)
            {
                throw new ArgumentNullException(nameof(hostDocument));
            }

            if (!Documents.ContainsKey(hostDocument.FilePath))
            {
                return this;
            }
            
            var documents = Documents.Remove(hostDocument.FilePath);
            var state = new ProjectState(this, ProjectDifference.DocumentRemoved, HostProject, WorkspaceProject, documents);
            return state;
        }

        public ProjectState WithChangedHostDocument(HostDocument hostDocument, SourceText sourceText, VersionStamp version)
        {
            if (hostDocument == null)
            {
                throw new ArgumentNullException(nameof(hostDocument));
            }

            if (!Documents.TryGetValue(hostDocument.FilePath, out var document))
            {
                return this;
            }

            var documents = Documents.SetItem(hostDocument.FilePath, document.WithText(sourceText, version));
            var state = new ProjectState(this, ProjectDifference.DocumentChanged, HostProject, WorkspaceProject, documents);
            return state;
        }

        public ProjectState WithChangedHostDocument(HostDocument hostDocument, Func<Task<TextAndVersion>> loader)
        {
            if (hostDocument == null)
            {
                throw new ArgumentNullException(nameof(hostDocument));
            }

            if (!Documents.TryGetValue(hostDocument.FilePath, out var document))
            {
                return this;
            }

            var documents = Documents.SetItem(hostDocument.FilePath, document.WithTextLoader(loader));
            var state = new ProjectState(this, ProjectDifference.DocumentChanged, HostProject, WorkspaceProject, documents);
            return state;
        }

        public ProjectState WithHostProject(HostProject hostProject)
        {
            if (hostProject == null)
            {
                throw new ArgumentNullException(nameof(hostProject));
            }

            if (HostProject.Configuration.Equals(hostProject.Configuration))
            {
                return this;
            }
            
            var documents = Documents.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.WithConfigurationChange(), FilePathComparer.Instance);
            var state = new ProjectState(this, ProjectDifference.ConfigurationChanged, hostProject, WorkspaceProject, documents);
            return state;
        }

        public ProjectState WithWorkspaceProject(Project workspaceProject)
        {
            var difference = ProjectDifference.None;
            if (WorkspaceProject == null && workspaceProject != null)
            {
                difference |= ProjectDifference.WorkspaceProjectAdded;
            }
            else if (WorkspaceProject != null && workspaceProject == null)
            {
                difference |= ProjectDifference.WorkspaceProjectRemoved;
            }
            else
            {
                // We always update the snapshot right now when the project changes. This is how
                // we deal with changes to the content of C# sources.
                difference |= ProjectDifference.WorkspaceProjectChanged;
            }

            if (difference == ProjectDifference.None)
            {
                return this;
            }

            var documents = Documents.ToImmutableDictionary(kvp => kvp.Key, kvp => kvp.Value.WithWorkspaceProjectChange(), FilePathComparer.Instance);
            var state = new ProjectState(this, difference, HostProject, workspaceProject, documents);
            return state;
        }
    }
}
