// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Composition;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Text;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    [Export(typeof(IRazorEditorWorkerProvider))]
    public class RazorEditorWorkerProvider : IRazorEditorWorkerProvider
    {
        private readonly WorkspaceRegistrationMonitorProvider _workspaceProvider;
        private readonly WorkspaceMonitorProvider _workspaceMonitorProvider;

        public RazorEditorWorkerProvider()
        {
            _workspaceProvider = new WorkspaceRegistrationMonitorProvider();
            _workspaceMonitorProvider = new WorkspaceMonitorProvider();
        }

        public RazorEditorWorker GetWorker(ITextBuffer documentBuffer)
        {
            var roslynBuffer = GetRoslynBuffer(documentBuffer);
            var editorWorker = new RazorEditorWorker();

            var registrationMonitor = _workspaceProvider.GetRegistrationMonitor(roslynBuffer);
            registrationMonitor.ConnectedToWorkspace += workspace =>
            {
                var workspaceMonitor = _workspaceMonitorProvider.GetWorkspaceMonitor(workspace);

                workspaceMonitor.TagHelperFileChanged += OnTagHelpersChanged;
                workspaceMonitor.ReferencesChanged += OnTagHelpersChanged;
            };
            registrationMonitor.DisconnectedFromWorkspace += workspace =>
            {
                var workspaceMonitor = _workspaceMonitorProvider.StopMonitoring(workspace);

                workspaceMonitor.TagHelperFileChanged -= OnTagHelpersChanged;
                workspaceMonitor.ReferencesChanged -= OnTagHelpersChanged;
            };

            void OnTagHelpersChanged()
            {
                editorWorker.TagHelpersChanged?.Invoke();
            }

            return editorWorker;
        }

        private SourceTextContainer GetRoslynBuffer(ITextBuffer documentBuffer)
        {
            // Need to design a better approach of extracting the roslyn buffer from the document buffer.
            var roslynBufferProvider = (Func<SourceTextContainer>)documentBuffer.Properties["RoslynBufferProvider"];
            var roslynBuffer = roslynBufferProvider();

            return roslynBuffer;
        }
    }
}
