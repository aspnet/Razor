// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Globalization;

namespace Microsoft.AspNet.Razor.Generator.Compiler
{
    public class LineMapping
    {
        private static readonly int TypeHashCode = typeof(LineMapping).GetHashCode();

        public LineMapping()
            : this(documentLocation: null, generatedLocation: null)
        {
        }

        public LineMapping(MappingLocation documentLocation, MappingLocation generatedLocation)
        {
            DocumentLocation = documentLocation;
            GeneratedLocation = generatedLocation;
        }

        public MappingLocation DocumentLocation { get; set; }
        public MappingLocation GeneratedLocation { get; set; }

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
            // Hash code should include only immutable properties but Equals also checks the type.
            return TypeHashCode;
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
            return string.Format(CultureInfo.CurrentUICulture, "{0} -> {1}", DocumentLocation, GeneratedLocation);
        }
    }
}
