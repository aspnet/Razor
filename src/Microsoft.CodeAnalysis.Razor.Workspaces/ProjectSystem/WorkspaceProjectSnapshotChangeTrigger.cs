﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Composition;
using System.Diagnostics;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    [Export(typeof(ProjectSnapshotChangeTrigger))]
    internal class WorkspaceProjectSnapshotChangeTrigger : ProjectSnapshotChangeTrigger
    {
        private ProjectSnapshotManagerBase _projectManager;

        public override void Initialize(ProjectSnapshotManagerBase projectManager)
        {
            _projectManager = projectManager;
            _projectManager.Workspace.WorkspaceChanged += Workspace_WorkspaceChanged;

            InitializeSolution(_projectManager.Workspace.CurrentSolution);
        }

        private void InitializeSolution(Solution solution)
        {
            Debug.Assert(solution != null);

            _projectManager.WorkspaceProjectsCleared();

            foreach (var project in solution.Projects)
            {
                if (project.Language == LanguageNames.CSharp)
                {
                    _projectManager.WorkspaceProjectAdded(project);
                }
            }
        }

        // Internal for testing
        internal void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            Project underlyingProject;
            switch (e.Kind)
            {
                case WorkspaceChangeKind.ProjectAdded:
                    {
                        underlyingProject = e.NewSolution.GetProject(e.ProjectId);
                        Debug.Assert(underlyingProject != null);

                        if (underlyingProject.Language == LanguageNames.CSharp)
                        {
                            _projectManager.WorkspaceProjectAdded(underlyingProject);
                        }
                        break;
                    }

                case WorkspaceChangeKind.ProjectChanged:
                case WorkspaceChangeKind.ProjectReloaded:
                    {
                        underlyingProject = e.NewSolution.GetProject(e.ProjectId);
                        Debug.Assert(underlyingProject != null);

                        _projectManager.WorkspaceProjectChanged(underlyingProject);
                        break;
                    }

                case WorkspaceChangeKind.ProjectRemoved:
                    {
                        underlyingProject = e.OldSolution.GetProject(e.ProjectId);
                        Debug.Assert(underlyingProject != null);

                        _projectManager.WorkspaceProjectRemoved(underlyingProject);
                        break;
                    }

                case WorkspaceChangeKind.SolutionAdded:
                case WorkspaceChangeKind.SolutionChanged:
                case WorkspaceChangeKind.SolutionCleared:
                case WorkspaceChangeKind.SolutionReloaded:
                case WorkspaceChangeKind.SolutionRemoved:
                    InitializeSolution(e.NewSolution);
                    break;
            }
        }
    }
}
