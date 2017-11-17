// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Editor.Razor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    [Export(typeof(VisualStudioOpenDocumentManager))]
    internal class DefaultVisualStudioOpenDocumentManager : VisualStudioOpenDocumentManager
    {
        private readonly IVsFileChangeEx _fileChangeService;
        private readonly IVsRunningDocumentTable _runningDocumentTable;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private readonly ForegroundDispatcher _foregroundDispatcher;
        private readonly ErrorReporter _errorReporter;

        private List<VisualStudioDocumentTracker> _documents;
        private Dictionary<string, ViewImportChangeTracker> _viewImportChangeTrackerCache;

        [ImportingConstructor]
        public DefaultVisualStudioOpenDocumentManager(
            [Import(typeof(SVsServiceProvider))] IServiceProvider serviceProvider,
            [Import(typeof(VisualStudioWorkspace))] Workspace workspace,
            IVsEditorAdaptersFactoryService editorAdaptersFactoryService)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            if (workspace == null)
            {
                throw new ArgumentNullException(nameof(workspace));
            }

            if (editorAdaptersFactoryService == null)
            {
                throw new ArgumentNullException(nameof(editorAdaptersFactoryService));
            }

            _fileChangeService = serviceProvider.GetService(typeof(SVsFileChangeEx)) as IVsFileChangeEx;
            _runningDocumentTable = serviceProvider.GetService(typeof(SVsRunningDocumentTable)) as IVsRunningDocumentTable;
            _editorAdaptersFactoryService = editorAdaptersFactoryService;

            _documents = new List<VisualStudioDocumentTracker>();
            _viewImportChangeTrackerCache = new Dictionary<string, ViewImportChangeTracker>(StringComparer.OrdinalIgnoreCase);

            _foregroundDispatcher = workspace.Services.GetRequiredService<ForegroundDispatcher>();
            _errorReporter = workspace.Services.GetRequiredService<ErrorReporter>();
        }

        public override IReadOnlyList<VisualStudioDocumentTracker> OpenDocuments => _documents;

        public override void AddDocument(VisualStudioDocumentTracker tracker)
        {
            _foregroundDispatcher.AssertForegroundThread();

            if (!_documents.Contains(tracker))
            {
                _documents.Add(tracker);

                if (!tracker.TextBuffer.Properties.TryGetProperty<VisualStudioRazorParser>(typeof(VisualStudioRazorParser), out var parser))
                {
                    // The document tracker doesn't have a corresponding Razor parser. This should never be the case.
                    return;
                }

                EnsureUndisposedTrackers();

                var imports = tracker.Imports;
                foreach (var import in imports)
                {
                    var importFilePath = import.FilePath;
                    if (importFilePath == null)
                    {
                        // This is probably an in-memory view import. We can't track it.
                        continue;
                    }

                    ViewImportChangeTracker viewImportChangeTracker;
                    if (!_viewImportChangeTrackerCache.ContainsKey(importFilePath))
                    {
                        // First time seeing this view import. Create a change tracker for it.
                        viewImportChangeTracker = new ViewImportChangeTracker(
                            _fileChangeService,
                            _runningDocumentTable,
                            _editorAdaptersFactoryService,
                            _foregroundDispatcher,
                            _errorReporter,
                            importFilePath);

                        _viewImportChangeTrackerCache[importFilePath] = viewImportChangeTracker;
                    }
                    else
                    {
                        viewImportChangeTracker = _viewImportChangeTrackerCache[importFilePath];
                    }

                    // We want the current document to be reparsed when this import is changed.
                    viewImportChangeTracker.AddAssociatedParser(parser);
                }
            }
        }

        public override void RemoveDocument(VisualStudioDocumentTracker tracker)
        {
            _foregroundDispatcher.AssertForegroundThread();

            if (_documents.Contains(tracker))
            {
                _documents.Remove(tracker);
            }

            if (!tracker.TextBuffer.Properties.TryGetProperty<VisualStudioRazorParser>(typeof(VisualStudioRazorParser), out var parser))
            {
                // The document tracker doesn't have a corresponding Razor parser. This should never be the case.
                return;
            }

            EnsureUndisposedTrackers();

            var currentDocumentFilePath = tracker.FilePath;
            var imports = tracker.Imports;
            foreach (var import in imports)
            {
                var importFilePath = import.FilePath;
                if (importFilePath == null)
                {
                    // This is probably an in-memory view import. We can't track it.
                    continue;
                }

                if (!_viewImportChangeTrackerCache.ContainsKey(importFilePath))
                {
                    // We were never tracking this file to begin with.
                    continue;
                }
                var viewImportChangeTracker = _viewImportChangeTrackerCache[importFilePath];

                // This document is being closed. It no longer needs to be reparsed.
                viewImportChangeTracker.RemoveAssociatedParser(parser);
            }
        }

        private void EnsureUndisposedTrackers()
        {
            foreach (var key in _viewImportChangeTrackerCache.Keys.ToList())
            {
                var value = _viewImportChangeTrackerCache[key];
                if (value.IsDisposed)
                {
                    // This could happen if the file was deleted or renamed.
                    _viewImportChangeTrackerCache.Remove(key);
                }
            }
        }
    }
}
