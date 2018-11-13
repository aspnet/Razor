// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal class DynamicAttributeBlockChunkGenerator : ParentChunkGenerator
    {
        public DynamicAttributeBlockChunkGenerator(LocationTagged<string> prefix, int offset, int line, int col)
            : this(prefix, new SourceLocation(offset, line, col))
        {
        }

        public DynamicAttributeBlockChunkGenerator(LocationTagged<string> prefix, SourceLocation valueStart)
        {
            Prefix = prefix;
            ValueStart = valueStart;
        }

        public LocationTagged<string> Prefix { get; }

        public SourceLocation ValueStart { get; }

        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture, "DynAttr:{0:F}", Prefix);
        }

        public override bool Equals(object obj)
        {
            var other = obj as DynamicAttributeBlockChunkGenerator;
            return other != null &&
                Equals(other.Prefix, Prefix);
        }

        public override int GetHashCode()
        {
            return Prefix == null ? 0 : Prefix.GetHashCode();
        }
    }
}
