// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.LanguageServices.Razor.Editor;

namespace Microsoft.VisualStudio.LanguageServices.Razor.ProjectSystem
{
    [Export(typeof(ProjectSnapshotChangeTrigger))]
    internal class VsSolutionUpdatesProjectSnapshotChangeTrigger : ProjectSnapshotChangeTrigger
    {
        private readonly IServiceProvider _services;
        private readonly TextBufferProjectService _projectService;

        [ImportingConstructor]
        public VsSolutionUpdatesProjectSnapshotChangeTrigger(
            [Import(typeof(SVsServiceProvider))] IServiceProvider services,
            TextBufferProjectService projectService)
        {
            _services = services;
            _projectService = projectService;
        }

        public override void Initialize(ProjectSnapshotManagerBase projectManager)
        {
            var eventSink = new SolutionUpdateEventSink(projectManager, _projectService);

            // Attach the event sink to solution update events.
            var solutionBuildManager = _services.GetService(typeof(SVsSolutionBuildManager)) as IVsSolutionBuildManager;
            var result = solutionBuildManager.AdviseUpdateSolutionEvents(eventSink, out var cookie);
            Debug.Assert(result == VSConstants.S_OK);
        }

        // Internal for testing.
        internal class SolutionUpdateEventSink : IVsUpdateSolutionEvents2
        {
            private readonly ProjectSnapshotManagerBase _projectManager;
            private readonly TextBufferProjectService _projectService;

            public SolutionUpdateEventSink(ProjectSnapshotManagerBase projectManager, TextBufferProjectService projectService)
            {
                _projectManager = projectManager;
                _projectService = projectService;
            }

            public int UpdateSolution_Begin(ref int pfCancelUpdate)
            {
                return VSConstants.S_OK;
            }

            public int UpdateSolution_Done(int fSucceeded, int fModified, int fCancelCommand)
            {
                return VSConstants.S_OK;
            }

            public int UpdateSolution_StartUpdate(ref int pfCancelUpdate)
            {
                return VSConstants.S_OK;
            }

            public int UpdateSolution_Cancel()
            {
                return VSConstants.S_OK;
            }

            public int OnActiveProjectCfgChange(IVsHierarchy pIVsHierarchy)
            {
                return VSConstants.S_OK;
            }

            public int UpdateProjectCfg_Begin(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, ref int pfCancel)
            {
                return VSConstants.S_OK;
            }

            public int UpdateProjectCfg_Done(IVsHierarchy pHierProj, IVsCfg pCfgProj, IVsCfg pCfgSln, uint dwAction, int fSuccess, int fCancel)
            {
                var projectName = _projectService.GetProjectName(pHierProj);
                var projectPath = _projectService.GetProjectPath(pHierProj);

                // Get the corresponding roslyn project by matching the project name and the project path.
                foreach (var project in _projectManager.Workspace.CurrentSolution.Projects)
                {
                    if (string.Equals(projectName, project.Name, StringComparison.Ordinal) &&
                        string.Equals(projectPath, project.FilePath, StringComparison.Ordinal))
                    {
                        _projectManager.ProjectBuildComplete(project);
                        break;
                    }
                }

                return VSConstants.S_OK;
            }
        }
    }
}
