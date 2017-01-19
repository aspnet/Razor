// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    internal class CSharpSymbol : SymbolBase<CSharpSymbolType>
    {
        internal CSharpSymbol(
            string content,
            CSharpSymbolType type,
            IReadOnlyList<RazorError> errors)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            Content = content;
            Type = type;
            Errors = errors;
        }

        public override IReadOnlyList<RazorError> Errors { get; }

        public override string Content { get; }

        public override CSharpSymbolType Type { get; }

        public CSharpKeyword? Keyword { get; set; }

        public override bool Equals(object obj)
        {
            var other = obj as CSharpSymbol;
            return base.Equals(other) &&
                other.Keyword == Keyword;
        }

        public override int GetHashCode()
        {
            // Hash code should include only immutable properties.
            return base.GetHashCode();
        }
    }
}
