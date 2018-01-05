﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.Razor;

namespace Microsoft.VisualStudio.Editor.Razor
{
    internal class DefaultImportDocumentManager : ImportDocumentManager
    {
        private readonly FileChangeTrackerFactory _fileChangeTrackerFactory;
        private readonly ForegroundDispatcher _foregroundDispatcher;
        private readonly ErrorReporter _errorReporter;
        private readonly RazorProjectEngineFactoryService _projectEngineFactoryService;
        private readonly Dictionary<string, ImportTracker> _importTrackerCache;

        public override event EventHandler<ImportChangedEventArgs> Changed;

        public DefaultImportDocumentManager(
            ForegroundDispatcher foregroundDispatcher,
            ErrorReporter errorReporter,
            FileChangeTrackerFactory fileChangeTrackerFactory,
            RazorProjectEngineFactoryService projectEngineFactoryService)
        {
            if (foregroundDispatcher == null)
            {
                throw new ArgumentNullException(nameof(foregroundDispatcher));
            }

            if (errorReporter == null)
            {
                throw new ArgumentNullException(nameof(errorReporter));
            }

            if (fileChangeTrackerFactory == null)
            {
                throw new ArgumentNullException(nameof(fileChangeTrackerFactory));
            }

            if (projectEngineFactoryService == null)
            {
                throw new ArgumentNullException(nameof(projectEngineFactoryService));
            }

            _foregroundDispatcher = foregroundDispatcher;
            _errorReporter = errorReporter;
            _fileChangeTrackerFactory = fileChangeTrackerFactory;
            _projectEngineFactoryService = projectEngineFactoryService;
            _importTrackerCache = new Dictionary<string, ImportTracker>(StringComparer.OrdinalIgnoreCase);
        }

        public override void OnSubscribed(VisualStudioDocumentTracker tracker)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException(nameof(tracker));
            }

            _foregroundDispatcher.AssertForegroundThread();

            var imports = GetImportDocuments(tracker);
            foreach (var import in imports)
            {
                var importFilePath = import.FilePath;
                Debug.Assert(importFilePath != null);

                if (!_importTrackerCache.TryGetValue(importFilePath, out var importTracker))
                {
                    // First time seeing this import. Start tracking it.
                    var fileChangeTracker = _fileChangeTrackerFactory.Create(importFilePath);
                    importTracker = new ImportTracker(fileChangeTracker);
                    _importTrackerCache[importFilePath] = importTracker;

                    fileChangeTracker.Changed += FileChangeTracker_Changed;
                    fileChangeTracker.StartListening();
                }

                importTracker.AssociatedDocuments.Add(tracker.FilePath);
            }
        }

        public override void OnUnsubscribed(VisualStudioDocumentTracker tracker)
        {
            if (tracker == null)
            {
                throw new ArgumentNullException(nameof(tracker));
            }

            _foregroundDispatcher.AssertForegroundThread();

            var imports = GetImportDocuments(tracker);
            foreach (var import in imports)
            {
                var importFilePath = import.FilePath;
                Debug.Assert(importFilePath != null);

                if (_importTrackerCache.TryGetValue(importFilePath, out var importTracker))
                {
                    importTracker.AssociatedDocuments.Remove(tracker.FilePath);

                    if (importTracker.AssociatedDocuments.Count == 0)
                    {
                        // There are no open documents that care about this import. We no longer need to track it.
                        importTracker.FileChangeTracker.StopListening();
                        _importTrackerCache.Remove(importFilePath);
                    }
                }
            }
        }

        private IEnumerable<RazorProjectItem> GetImportDocuments(VisualStudioDocumentTracker tracker)
        {
            var projectDirectory = Path.GetDirectoryName(tracker.ProjectPath);
            var projectEngine = _projectEngineFactoryService.Create(projectDirectory, _ => { });
            var importItemFeature = projectEngine.Features.OfType<IRazorImportItemFeature>().FirstOrDefault();

            if (importItemFeature == null)
            {
                Debug.Fail("The import feature should always be available on the project engine.");
                return Enumerable.Empty<RazorProjectItem>();
            }
            var imports = importItemFeature.GetAssociatedImportItems(tracker.FilePath);

            return imports;
        }

        private void OnChanged(ImportTracker importTracker, FileChangeKind changeKind)
        {
            _foregroundDispatcher.AssertForegroundThread();

            if (Changed == null)
            {
                return;
            }

            var args = new ImportChangedEventArgs(importTracker.FilePath, changeKind, importTracker.AssociatedDocuments);
            Changed.Invoke(this, args);
    }

        private void FileChangeTracker_Changed(object sender, FileChangeEventArgs args)
        {
            _foregroundDispatcher.AssertForegroundThread();

            if (_importTrackerCache.TryGetValue(args.FilePath, out var importTracker))
            {
                OnChanged(importTracker, args.Kind);
            }
        }

        private class ImportTracker
        {
            public ImportTracker(FileChangeTracker fileChangeTracker)
            {
                FileChangeTracker = fileChangeTracker;
                AssociatedDocuments = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            }

            public string FilePath => FileChangeTracker.FilePath;

            public FileChangeTracker FileChangeTracker { get; }

            public HashSet<string> AssociatedDocuments { get; }
        }
    }
}
