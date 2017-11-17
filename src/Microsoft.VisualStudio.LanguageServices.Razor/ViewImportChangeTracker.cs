// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.Editor;
using Microsoft.VisualStudio.Editor.Razor;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.TextManager.Interop;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    internal class ViewImportChangeTracker : IVsFileChangeEvents, IVsRunningDocTableEvents, IDisposable
    {
        private const uint FileChangeFlags = (uint)(_VSFILECHANGEFLAGS.VSFILECHG_Time | _VSFILECHANGEFLAGS.VSFILECHG_Size | _VSFILECHANGEFLAGS.VSFILECHG_Del | _VSFILECHANGEFLAGS.VSFILECHG_Add);

        private readonly IVsFileChangeEx _fileChangeService;
        private readonly IVsRunningDocumentTable _runningDocumentTable;
        private readonly IVsEditorAdaptersFactoryService _editorAdaptersFactoryService;
        private readonly ForegroundDispatcher _foregroundDispatcher;
        private readonly ErrorReporter _errorReporter;
        private readonly string _filePath;
        private readonly List<VisualStudioRazorParser> _associatedParsers;

        private uint _fileChangeCookie;
        private uint _runningDocumentTableCookie;
        private bool _disposed;

        public ViewImportChangeTracker(
            IVsFileChangeEx fileChangeService,
            IVsRunningDocumentTable runningDocumentTable,
            IVsEditorAdaptersFactoryService editorAdaptersFactoryService,
            ForegroundDispatcher foregroundDispatcher,
            ErrorReporter errorReporter,
            string filePath)
        {
            if (fileChangeService == null)
            {
                throw new ArgumentNullException(nameof(fileChangeService));
            }

            if (runningDocumentTable == null)
            {
                throw new ArgumentNullException(nameof(runningDocumentTable));
            }

            if (editorAdaptersFactoryService == null)
            {
                throw new ArgumentNullException(nameof(editorAdaptersFactoryService));
            }

            if (foregroundDispatcher == null)
            {
                throw new ArgumentNullException(nameof(foregroundDispatcher));
            }

            if (errorReporter == null)
            {
                throw new ArgumentNullException(nameof(errorReporter));
            }

            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            _fileChangeService = fileChangeService;
            _runningDocumentTable = runningDocumentTable;
            _editorAdaptersFactoryService = editorAdaptersFactoryService;
            _foregroundDispatcher = foregroundDispatcher;
            _errorReporter = errorReporter;
            _filePath = filePath;

            _fileChangeCookie = VSConstants.VSCOOKIE_NIL;
            _runningDocumentTableCookie = VSConstants.VSCOOKIE_NIL;
            _associatedParsers = new List<VisualStudioRazorParser>();
        }

        public string FilePath => _filePath;

        public bool IsDisposed => _disposed;

        public void AddAssociatedParser(VisualStudioRazorParser parser)
        {
            _associatedParsers.Add(parser);

            // This should no-op if we are already listening for changes to this view import.
            StartListeningForChanges();
        }

        public void RemoveAssociatedParser(VisualStudioRazorParser parser)
        {
            _associatedParsers.Remove(parser);

            if (_associatedParsers.Count == 0)
            {
                // There are no open documents that care about changes to this import.
                StopListeningForChanges();
            }
        }

        #region IVsFileChangeEvents
        public int FilesChanged(uint cChanges, string[] rgpszFile, uint[] rggrfChange)
        {
            // This will be triggered when this file is changed outside of Visual Studio.

            _foregroundDispatcher.AssertForegroundThread();

            FireFileChanged();

            foreach (_VSFILECHANGEFLAGS flag in rggrfChange)
            {
                if (flag.HasFlag(_VSFILECHANGEFLAGS.VSFILECHG_Del))
                {
                    Dispose();
                }
            }

            return VSConstants.S_OK;
        }

        public int DirectoryChanged(string pszDirectory)
        {
            return VSConstants.S_OK;
        }
        #endregion

        #region IVsRunningDocTableEvents
        public int OnAfterFirstDocumentLock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeLastDocumentUnlock(uint docCookie, uint dwRDTLockType, uint dwReadLocksRemaining, uint dwEditLocksRemaining)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterSave(uint docCookie)
        {
            // This will be triggered when the document is saved in Visual Studio.

            _foregroundDispatcher.AssertForegroundThread();

            FireOnAfterSave(docCookie);

            return VSConstants.S_OK;
        }

        public int OnAfterAttributeChange(uint docCookie, uint grfAttribs)
        {
            return VSConstants.S_OK;
        }

        public int OnBeforeDocumentWindowShow(uint docCookie, int fFirstShow, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }

        public int OnAfterDocumentWindowHide(uint docCookie, IVsWindowFrame pFrame)
        {
            return VSConstants.S_OK;
        }
        #endregion

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            StopListeningForChanges();

            _associatedParsers.Clear();

            _disposed = true;
        }

        private void StartListeningForChanges()
        {
            try
            {
                if (_fileChangeCookie == VSConstants.VSCOOKIE_NIL)
                {
                    Marshal.ThrowExceptionForHR(
                        _fileChangeService.AdviseFileChange(_filePath, FileChangeFlags, this, out _fileChangeCookie));
                }

                if (_runningDocumentTableCookie == VSConstants.VSCOOKIE_NIL)
                {
                    Marshal.ThrowExceptionForHR(
                        _runningDocumentTable.AdviseRunningDocTableEvents(this, out _runningDocumentTableCookie));
                }
            }
            catch (Exception exception)
            {
                _errorReporter.ReportError(exception);
            }
        }

        private void StopListeningForChanges()
        {
            try
            {
                if (_fileChangeCookie != VSConstants.VSCOOKIE_NIL)
                {
                    Marshal.ThrowExceptionForHR(_fileChangeService.UnadviseFileChange(_fileChangeCookie));
                    _fileChangeCookie = VSConstants.VSCOOKIE_NIL;
                }

                if (_runningDocumentTableCookie != VSConstants.VSCOOKIE_NIL)
                {
                    Marshal.ThrowExceptionForHR(_runningDocumentTable.UnadviseRunningDocTableEvents(_runningDocumentTableCookie));
                    _runningDocumentTableCookie = VSConstants.VSCOOKIE_NIL;
                }
            }
            catch (Exception exception)
            {
                _errorReporter.ReportError(exception);
            }
        }

        private void FireFileChanged()
        {
            // We want to reparse all open documents that are associated with this import.
            foreach (var parser in _associatedParsers)
            {
                parser.QueueReparse();
            }
        }

        private void FireOnAfterSave(uint docCookie)
        {
            if (!TryGetTextBufferFromDocCookie(docCookie, out var textBuffer))
            {
                return;
            }

            if (!textBuffer.IsRazorBuffer() ||
                !textBuffer.Properties.TryGetProperty<VisualStudioDocumentTracker>(typeof(VisualStudioDocumentTracker), out var tracker))
            {
                // This is not the document we care about. Bail.
                return;
            }

            if (string.Equals(_filePath, tracker.FilePath, StringComparison.OrdinalIgnoreCase))
            {
                FireFileChanged();
            }
        }

        private bool TryGetTextBufferFromDocCookie(uint docCookie, out ITextBuffer textBuffer)
        {
            // Refer https://github.com/dotnet/roslyn/blob/aeeec402a2f223580f36c298bbd9d92ffc94b330/src/VisualStudio/Core/Def/Implementation/SaveEventsService.cs#L97

            textBuffer = null;
            var docData = IntPtr.Zero;

            try
            {
                Marshal.ThrowExceptionForHR(
                    _runningDocumentTable.GetDocumentInfo(
                        docCookie,
                        out var flags,
                        out var readLocks,
                        out var writeLocks,
                        out var moniker,
                        out var hierarchy,
                        out var itemid,
                        out docData));

                if (Marshal.GetObjectForIUnknown(docData) is IVsTextBuffer shimTextBuffer)
                {
                    textBuffer = _editorAdaptersFactoryService.GetDocumentBuffer(shimTextBuffer);
                    if (textBuffer != null)
                    {
                        return true;
                    }
                }
            }
            finally
            {
                if (docData != IntPtr.Zero)
                {
                    Marshal.Release(docData);
                }
            }

            return false;
        }
    }
}
