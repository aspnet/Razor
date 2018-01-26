// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Razor.Editor;

namespace Microsoft.VisualStudio.Editor.Razor
{
    [System.Composition.Shared]
    [Export(typeof(EditorSettingsManager))]
    internal class DefaultEditorSettingsManager : EditorSettingsManager
    {
        private readonly List<WeakReference<EventHandler<EditorSettingsChangedEventArgs>>> _changedHandlers;
        private readonly object SettingsAccessorLock = new object();
        private readonly ForegroundDispatcher _foregroundDispatcher;
        private EditorSettings _settings;

        [ImportingConstructor]
        public DefaultEditorSettingsManager(ForegroundDispatcher foregroundDispatcher)
        {
            _foregroundDispatcher = foregroundDispatcher;
            _settings = EditorSettings.Default;
        }

        public override event EventHandler<EditorSettingsChangedEventArgs> Changed
        {
            add
            {
                _foregroundDispatcher.AssertForegroundThread();

                _changedHandlers.Add(new WeakReference<EventHandler<EditorSettingsChangedEventArgs>>(value));
            }
            remove
            {
                _foregroundDispatcher.AssertForegroundThread();

                for (var i = 0; i < _changedHandlers.Count; i++)
                {
                    if (_changedHandlers[i].TryGetTarget(out var target) && target == value)
                    {
                        _changedHandlers.RemoveAt(i);
                        return;
                    }
                }
            }
        }

        public override EditorSettings Current
        {
            get
            {
                lock (SettingsAccessorLock)
                {
                    return _settings;
                }
            }
        }

        public override void Update(EditorSettings updatedSettings)
        {
            if (updatedSettings == null)
            {
                throw new ArgumentNullException(nameof(updatedSettings));
            }

            _foregroundDispatcher.AssertForegroundThread();

            lock (SettingsAccessorLock)
            {
                if (!_settings.Equals(updatedSettings))
                {
                    _settings = updatedSettings;
                    OnChanged();
                }
            }
        }

        private void OnChanged()
        {
            _foregroundDispatcher.AssertForegroundThread();

            var args = new EditorSettingsChangedEventArgs(Current);

            for (var i = 0; i < _changedHandlers.Count; i++)
            {
                if (_changedHandlers[i].TryGetTarget(out var handler))
                {
                    handler.Invoke(this, args);
                }
                else
                {
                    _changedHandlers.RemoveAt(i--);
                }
            }
        }
    }
}
