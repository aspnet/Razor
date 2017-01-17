// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    public class CSharpTokenizerCommentTest : CSharpTokenizerTestBase
    {
        private new CSharpSymbol IgnoreRemaining => (CSharpSymbol)base.IgnoreRemaining;

        private new CSharpLanguageCharacteristics Language => (CSharpLanguageCharacteristics)base.Language;

        [Fact]
        public void Next_Ignores_Star_At_EOF_In_RazorComment()
        {
            TestTokenizer(
                "@* Foo * Bar * Baz *",
                Language.CreateSymbol("@", CSharpSymbolType.RazorCommentTransition),
                Language.CreateSymbol("*", CSharpSymbolType.RazorCommentStar),
                Language.CreateSymbol(" Foo * Bar * Baz *", CSharpSymbolType.RazorComment));
        }

        [Fact]
        public void Next_Ignores_Star_Without_Trailing_At()
        {
            TestTokenizer(
                "@* Foo * Bar * Baz *@",
                Language.CreateSymbol("@", CSharpSymbolType.RazorCommentTransition),
                Language.CreateSymbol("*", CSharpSymbolType.RazorCommentStar),
                Language.CreateSymbol(" Foo * Bar * Baz ", CSharpSymbolType.RazorComment),
                Language.CreateSymbol("*", CSharpSymbolType.RazorCommentStar),
                Language.CreateSymbol("@", CSharpSymbolType.RazorCommentTransition));
        }

        [Fact]
        public void Next_Returns_RazorComment_Token_For_Entire_Razor_Comment()
        {
            TestTokenizer(
                "@* Foo Bar Baz *@",
                Language.CreateSymbol("@", CSharpSymbolType.RazorCommentTransition),
                Language.CreateSymbol("*", CSharpSymbolType.RazorCommentStar),
                Language.CreateSymbol(" Foo Bar Baz ", CSharpSymbolType.RazorComment),
                Language.CreateSymbol("*", CSharpSymbolType.RazorCommentStar),
                Language.CreateSymbol("@", CSharpSymbolType.RazorCommentTransition));
        }

        [Fact]
        public void Next_Returns_Comment_Token_For_Entire_Single_Line_Comment()
        {
            TestTokenizer("// Foo Bar Baz", Language.CreateSymbol("// Foo Bar Baz", CSharpSymbolType.Comment));
        }

        [Fact]
        public void Single_Line_Comment_Is_Terminated_By_Newline()
        {
            TestTokenizer("// Foo Bar Baz\na", Language.CreateSymbol("// Foo Bar Baz", CSharpSymbolType.Comment), IgnoreRemaining);
        }

        [Fact]
        public void Multi_Line_Comment_In_Single_Line_Comment_Has_No_Effect()
        {
            TestTokenizer("// Foo/*Bar*/ Baz\na", Language.CreateSymbol("// Foo/*Bar*/ Baz", CSharpSymbolType.Comment), IgnoreRemaining);
        }

        [Fact]
        public void Next_Returns_Comment_Token_For_Entire_Multi_Line_Comment()
        {
            TestTokenizer("/* Foo\nBar\nBaz */", Language.CreateSymbol("/* Foo\nBar\nBaz */", CSharpSymbolType.Comment));
        }

        [Fact]
        public void Multi_Line_Comment_Is_Terminated_By_End_Sequence()
        {
            TestTokenizer("/* Foo\nBar\nBaz */a", Language.CreateSymbol("/* Foo\nBar\nBaz */", CSharpSymbolType.Comment), IgnoreRemaining);
        }

        [Fact]
        public void Unterminated_Multi_Line_Comment_Captures_To_EOF()
        {
            TestTokenizer("/* Foo\nBar\nBaz", Language.CreateSymbol("/* Foo\nBar\nBaz", CSharpSymbolType.Comment), IgnoreRemaining);
        }

        [Fact]
        public void Nested_Multi_Line_Comments_Terminated_At_First_End_Sequence()
        {
            TestTokenizer("/* Foo/*\nBar\nBaz*/ */", Language.CreateSymbol("/* Foo/*\nBar\nBaz*/", CSharpSymbolType.Comment), IgnoreRemaining);
        }

        [Fact]
        public void Nested_Multi_Line_Comments_Terminated_At_Full_End_Sequence()
        {
            TestTokenizer("/* Foo\nBar\nBaz* */", Language.CreateSymbol("/* Foo\nBar\nBaz* */", CSharpSymbolType.Comment), IgnoreRemaining);
        }
    }
}
