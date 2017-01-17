// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    internal abstract class HtmlSymbol : SymbolBase<HtmlSymbolType>
    {
        public override IReadOnlyList<RazorError> Errors => RazorError.EmptyArray;
    }
}
