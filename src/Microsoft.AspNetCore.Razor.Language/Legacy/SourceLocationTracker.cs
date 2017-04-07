// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal class SourceLocationTracker
    {
        private int _absoluteIndex = 0;
        private int _characterIndex = 0;
        private int _lineIndex = 0;
        private SourceLocation _currentLocation;

        public SourceLocationTracker()
            : this(SourceLocation.Zero)
        {
        }

        public SourceLocationTracker(SourceLocation currentLocation)
        {
            CurrentLocation = currentLocation;

            UpdateInternalState();
        }

        public SourceLocation CurrentLocation
        {
            get
            {
                return _currentLocation;
            }
            set
            {
                if (!_currentLocation.Equals(value))
                {
                    _currentLocation = value;
                    UpdateInternalState();
                }
            }
        }

        public static SourceLocation Advance(SourceLocation location, string text)
        {
            if (text == null)
            {
                throw new ArgumentNullException(nameof(text));
            }

            var tracker = new SourceLocationTracker(location);
            tracker.UpdateLocation(text);
            return tracker.CurrentLocation;
        }

        public void UpdateLocation(char characterRead, char nextCharacter)
        {
            UpdateCharacterCore(characterRead, nextCharacter);
            RecalculateSourceLocation();
        }

        public SourceLocationTracker UpdateLocation(string content)
        {
            for (int i = 0; i < content.Length; i++)
            {
                var nextCharacter = '\0';
                if (i < content.Length - 1)
                {
                    nextCharacter = content[i + 1];
                }
                UpdateCharacterCore(content[i], nextCharacter);
            }
            RecalculateSourceLocation();
            return this;
        }

        private void UpdateCharacterCore(char characterRead, char nextCharacter)
        {
            _absoluteIndex++;

            if (Environment.NewLine.Length == 1 && characterRead == Environment.NewLine[0] ||
                ParserHelpers.IsNewLine(characterRead) && (characterRead != '\r' || nextCharacter != '\n'))
            {
                _lineIndex++;
                _characterIndex = 0;
            }
            else
            {
                _characterIndex++;
            }
        }

        private void UpdateInternalState()
        {
            _absoluteIndex = CurrentLocation.AbsoluteIndex;
            _characterIndex = CurrentLocation.CharacterIndex;
            _lineIndex = CurrentLocation.LineIndex;
        }

        private void RecalculateSourceLocation()
        {
            _currentLocation = new SourceLocation(
                _currentLocation.FilePath,
                _absoluteIndex,
                _lineIndex,
                _characterIndex);
        }

        public static SourceLocation CalculateNewLocation(SourceLocation lastPosition, string newContent)
        {
            return new SourceLocationTracker(lastPosition).UpdateLocation(newContent).CurrentLocation;
        }
    }
}
