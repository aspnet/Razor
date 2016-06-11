// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Text;
using Microsoft.AspNetCore.Razor.Tokenizer;
using Microsoft.AspNetCore.Razor.Tokenizer.Internal;
using Microsoft.AspNetCore.Razor.Tokenizer.Symbols;

namespace Microsoft.AspNetCore.Razor.Test.Tokenizer.Internal
{
    public abstract class CSharpTokenizerTestBase : TokenizerTestBase<CSharpSymbol, CSharpSymbolType>
    {
        private static CSharpSymbol _ignoreRemaining = new CSharpSymbol(0, 0, 0, string.Empty, CSharpSymbolType.Unknown);

        protected override CSharpSymbol IgnoreRemaining
        {
            get { return _ignoreRemaining; }
        }

        protected override Tokenizer<CSharpSymbol, CSharpSymbolType> CreateTokenizer(ITextDocument source)
        {
            return new CSharpTokenizer(source);
        }

        protected void TestSingleToken(string text, CSharpSymbolType expectedSymbolType)
        {
            TestTokenizer(text, new CSharpSymbol(0, 0, 0, text, expectedSymbolType));
        }
    }
}
