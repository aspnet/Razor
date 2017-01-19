// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    public abstract class CSharpTokenizerTestBase : TokenizerTestBase
    {
        private readonly CSharpLanguageCharacteristics _language;

        private readonly CSharpSymbol _ignoreRemaining;

        protected CSharpTokenizerTestBase()
        {
            _language = new CSharpLanguageCharacteristics(new DefaultCSharpSymbolFactory());
            _ignoreRemaining = _language.CreateSymbol(string.Empty, CSharpSymbolType.Unknown);
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

        internal void TestSingleToken(string text, CSharpSymbolType expectedSymbolType)
        {
            TestTokenizer(text, _language.CreateSymbol(text, expectedSymbolType, RazorError.EmptyArray));
        }

        internal void TestTokenizer(string input, params CSharpSymbol[] expectedSymbols)
        {
            base.TestTokenizer<CSharpSymbol, CSharpSymbolType>(input, expectedSymbols);
        }
    }
}
