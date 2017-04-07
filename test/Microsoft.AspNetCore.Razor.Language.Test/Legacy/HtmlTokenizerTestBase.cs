// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    public abstract class HtmlTokenizerTestBase : TokenizerTestBase
    {
        private static HtmlSymbol _ignoreRemaining = new HtmlSymbol(string.Empty, HtmlSymbolType.Unknown);

        internal override object IgnoreRemaining
        {
            get { return _ignoreRemaining; }
        }

        internal override object CreateTokenizer(ITextDocument source)
        {
            return new HtmlTokenizer(source);
        }

        internal void TestSingleToken(string text, HtmlSymbolType expectedSymbolType)
        {
            TestTokenizer(text, new HtmlSymbol(text, expectedSymbolType));
        }

        internal void TestTokenizer(string input, params HtmlSymbol[] expectedSymbols)
        {
            base.TestTokenizer<HtmlSymbol, HtmlSymbolType>(input, expectedSymbols);
        }
    }
}
