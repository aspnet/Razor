// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    public class CSharpTokenizerTest : CSharpTokenizerTestBase
    {
        private new CSharpSymbol IgnoreRemaining => (CSharpSymbol)base.IgnoreRemaining;

        private new CSharpLanguageCharacteristics Language => (CSharpLanguageCharacteristics)base.Language;

        [Fact]
        public void Next_Returns_Null_When_EOF_Reached()
        {
            TestTokenizer("");
        }

        [Fact]
        public void Next_Returns_Newline_Token_For_Single_CR()
        {
            TestTokenizer(
                "\r\ra",
                Language.CreateSymbol("\r", CSharpSymbolType.NewLine),
                Language.CreateSymbol("\r", CSharpSymbolType.NewLine),
                IgnoreRemaining);
        }

        [Fact]
        public void Next_Returns_Newline_Token_For_Single_LF()
        {
            TestTokenizer(
                "\n\na",
                Language.CreateSymbol("\n", CSharpSymbolType.NewLine),
                Language.CreateSymbol("\n", CSharpSymbolType.NewLine),
                IgnoreRemaining);
        }

        [Fact]
        public void Next_Returns_Newline_Token_For_Single_NEL()
        {
            // NEL: Unicode "Next Line" U+0085
            TestTokenizer(
                "\u0085\u0085a",
                Language.CreateSymbol("\u0085", CSharpSymbolType.NewLine),
                Language.CreateSymbol("\u0085", CSharpSymbolType.NewLine),
                IgnoreRemaining);
        }

        [Fact]
        public void Next_Returns_Newline_Token_For_Single_Line_Separator()
        {
            // Unicode "Line Separator" U+2028
            TestTokenizer(
                "\u2028\u2028a",
                Language.CreateSymbol("\u2028", CSharpSymbolType.NewLine),
                Language.CreateSymbol("\u2028", CSharpSymbolType.NewLine),
                IgnoreRemaining);
        }

        [Fact]
        public void Next_Returns_Newline_Token_For_Single_Paragraph_Separator()
        {
            // Unicode "Paragraph Separator" U+2029
            TestTokenizer(
                "\u2029\u2029a",
                Language.CreateSymbol("\u2029", CSharpSymbolType.NewLine),
                Language.CreateSymbol("\u2029", CSharpSymbolType.NewLine),
                IgnoreRemaining);
        }

        [Fact]
        public void Next_Returns_Single_Newline_Token_For_CRLF()
        {
            TestTokenizer(
                "\r\n\r\na",
                Language.CreateSymbol("\r\n", CSharpSymbolType.NewLine),
                Language.CreateSymbol("\r\n", CSharpSymbolType.NewLine),
                IgnoreRemaining);
        }

        [Fact]
        public void Next_Returns_Token_For_Whitespace_Characters()
        {
            TestTokenizer(
                " \f\t\u000B \n ",
                Language.CreateSymbol(" \f\t\u000B ", CSharpSymbolType.WhiteSpace),
                Language.CreateSymbol("\n", CSharpSymbolType.NewLine),
                Language.CreateSymbol(" ", CSharpSymbolType.WhiteSpace));
        }

        [Fact]
        public void Transition_Is_Recognized()
        {
            TestSingleToken("@", CSharpSymbolType.Transition);
        }

        [Fact]
        public void Transition_Is_Recognized_As_SingleCharacter()
        {
            TestTokenizer(
                "@(",
                Language.CreateSymbol("@", CSharpSymbolType.Transition),
                Language.CreateSymbol("(", CSharpSymbolType.LeftParenthesis));
        }
    }
}
