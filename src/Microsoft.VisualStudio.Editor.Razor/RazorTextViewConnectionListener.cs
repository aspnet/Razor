// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace Microsoft.VisualStudio.Editor.Razor
{
    [ContentType(RazorLanguage.CoreContentType)]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    [Export(typeof(ITextViewConnectionListener))]
    internal class RazorTextViewConnectionListener : ITextViewConnectionListener
    {
        private readonly ForegroundDispatcher _foregroundDispatcher;
        private readonly WorkspaceProvider _workspaceProvider;

        [ImportingConstructor]
        public RazorTextViewConnectionListener(ForegroundDispatcher foregroundDispatcher, WorkspaceProvider workspaceProvider)
        {
            if (foregroundDispatcher == null)
            {
                throw new ArgumentNullException(nameof(foregroundDispatcher));
            }

            if (workspaceProvider == null)
            {
                throw new ArgumentNullException(nameof(workspaceProvider));
            }

            _foregroundDispatcher = foregroundDispatcher;
            _workspaceProvider = workspaceProvider;
        }

        public void SubjectBuffersConnected(ITextView textView, ConnectionReason reason, IReadOnlyCollection<ITextBuffer> subjectBuffers)
        {
            if (textView == null)
            {
                throw new ArgumentException(nameof(textView));
            }

            if (subjectBuffers == null)
            {
                throw new ArgumentNullException(nameof(subjectBuffers));
            }

            _foregroundDispatcher.AssertForegroundThread();

            var workspace = _workspaceProvider.GetWorkspace(textView);
            var languageServices = workspace.Services.GetLanguageServices(RazorLanguage.Name);
            var documentManager = languageServices.GetRequiredService<RazorDocumentManager>();

            documentManager.OnTextViewOpened(textView, subjectBuffers);
        }

        public void SubjectBuffersDisconnected(ITextView textView, ConnectionReason reason, IReadOnlyCollection<ITextBuffer> subjectBuffers)
        {
            if (textView == null)
            {
                throw new ArgumentException(nameof(textView));
            }

            if (subjectBuffers == null)
            {
                throw new ArgumentNullException(nameof(subjectBuffers));
            }

            _foregroundDispatcher.AssertForegroundThread();

            var workspace = _workspaceProvider.GetWorkspace(textView);
            var languageServices = workspace.Services.GetLanguageServices(RazorLanguage.Name);
            var documentManager = languageServices.GetRequiredService<RazorDocumentManager>();

            documentManager.OnTextViewClosed(textView, subjectBuffers);
        }
    }
}
