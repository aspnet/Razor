// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
using Microsoft.AspNetCore.Razor.Test.Framework;
using Microsoft.AspNetCore.Razor.Tokenizer.Symbols;
using Microsoft.AspNetCore.Razor.Tokenizer.Symbols.Internal;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Parser
{
    public class CSharpRazorCommentsTest : CsHtmlMarkupParserTestBase
    {
        [Fact]
        public void UnterminatedRazorComment()
        {
            ParseDocumentTest("@*",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            string.Empty,
                            HtmlSymbolType.Unknown))
                               .Accepts(AcceptedCharacters.Any))),
                new RazorError(
                    RazorResources.ParseError_RazorComment_Not_Terminated,
                    SourceLocation.Zero,
                    length: 2));
        }

        [Fact]
        public void EmptyRazorComment()
        {
            ParseDocumentTest("@**@",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            string.Empty,
                            HtmlSymbolType.Unknown))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void RazorCommentInImplicitExpressionMethodCall()
        {
            ParseDocumentTest("@foo(" + Environment.NewLine
                            + "@**@" + Environment.NewLine,
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("foo(" + Environment.NewLine)
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords),
                        new CommentBlock(
                            Factory.CodeTransition(CSharpSymbolType.RazorCommentTransition)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.MetaCode("*", CSharpSymbolType.RazorCommentStar)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.Span(SpanKind.Comment, new CSharpSymbol(
                                Factory.LocationTracker.CurrentLocation,
                                string.Empty,
                                CSharpSymbolType.Unknown))
                                   .Accepts(AcceptedCharacters.Any),
                            Factory.MetaCode("*", CSharpSymbolType.RazorCommentStar)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.CodeTransition(CSharpSymbolType.RazorCommentTransition)
                                   .Accepts(AcceptedCharacters.None)),
                        Factory.Code(Environment.NewLine)
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords))),
                new RazorError(
                    RazorResources.FormatParseError_Expected_CloseBracket_Before_EOF("(", ")"),
                    new SourceLocation(4, 0, 4),
                    length: 1));
        }

        [Fact]
        public void UnterminatedRazorCommentInImplicitExpressionMethodCall()
        {
            ParseDocumentTest("@foo(@*",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("foo(")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords),
                        new CommentBlock(
                            Factory.CodeTransition(CSharpSymbolType.RazorCommentTransition)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.MetaCode("*", CSharpSymbolType.RazorCommentStar)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.Span(SpanKind.Comment, new CSharpSymbol(
                                Factory.LocationTracker.CurrentLocation,
                                string.Empty,
                                CSharpSymbolType.Unknown))
                                    .Accepts(AcceptedCharacters.Any)))),
                new RazorError(
                    RazorResources.ParseError_RazorComment_Not_Terminated,
                    new SourceLocation(5, 0, 5),
                    length: 2),
                new RazorError(
                    RazorResources.FormatParseError_Expected_CloseBracket_Before_EOF("(", ")"),
                    new SourceLocation(4, 0, 4),
                    length: 1));
        }

        [Fact]
        public void RazorCommentInVerbatimBlock()
        {
            ParseDocumentTest("@{" + Environment.NewLine
                            + "    <text" + Environment.NewLine
                            + "    @**@" + Environment.NewLine
                            + "}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        Factory.Code(Environment.NewLine)
                            .AsStatement()
                            .AutoCompleteWith("}"),
                        new MarkupBlock(
                            new MarkupTagBlock(
                                Factory.MarkupTransition("<text").Accepts(AcceptedCharacters.Any)),
                            Factory.Markup(Environment.NewLine).Accepts(AcceptedCharacters.None),
                            Factory.Markup("    ").With(SpanChunkGenerator.Null),
                            new CommentBlock(
                                Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                                       .Accepts(AcceptedCharacters.None),
                                Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                                       .Accepts(AcceptedCharacters.None),
                                Factory.Span(SpanKind.Comment, new HtmlSymbol(
                                    Factory.LocationTracker.CurrentLocation,
                                    string.Empty,
                                    HtmlSymbolType.Unknown))
                                       .Accepts(AcceptedCharacters.Any),
                                Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                                       .Accepts(AcceptedCharacters.None),
                                Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                                       .Accepts(AcceptedCharacters.None)),
                            Factory.Markup(Environment.NewLine).With(SpanChunkGenerator.Null),
                            Factory.Markup("}")))),
                new RazorError(
                    RazorResources.ParseError_TextTagCannotContainAttributes,
                    new SourceLocation(7 + Environment.NewLine.Length, 1, 5),
                    length: 4),
                new RazorError(
                    RazorResources.FormatParseError_MissingEndTag("text"),
                    new SourceLocation(7 + Environment.NewLine.Length, 1, 5),
                    length: 4),
                new RazorError(
                    RazorResources.FormatParseError_Expected_EndOfBlock_Before_EOF(RazorResources.BlockName_Code, "}", "{"),
                    new SourceLocation(1, 0, 1),
                    length: 1));
        }

        [Fact]
        public void UnterminatedRazorCommentInVerbatimBlock()
        {
            ParseDocumentTest("@{@*",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        Factory.EmptyCSharp()
                            .AsStatement()
                            .AutoCompleteWith("}"),
                        new CommentBlock(
                            Factory.CodeTransition(CSharpSymbolType.RazorCommentTransition)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.MetaCode("*", CSharpSymbolType.RazorCommentStar)
                                   .Accepts(AcceptedCharacters.None),
                            Factory.Span(SpanKind.Comment, new CSharpSymbol(Factory.LocationTracker.CurrentLocation,
                                                                        string.Empty,
                                                                        CSharpSymbolType.Unknown))
                                   .Accepts(AcceptedCharacters.Any)))),
                new RazorError(
                    RazorResources.ParseError_RazorComment_Not_Terminated,
                    new SourceLocation(2, 0, 2),
                    length: 2),
                new RazorError(
                    RazorResources.FormatParseError_Expected_EndOfBlock_Before_EOF(
                        RazorResources.BlockName_Code, "}", "{"),
                    new SourceLocation(1, 0, 1),
                    length: 1));
        }

        [Fact]
        public void RazorCommentInMarkup()
        {
            ParseDocumentTest(
                "<p>" + Environment.NewLine
                + "@**@" + Environment.NewLine
                + "</p>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<p>")),
                    Factory.Markup(Environment.NewLine),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            string.Empty,
                            HtmlSymbolType.Unknown))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.Markup(Environment.NewLine).With(SpanChunkGenerator.Null),
                    new MarkupTagBlock(
                        Factory.Markup("</p>"))
                    ));
        }

        [Fact]
        public void MultipleRazorCommentInMarkup()
        {
            ParseDocumentTest(
                "<p>" + Environment.NewLine
                + "  @**@  " + Environment.NewLine
                + "@**@" + Environment.NewLine
                + "</p>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<p>")),
                    Factory.Markup(Environment.NewLine),
                    Factory.Markup("  ").With(SpanChunkGenerator.Null),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            string.Empty,
                            HtmlSymbolType.Unknown))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.Markup("  " + Environment.NewLine).With(SpanChunkGenerator.Null),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            string.Empty,
                            HtmlSymbolType.Unknown))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.Markup(Environment.NewLine).With(SpanChunkGenerator.Null),
                    new MarkupTagBlock(
                        Factory.Markup("</p>"))
                    ));
        }

        [Fact]
        public void MultipleRazorCommentsInSameLineInMarkup()
        {
            ParseDocumentTest(
                "<p>" + Environment.NewLine
                + "@**@  @**@" + Environment.NewLine
                + "</p>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<p>")),
                    Factory.Markup(Environment.NewLine),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            string.Empty,
                            HtmlSymbolType.Unknown))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml(),
                    Factory.Markup("  ").With(SpanChunkGenerator.Null),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            string.Empty,
                            HtmlSymbolType.Unknown))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.Markup(Environment.NewLine).With(SpanChunkGenerator.Null),
                    new MarkupTagBlock(
                        Factory.Markup("</p>"))
                    ));
        }

        [Fact]
        public void RazorCommentsSurroundingMarkup()
        {
            ParseDocumentTest(
                "<p>" + Environment.NewLine
                + "@* hello *@ content @* world *@" + Environment.NewLine
                + "</p>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<p>")),
                    Factory.Markup(Environment.NewLine),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            " hello ",
                            HtmlSymbolType.RazorComment))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.Markup(" content "),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            " world ",
                            HtmlSymbolType.RazorComment))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.Markup(Environment.NewLine),
                    new MarkupTagBlock(
                        Factory.Markup("</p>"))
                    ));
        }

        [Fact]
        public void RazorCommentWithExtraNewLineInMarkup()
        {
            ParseDocumentTest(
                "<p>" + Environment.NewLine + Environment.NewLine
                + "@* content *@" + Environment.NewLine
                + "@*" + Environment.NewLine
                + "content" + Environment.NewLine
                + "*@" + Environment.NewLine + Environment.NewLine
                + "</p>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<p>")),
                    Factory.Markup(Environment.NewLine + Environment.NewLine),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            " content ",
                            HtmlSymbolType.RazorComment))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.Markup(Environment.NewLine).With(SpanChunkGenerator.Null),
                    new CommentBlock(
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Comment, new HtmlSymbol(
                            Factory.LocationTracker.CurrentLocation,
                            Environment.NewLine + "content" + Environment.NewLine,
                            HtmlSymbolType.RazorComment))
                               .Accepts(AcceptedCharacters.Any),
                        Factory.MetaMarkup("*", HtmlSymbolType.RazorCommentStar)
                               .Accepts(AcceptedCharacters.None),
                        Factory.MarkupTransition(HtmlSymbolType.RazorCommentTransition)
                               .Accepts(AcceptedCharacters.None)),
                    Factory.Markup(Environment.NewLine).With(SpanChunkGenerator.Null),
                    Factory.Markup(Environment.NewLine),
                    new MarkupTagBlock(
                        Factory.Markup("</p>"))
                    ));
        }
    }
}
