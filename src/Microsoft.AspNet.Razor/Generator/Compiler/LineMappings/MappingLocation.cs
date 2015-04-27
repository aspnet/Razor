// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace Microsoft.AspNet.Razor.Generator.Compiler
{
    public class MappingLocation
    {
        private static readonly int TypeHashCode = typeof(MappingLocation).GetHashCode();

        public MappingLocation()
        {
        }

        public MappingLocation(SourceLocation location, int contentLength)
        {
            ContentLength = contentLength;
            AbsoluteIndex = location.AbsoluteIndex;
            LineIndex = location.LineIndex;
            CharacterIndex = location.CharacterIndex;
        }

        public int ContentLength { get; set; }
        public int AbsoluteIndex { get; set; }
        public int LineIndex { get; set; }
        public int CharacterIndex { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as MappingLocation;
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return AbsoluteIndex == other.AbsoluteIndex &&
                ContentLength == other.ContentLength &&
                LineIndex == other.LineIndex &&
                CharacterIndex == other.CharacterIndex;
        }

        public override int GetHashCode()
        {
            // Hash code should include only immutable properties but Equals also checks the type.
            return TypeHashCode;
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture, "({0}:{1},{2} [{3}])",
                AbsoluteIndex,
                LineIndex,
                CharacterIndex,
                ContentLength);
        }

        public static bool operator ==(MappingLocation left, MappingLocation right)
        {
            if (ReferenceEquals(left, right))
            {
                // Exact equality e.g. both objects are null.
                return true;
            }

            if (ReferenceEquals(left, null))
            {
                return false;
            }

            return left.Equals(right);
        }

        public static bool operator !=(MappingLocation left, MappingLocation right)
        {
            if (ReferenceEquals(left, right))
            {
                // Exact equality e.g. both objects are null.
                return false;
            }

            if (ReferenceEquals(left, null))
            {
                return true;
            }

            return !left.Equals(right);
        }
    }
}
