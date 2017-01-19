// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    public abstract class HtmlTokenizerTestBase : TokenizerTestBase
    {
        private readonly HtmlLanguageCharacteristics _language;

        private readonly HtmlSymbol _ignoreRemaining;

        protected HtmlTokenizerTestBase()
        {
            _language = new HtmlLanguageCharacteristics(new DefaultHtmlSymbolFactory());
            _ignoreRemaining = _language.CreateSymbol(string.Empty, HtmlSymbolType.Unknown);
        }

        internal override object IgnoreRemaining
        {
            get { return _ignoreRemaining; }
        }

        internal override object Language
        {
            get { return _language; }
        }

        internal override object CreateTokenizer(ITextDocument source)
        {
            return _language.CreateTokenizer(source);
        }

        internal void TestSingleToken(string text, HtmlSymbolType expectedSymbolType)
        {
            TestTokenizer(text, _language.CreateSymbol(text, expectedSymbolType));
        }

        internal void TestTokenizer(string input, params HtmlSymbol[] expectedSymbols)
        {
            base.TestTokenizer<HtmlSymbol, HtmlSymbolType>(input, expectedSymbols);
        }
    }
}
