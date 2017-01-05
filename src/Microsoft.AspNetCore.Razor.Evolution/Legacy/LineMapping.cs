// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    internal class LineMapping
    {
        public LineMapping(SourceSpan documentLocation, SourceSpan generatedLocation)
        {
            DocumentLocation = documentLocation;
            GeneratedLocation = generatedLocation;
        }

        public SourceSpan DocumentLocation { get; }

        public SourceSpan GeneratedLocation { get; }

        public override bool Equals(object obj)
        {
            var other = obj as LineMapping;
            if (ReferenceEquals(other, null))
            {
                return false;
            }

            return DocumentLocation.Equals(other.DocumentLocation) &&
                GeneratedLocation.Equals(other.GeneratedLocation);
        }

        public override int GetHashCode()
        {
            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(DocumentLocation);
            hashCodeCombiner.Add(GeneratedLocation);

            return hashCodeCombiner;
        }

        public static bool operator ==(LineMapping left, LineMapping right)
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

        public static bool operator !=(LineMapping left, LineMapping right)
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

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} -> {1}", DocumentLocation, GeneratedLocation);
        }
    }
}
