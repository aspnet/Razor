// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.Editor.Razor
{
    [System.Composition.Shared]
    [Export(typeof(RazorEditorFactoryService))]
    internal class DefaultRazorEditorFactoryService : RazorEditorFactoryService
    {
        private static readonly object RazorTextBufferInitializationKey = new object();

        private readonly WorkspaceProvider _workspaceProvider;

        [ImportingConstructor]
        public DefaultRazorEditorFactoryService(WorkspaceProvider workspaceProvider)
        {
            if (workspaceProvider == null)
            {
                throw new ArgumentNullException(nameof(workspaceProvider));
            }

            _workspaceProvider = workspaceProvider;
        }

        public override bool TryGetDocumentTracker(ITextBuffer textBuffer, out VisualStudioDocumentTracker documentTracker)
        {
            if (textBuffer == null)
            {
                throw new ArgumentNullException(nameof(textBuffer));
            }

            if (!textBuffer.IsRazorBuffer())
            {
                documentTracker = null;
                return false;
            }

            EnsureTextBufferInitialized(textBuffer);

            if (!textBuffer.Properties.TryGetProperty(typeof(VisualStudioDocumentTracker), out documentTracker))
            {
                Debug.Fail("Document tracker should have been stored on the text buffer during initialization.");
                return false;
            }

            return true;
        }

        public override bool TryGetParser(ITextBuffer textBuffer, out VisualStudioRazorParser parser)
        {
            if (textBuffer == null)
            {
                throw new ArgumentNullException(nameof(textBuffer));
            }

            if (!textBuffer.IsRazorBuffer())
            {
                parser = null;
                return false;
            }

            EnsureTextBufferInitialized(textBuffer);

            if (!textBuffer.Properties.TryGetProperty(typeof(VisualStudioRazorParser), out parser))
            {
                Debug.Fail("Parser should have been stored on the text buffer during initialization.");
                return false;
            }

            return true;
        }

        internal override bool TryGetSmartIndenter(ITextBuffer textBuffer, out BraceSmartIndenter braceSmartIndenter)
        {
            if (textBuffer == null)
            {
                throw new ArgumentNullException(nameof(textBuffer));
            }

            if (!textBuffer.IsRazorBuffer())
            {
                braceSmartIndenter = null;
                return false;
            }

            EnsureTextBufferInitialized(textBuffer);

            if (!textBuffer.Properties.TryGetProperty(typeof(BraceSmartIndenter), out braceSmartIndenter))
            {
                Debug.Fail("Brace smart indenter should have been stored on the text buffer during initialization.");
                return false;
            }

            return true;
        }

        // Internal for testing
        internal void EnsureTextBufferInitialized(ITextBuffer textBuffer)
        {
            if (textBuffer.Properties.ContainsProperty(RazorTextBufferInitializationKey))
            {
                // Buffer already initialized.
                return;
            }

            var workspace = _workspaceProvider.GetWorkspace(textBuffer);
            var razorLanguageServices = workspace.Services.GetLanguageServices(RazorLanguage.Name);
            var documentTrackerFactory = razorLanguageServices.GetRequiredService<VisualStudioDocumentTrackerFactory>();
            var parserFactory = razorLanguageServices.GetRequiredService<VisualStudioRazorParserFactory>();
            var braceSmartIndenterFactory = razorLanguageServices.GetRequiredService<BraceSmartIndenterFactory>();

            var tracker = documentTrackerFactory.Create(textBuffer);
            textBuffer.Properties[typeof(VisualStudioDocumentTracker)] = tracker;

            var parser = parserFactory.Create(tracker);
            textBuffer.Properties[typeof(VisualStudioRazorParser)] = parser;

            var braceSmartIndenter = braceSmartIndenterFactory.Create(tracker);
            textBuffer.Properties[typeof(BraceSmartIndenter)] = braceSmartIndenter;

            textBuffer.Properties.AddProperty(RazorTextBufferInitializationKey, RazorTextBufferInitializationKey);
        }
    }
}
