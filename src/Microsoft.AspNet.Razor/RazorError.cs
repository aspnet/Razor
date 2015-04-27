// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.AspNet.Razor
{
    public class RazorError : IEquatable<RazorError>
    {
        private static readonly int TypeHashCode = typeof(RazorError).GetHashCode();

        public RazorError()
            : this(message: string.Empty, location: SourceLocation.Undefined)
        {
        }

        public RazorError(string message, SourceLocation location)
            : this(message, location, 1)
        {
        }

        public RazorError(string message, int absoluteIndex, int lineIndex, int columnIndex)
            : this(message, new SourceLocation(absoluteIndex, lineIndex, columnIndex))
        {
        }

        public RazorError(string message, SourceLocation location, int length)
        {
            Message = message;
            Location = location;
            Length = length;
        }

        public RazorError(string message, int absoluteIndex, int lineIndex, int columnIndex, int length)
            : this(message, new SourceLocation(absoluteIndex, lineIndex, columnIndex), length)
        {
        }

        public string Message { get; set; }
        public SourceLocation Location { get; set; }
        public int Length { get; set; }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "Error @ {0}({2}) - [{1}]", Location, Message, Length);
        }

        public override bool Equals(object obj)
        {
            var error = obj as RazorError;
            return Equals(error);
        }

        public override int GetHashCode()
        {
            // Hash code should include only immutable properties but Equals also checks the type.
            return TypeHashCode;
        }

        public bool Equals(RazorError other)
        {
            return other != null &&
                string.Equals(other.Message, Message, StringComparison.Ordinal) &&
                Location.Equals(other.Location);
        }
    }
}
