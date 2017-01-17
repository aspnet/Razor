// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    internal class DefaultCSharpSymbolFactory : ICSharpSymbolFactory
    {
        public CSharpSymbol Create(string content, CSharpSymbolType type, IReadOnlyList<RazorError> errors)
        {
            return new CSharpSymbol(content, type, errors);
        }
    }
}
