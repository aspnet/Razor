// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Razor.Editor;

namespace Microsoft.VisualStudio.Editor.Razor
{
    internal class DefaultEditorSettingsManagerInternal : EditorSettingsManagerInternal
    {
        public override event EventHandler<EditorSettingsChangedEventArgs> Changed;

        private readonly EditorSettingsManager _editorSettingsManager;
        private readonly ForegroundDispatcher _foregroundDispatcher;

        public DefaultEditorSettingsManagerInternal(EditorSettingsManager editorSettingsManager, ForegroundDispatcher dispatcher)
        {
            if (editorSettingsManager == null)
            {
                throw new ArgumentNullException(nameof(editorSettingsManager));
            }

            if (dispatcher == null)
            {
                throw new ArgumentNullException(nameof(dispatcher));
            }

            _editorSettingsManager = editorSettingsManager;
            _foregroundDispatcher = dispatcher;

            _editorSettingsManager.Changed += OnChanged;
        }

        public override EditorSettings Current => _editorSettingsManager.Current;

        private void OnChanged(object sender, EditorSettingsChangedEventArgs e)
        {
            _foregroundDispatcher.AssertForegroundThread();

            var args = new EditorSettingsChangedEventArgs(Current);
            Changed?.Invoke(this, args);
        }
    }
}
