// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.CodeAnalysis.Razor
{
    public class WorkspaceMonitor
    {
        private readonly Workspace _workspace;

        public WorkspaceMonitor(Workspace workspace)
        {
            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            _workspace = workspace;

            _workspace.WorkspaceChanged += OnWorkspaceChanged;
        }

        public event Action TagHelperFileChanged;

        public event Action ReferencesChanged;

        private void OnWorkspaceChanged(object sender, WorkspaceChangeEventArgs args)
        {
            switch (args.Kind)
            {
                case WorkspaceChangeKind.DocumentAdded:
                case WorkspaceChangeKind.DocumentChanged:
                    var document = _workspace.CurrentSolution.GetDocument(args.DocumentId);
                    var filePath = document?.FilePath;

                    if (filePath != null &&
                        filePath.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) &&
                        filePath.IndexOf("TagHelper", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        TagHelperFileChanged?.Invoke();
                    }
                    break;

                case WorkspaceChangeKind.ProjectChanged:
                    var change = args.NewSolution.GetChanges(args.OldSolution);
                    foreach (var projectChange in change.GetProjectChanges())
                    {
                        if (projectChange.GetAddedMetadataReferences().Any() ||
                            projectChange.GetAddedProjectReferences().Any() ||
                            projectChange.GetRemovedMetadataReferences().Any() ||
                            projectChange.GetRemovedProjectReferences().Any())
                        {
                            ReferencesChanged?.Invoke();
                        }
                    }

                    break;
            }
        }
    }
}
