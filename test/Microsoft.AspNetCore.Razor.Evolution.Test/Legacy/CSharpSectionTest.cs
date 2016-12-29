// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    public class CSharpSectionTest : CsHtmlMarkupParserTestBase
    {
        [Fact]
        public void ParseSectionBlockCapturesNewlineImmediatelyFollowing()
        {
            ParseDocumentTest("@section" + Environment.NewLine,
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None)),
                    Factory.Markup(Environment.NewLine)),
                new RazorError(
                    LegacyResources.FormatDirectiveExpectsIdentifier(CSharpCodeParser.SectionDirectiveDescriptor.Name),
                    new SourceLocation(8, 0, 8),
                    length: Environment.NewLine.Length));
        }

        [Fact]
        public void ParseSectionBlockCapturesWhitespaceToEndOfLineInSectionStatementMissingOpenBrace()
        {
            ParseDocumentTest("@section Foo         " + Environment.NewLine + "    ",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "Foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, "         " + Environment.NewLine + "    ", markup: false).Accepts(AcceptedCharacters.AllWhiteSpace)),
                    Factory.EmptyHtml()),
                new RazorError(
                    LegacyResources.FormatUnexpectedEOFAfterDirective(CSharpCodeParser.SectionDirectiveDescriptor.Name, "{"),
                    new SourceLocation(25 + Environment.NewLine.Length, 0, 25 + Environment.NewLine.Length),
                    length: 1));
        }

        [Fact]
        public void ParseSectionBlockCapturesWhitespaceToEndOfLineInSectionStatementMissingName()
        {
            ParseDocumentTest("@section         " + Environment.NewLine + "    ",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, "         ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace)),
                    Factory.Markup(Environment.NewLine + "    ")),
                new RazorError(
                    LegacyResources.FormatDirectiveExpectsIdentifier(CSharpCodeParser.SectionDirectiveDescriptor.Name),
                    new SourceLocation(17, 0, 17),
                    length: Environment.NewLine.Length));
        }

        [Fact]
        public void ParseSectionBlockIgnoresSectionUnlessAllLowerCase()
        {
            ParseDocumentTest("@Section foo",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("Section")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    Factory.Markup(" foo")));
        }

        [Fact]
        public void ParseSectionBlockReportsErrorAndTerminatesSectionBlockIfKeywordNotFollowedByIdentifierStartCharacter()
        {
            ParseDocumentTest("@section 9 { <p>Foo</p> }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace)),
                    Factory.Markup("9 { "),
                    new MarkupTagBlock(
                        Factory.Markup("<p>")),
                    Factory.Markup("Foo"),
                    new MarkupTagBlock(
                        Factory.Markup("</p>")),
                    Factory.Markup(" }")),
                new RazorError(
                    LegacyResources.FormatDirectiveExpectsIdentifier(CSharpCodeParser.SectionDirectiveDescriptor.Name),
                    new SourceLocation(9, 0, 9),
                    length: 1));
        }

        [Fact]
        public void ParseSectionBlockReportsErrorAndTerminatesSectionBlockIfNameNotFollowedByOpenBrace()
        {
            ParseDocumentTest("@section foo-bar { <p>Foo</p> }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First()))),
                    Factory.Markup("-bar { "),
                    new MarkupTagBlock(
                        Factory.Markup("<p>")),
                    Factory.Markup("Foo"),
                    new MarkupTagBlock(
                        Factory.Markup("</p>")),
                    Factory.Markup(" }")),
                new RazorError(
                    LegacyResources.FormatUnexpectedDirectiveLiteral(CSharpCodeParser.SectionDirectiveDescriptor.Name, "{"),
                    new SourceLocation(12, 0, 12),
                    length: 1));
        }

        [Fact]
        public void ParserOutputsErrorOnNestedSections()
        {
            ParseDocumentTest("@section foo { @section bar { <p>Foo</p> } }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(" "),
                            new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                                Factory.CodeTransition(),
                                Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                                Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                                Factory.Span(SpanKind.Code, "bar", CSharpSymbolType.Identifier)
                                    .Accepts(AcceptedCharacters.NonWhiteSpace)
                                    .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                                Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                                Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                                new MarkupBlock(
                                    Factory.Markup(" "),
                                    new MarkupTagBlock(
                                        Factory.Markup("<p>")),
                                    Factory.Markup("Foo"),
                                    new MarkupTagBlock(
                                        Factory.Markup("</p>")),
                                    Factory.Markup(" ")),
                                Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                            Factory.Markup(" ")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()),
                new RazorError(
                    LegacyResources.FormatParseError_Sections_Cannot_Be_Nested(LegacyResources.SectionExample_CS),
                    new SourceLocation(15, 0, 15),
                    8));
        }

        [Fact]
        public void ParseSectionBlockHandlesEOFAfterOpenBrace()
        {
            ParseDocumentTest("@section foo {",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                    Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                    Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                    Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                        .Accepts(AcceptedCharacters.NonWhiteSpace)
                        .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                    Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                    Factory.MetaCode("{").AutoCompleteWith("}", atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                    new MarkupBlock(
                        Factory.EmptyHtml()))),
                new RazorError(
                    LegacyResources.FormatParseError_Expected_EndOfBlock_Before_EOF(CSharpCodeParser.SectionDirectiveDescriptor.Name, "}", "{"),
                    new SourceLocation(13, 0, 13),
                    length: 1));
        }

        [Theory]
        [InlineData(" ")]
        [InlineData("\n")]
        [InlineData(" abc")]
        [InlineData(" \n abc")]
        public void ParseSectionBlockHandlesEOFAfterOpenContent(string postStartBrace)
        {
            ParseDocumentTest("@section foo {" + postStartBrace,
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith("}", atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(postStartBrace)))),
                new RazorError(
                    LegacyResources.FormatParseError_Expected_EndOfBlock_Before_EOF("section", "}", "{"),
                    new SourceLocation(13, 0, 13),
                    length: 1));
        }

        [Fact]
        public void ParseSectionBlockHandlesUnterminatedSection()
        {
            ParseDocumentTest("@section foo { <p>Foo{}</p>",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith("}", atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(" "),
                            new MarkupTagBlock(
                                Factory.Markup("<p>")),
                            // Need to provide the markup span as fragments, since the parser will split the {} into separate symbols.
                            Factory.Markup("Foo", "{", "}"),
                            new MarkupTagBlock(
                                Factory.Markup("</p>"))))),
                new RazorError(
                    LegacyResources.FormatParseError_Expected_EndOfBlock_Before_EOF("section", "}", "{"),
                    new SourceLocation(13, 0, 13),
                    length: 1));
        }

        [Fact]
        public void ParseSectionBlockHandlesUnterminatedSectionWithNestedIf()
        {
            var newLine = Environment.NewLine;
            var spaces = "    ";
            ParseDocumentTest(
                string.Format(
                    "@section Test{0}{{{0}{1}@if(true){0}{1}{{{0}{1}{1}<p>Hello World</p>{0}{1}}}",
                    newLine,
                    spaces),
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "Test", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, Environment.NewLine, CSharpSymbolType.NewLine).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith("}", atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(newLine),
                            new StatementBlock(
                                Factory.Code(spaces).AsStatement(),
                                Factory.CodeTransition(),
                                Factory.Code($"if(true){newLine}{spaces}{{{newLine}").AsStatement(),
                                new MarkupBlock(
                                    Factory.Markup($"{spaces}{spaces}"),
                                    BlockFactory.MarkupTagBlock("<p>", AcceptedCharacters.None),
                                    Factory.Markup("Hello World"),
                                    BlockFactory.MarkupTagBlock("</p>", AcceptedCharacters.None),
                                    Factory.Markup(newLine).Accepts(AcceptedCharacters.None)),
                                Factory.Code($"{spaces}}}").AsStatement())))),
                new RazorError(
                    LegacyResources.FormatParseError_Expected_EndOfBlock_Before_EOF("section", "}", "{"),
                    new SourceLocation(13 + newLine.Length, 1, 0),
                    length: 1));
        }

        [Fact]
        public void ParseSectionBlockReportsErrorAndAcceptsWhitespaceToEndOfLineIfSectionNotFollowedByOpenBrace()
        {
            ParseDocumentTest("@section foo      " + Environment.NewLine,
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, "      " + Environment.NewLine, markup: false).Accepts(AcceptedCharacters.AllWhiteSpace)),
                    Factory.EmptyHtml()),
                new RazorError(
                    LegacyResources.FormatUnexpectedEOFAfterDirective(CSharpCodeParser.SectionDirectiveDescriptor.Name, "{"),
                    new SourceLocation(18 + Environment.NewLine.Length, 0, 18 + Environment.NewLine.Length),
                    length: 1));
        }

        [Fact]
        public void ParseSectionBlockAcceptsOpenBraceMultipleLinesBelowSectionName()
        {
            ParseDocumentTest("@section foo      " + Environment.NewLine
                            + Environment.NewLine
                            + Environment.NewLine
                            + Environment.NewLine
                            + Environment.NewLine
                            + Environment.NewLine
                            + "{" + Environment.NewLine
                            + "<p>Foo</p>" + Environment.NewLine
                            + "}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, "      " + string.Format("{0}{0}{0}{0}{0}{0}", Environment.NewLine), markup: false).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(Environment.NewLine),
                            new MarkupTagBlock(
                                Factory.Markup("<p>")),
                            Factory.Markup("Foo"),
                            new MarkupTagBlock(
                                Factory.Markup("</p>")),
                            Factory.Markup(Environment.NewLine)),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockParsesNamedSectionCorrectly()
        {
            ParseDocumentTest("@section foo { <p>Foo</p> }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(" "),
                            new MarkupTagBlock(
                                Factory.Markup("<p>")),
                            Factory.Markup("Foo"),
                            new MarkupTagBlock(
                                Factory.Markup("</p>")),
                            Factory.Markup(" ")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockDoesNotRequireSpaceBetweenSectionNameAndOpenBrace()
        {
            ParseDocumentTest("@section foo{ <p>Foo</p> }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(" "),
                            new MarkupTagBlock(
                                Factory.Markup("<p>")),
                            Factory.Markup("Foo"),
                            new MarkupTagBlock(
                                Factory.Markup("</p>")),
                            Factory.Markup(" ")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockBalancesBraces()
        {
            ParseDocumentTest("@section foo { <script>(function foo() { return 1; })();</script> }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(" "),
                            new MarkupTagBlock(
                                Factory.Markup("<script>")),
                            Factory.Markup("(function foo() { return 1; })();"),
                            new MarkupTagBlock(
                                Factory.Markup("</script>")),
                            Factory.Markup(" ")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockAllowsBracesInCSharpExpression()
        {
            ParseDocumentTest("@section foo { I really want to render a close brace, so here I go: @(\"}\") }",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(" I really want to render a close brace, so here I go: "),
                            new ExpressionBlock(
                                Factory.CodeTransition(),
                                Factory.MetaCode("(").Accepts(AcceptedCharacters.None),
                                Factory.Code("\"}\"").AsExpression(),
                                Factory.MetaCode(")").Accepts(AcceptedCharacters.None)),
                            Factory.Markup(" ")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void SectionIsCorrectlyTerminatedWhenCloseBraceImmediatelyFollowsCodeBlock()
        {
            ParseDocumentTest("@section Foo {" + Environment.NewLine
                            + "@if(true) {" + Environment.NewLine
                            + "}" + Environment.NewLine
                            + "}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "Foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(Environment.NewLine),
                            new StatementBlock(
                                Factory.CodeTransition(),
                                Factory.Code($"if(true) {{{Environment.NewLine}}}{Environment.NewLine}").AsStatement()
                            )),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void SectionIsCorrectlyTerminatedWhenCloseBraceImmediatelyFollowsCodeBlockNoWhitespace()
        {
            ParseDocumentTest("@section Foo {" + Environment.NewLine
                            + "@if(true) {" + Environment.NewLine
                            + "}}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "Foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(Environment.NewLine),
                            new StatementBlock(
                                Factory.CodeTransition(),
                                Factory.Code($"if(true) {{{Environment.NewLine}}}").AsStatement()
                            )),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockCorrectlyTerminatesWhenCloseBraceImmediatelyFollowsMarkup()
        {
            ParseDocumentTest("@section foo {something}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "foo", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup("something")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockParsesComment()
        {
            ParseDocumentTest("@section s {<!-- -->}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "s", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup("<!-- -->")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        // This was a user reported bug (codeplex #710), the section parser wasn't handling
        // comments.
        [Fact]
        public void ParseSectionBlockParsesCommentWithDelimiters()
        {
            ParseDocumentTest("@section s {<!-- > \" '-->}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "s", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup("<!-- > \" '-->")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockCommentRecoversFromUnclosedTag()
        {
            ParseDocumentTest(
                "@section s {" + Environment.NewLine + "<a" + Environment.NewLine + "<!--  > \" '-->}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "s", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(Environment.NewLine),
                            new MarkupTagBlock(
                                Factory.Markup("<a" + Environment.NewLine)),
                            Factory.Markup("<!--  > \" '-->")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        [Fact]
        public void ParseSectionBlockParsesXmlProcessingInstruction()
        {
            ParseDocumentTest(
                "@section s { <? xml bleh ?>}",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                        Factory.CodeTransition(),
                        Factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                        Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                        Factory.Span(SpanKind.Code, "s", CSharpSymbolType.Identifier)
                            .Accepts(AcceptedCharacters.NonWhiteSpace)
                            .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                        Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                        Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                        new MarkupBlock(
                            Factory.Markup(" <? xml bleh ?>")),
                        Factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                    Factory.EmptyHtml()));
        }

        public static TheoryData SectionWithEscapedTransitionData
        {
            get
            {
                var factory = new SpanFactory();

                return new TheoryData<string, Block>
                {
                    {
                        "@section s {<span foo='@@' />}",
                        new MarkupBlock(
                            factory.EmptyHtml(),
                            new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                                factory.CodeTransition(),
                                factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                                factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                                factory.Span(SpanKind.Code, "s", CSharpSymbolType.Identifier)
                                    .Accepts(AcceptedCharacters.NonWhiteSpace)
                                    .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                                factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                                factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                            new MarkupBlock(
                                new MarkupTagBlock(
                                    factory.Markup("<span"),
                                    new MarkupBlock(
                                        new AttributeBlockChunkGenerator("foo", new LocationTagged<string>(" foo='", 17, 0, 17), new LocationTagged<string>("'", 25, 0, 25)),
                                        factory.Markup(" foo='").With(SpanChunkGenerator.Null),
                                        new MarkupBlock(
                                            factory.Markup("@").With(new LiteralAttributeChunkGenerator(new LocationTagged<string>(string.Empty, 23, 0, 23), new LocationTagged<string>("@", 23, 0, 23))).Accepts(AcceptedCharacters.None),
                                            factory.Markup("@").With(SpanChunkGenerator.Null).Accepts(AcceptedCharacters.None)),
                                        factory.Markup("'").With(SpanChunkGenerator.Null)),
                                factory.Markup(" />"))),
                            factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                            factory.EmptyHtml())
                    },
                    {
                        "@section s {<span foo='@DateTime.Now @@' />}",
                        new MarkupBlock(
                            factory.EmptyHtml(),
                            new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor),
                                factory.CodeTransition(),
                                factory.MetaCode("section").Accepts(AcceptedCharacters.None),
                                factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                                factory.Span(SpanKind.Code, "s", CSharpSymbolType.Identifier)
                                    .Accepts(AcceptedCharacters.NonWhiteSpace)
                                    .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.SectionDirectiveDescriptor.Tokens.First())),
                                factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                                factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                            new MarkupBlock(
                                new MarkupTagBlock(
                                    factory.Markup("<span"),
                                    new MarkupBlock(
                                        new AttributeBlockChunkGenerator("foo", new LocationTagged<string>(" foo='", 17, 0, 17), new LocationTagged<string>("'", 39, 0, 39)),
                                        factory.Markup(" foo='").With(SpanChunkGenerator.Null),
                                        new MarkupBlock(
                                            new DynamicAttributeBlockChunkGenerator(new LocationTagged<string>(string.Empty, 23, 0, 23), 23, 0, 23),
                                            new ExpressionBlock(
                                                factory.CodeTransition(),
                                                factory.Code("DateTime.Now")
                                                    .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                                    .Accepts(AcceptedCharacters.NonWhiteSpace))),
                                     new MarkupBlock(
                                        factory.Markup(" @").With(new LiteralAttributeChunkGenerator(new LocationTagged<string>(" ", 36, 0, 36), new LocationTagged<string>("@", 37, 0, 37))).Accepts(AcceptedCharacters.None),
                                        factory.Markup("@").With(SpanChunkGenerator.Null).Accepts(AcceptedCharacters.None)),
                                    factory.Markup("'").With(SpanChunkGenerator.Null)),
                                factory.Markup(" />"))),
                            factory.MetaCode("}").Accepts(AcceptedCharacters.None)),
                            factory.EmptyHtml())
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(SectionWithEscapedTransitionData))]
        public void ParseSectionBlock_WithDoubleTransition_DoesNotThrow(string input, object expected)
        {
            FixupSpans = true;

            ParseDocumentTest(input, (Block)expected);
        }
    }
}
