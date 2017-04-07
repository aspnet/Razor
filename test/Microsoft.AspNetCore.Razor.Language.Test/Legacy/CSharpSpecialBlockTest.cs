// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    public class CSharpSpecialBlockTest : CsHtmlCodeParserTestBase
    {
        [Fact]
        public void ParseInheritsStatementMarksInheritsSpanAsCanGrowIfMissingTrailingSpace()
        {
            ParseBlockTest("inherits",
                new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.InheritsDirectiveDescriptor),
                    Factory.MetaCode("inherits").Accepts(AcceptedCharacters.None)),
                new RazorError(
                    LegacyResources.FormatUnexpectedEOFAfterDirective(CSharpCodeParser.InheritsDirectiveDescriptor.Name, "type"),
                    new SourceLocation(8, 0, 8), 1));
        }

        [Fact]
        public void InheritsBlockAcceptsMultipleGenericArguments()
        {
            ParseBlockTest("inherits Foo.Bar<Biz<Qux>, string, int>.Baz",
                new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.InheritsDirectiveDescriptor),
                    Factory.MetaCode("inherits").Accepts(AcceptedCharacters.None),
                    Factory.Span(SpanKind.Code, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace),
                    Factory.Span(SpanKind.Code, "Foo.Bar<Biz<Qux>, string, int>.Baz", markup: false)
                        .Accepts(AcceptedCharacters.NonWhiteSpace)
                        .With(new DirectiveTokenChunkGenerator(CSharpCodeParser.InheritsDirectiveDescriptor.Tokens.First()))));
        }

        [Fact]
        public void InheritsBlockOutputsErrorIfInheritsNotFollowedByTypeButAcceptsEntireLineAsCode()
        {
            ParseBlockTest("inherits                " + Environment.NewLine + "foo",
                new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.InheritsDirectiveDescriptor),
                    Factory.MetaCode("inherits").Accepts(AcceptedCharacters.None),
                    Factory.Span(SpanKind.Code, "                ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.WhiteSpace)),
                new RazorError(LegacyResources.FormatDirectiveExpectsTypeName(CSharpCodeParser.InheritsDirectiveDescriptor.Name), 24, 0, 24, Environment.NewLine.Length));
        }

        [Fact]
        public void NamespaceImportInsideCodeBlockCausesError()
        {
            ParseBlockTest("{ using Foo.Bar.Baz; var foo = bar; }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code(" using Foo.Bar.Baz; var foo = bar; ")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)
                               ),
                           new RazorError(
                               LegacyResources.ParseError_NamespaceImportAndTypeAlias_Cannot_Exist_Within_CodeBlock,
                               new SourceLocation(2, 0, 2),
                               length: 5));
        }

        [Fact]
        public void TypeAliasInsideCodeBlockIsNotHandledSpecially()
        {
            ParseBlockTest("{ using Foo = Bar.Baz; var foo = bar; }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code(" using Foo = Bar.Baz; var foo = bar; ")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)
                               ),
                           new RazorError(
                               LegacyResources.ParseError_NamespaceImportAndTypeAlias_Cannot_Exist_Within_CodeBlock,
                               new SourceLocation(2, 0, 2),
                               length: 5));
        }

        [Fact]
        public void Plan9FunctionsKeywordInsideCodeBlockIsNotHandledSpecially()
        {
            ParseBlockTest("{ functions Foo; }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code(" functions Foo; ")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void NonKeywordStatementInCodeBlockIsHandledCorrectly()
        {
            ParseBlockTest("{" + Environment.NewLine
                         + "    List<dynamic> photos = gallery.Photo.ToList();" + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code($"{Environment.NewLine}    List<dynamic> photos = gallery.Photo.ToList();{Environment.NewLine}")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void ParseBlockBalancesBracesOutsideStringsIfFirstCharacterIsBraceAndReturnsSpanOfTypeCode()
        {
            // Arrange
            const string code = "foo\"b}ar\" if(condition) { string.Format(\"{0}\"); } ";

            // Act/Assert
            ParseBlockTest("{" + code + "}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code(code)
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               Factory.MetaCode("}").Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void ParseBlockBalancesParensOutsideStringsIfFirstCharacterIsParenAndReturnsSpanOfTypeExpression()
        {
            // Arrange
            const string code = "foo\"b)ar\" if(condition) { string.Format(\"{0}\"); } ";

            // Act/Assert
            ParseBlockTest("(" + code + ")",
                           new ExpressionBlock(
                               Factory.MetaCode("(").Accepts(AcceptedCharacters.None),
                               Factory.Code(code).AsExpression(),
                               Factory.MetaCode(")").Accepts(AcceptedCharacters.None)
                               ));
        }

        [Fact]
        public void ParseBlockBalancesBracesAndOutputsContentAsClassLevelCodeSpanIfFirstIdentifierIsFunctionsKeyword()
        {
            const string code = " foo(); \"bar}baz\" ";
            ParseBlockTest("functions {" + code + "} zoop",
                new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.FunctionsDirectiveDescriptor),
                    Factory.MetaCode("functions").Accepts(AcceptedCharacters.None),
                    Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                    Factory.MetaCode("{").AutoCompleteWith(null, atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                    Factory.Code(code).AsStatement(),
                    Factory.MetaCode("}").Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void ParseBlockDoesNoErrorRecoveryForFunctionsBlock()
        {
            ParseBlockTest("functions { { { { { } zoop",
                new DirectiveBlock(new DirectiveChunkGenerator(CSharpCodeParser.FunctionsDirectiveDescriptor),
                    Factory.MetaCode("functions").Accepts(AcceptedCharacters.None),
                    Factory.Span(SpanKind.Markup, " ", CSharpSymbolType.WhiteSpace).Accepts(AcceptedCharacters.AllWhiteSpace),
                    Factory.MetaCode("{").AutoCompleteWith("}", atEndOfSpan: true).Accepts(AcceptedCharacters.None),
                    Factory.Code(" { { { { } zoop").AsStatement()),
                new RazorError(
                    LegacyResources.FormatParseError_Expected_EndOfBlock_Before_EOF("functions", "}", "{"),
                    new SourceLocation(10, 0, 10),
                    length: 1));
        }

        [Fact]
        public void ParseBlockIgnoresFunctionsUnlessAllLowerCase()
        {
            ParseBlockTest("Functions { foo() }",
                           new ExpressionBlock(
                               Factory.Code("Functions")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)));
        }

        [Fact]
        public void ParseBlockIgnoresSingleSlashAtStart()
        {
            ParseBlockTest("@/ foo",
                           new ExpressionBlock(
                               Factory.CodeTransition(),
                               Factory.EmptyCSharp()
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharacters.NonWhiteSpace)),
                           new RazorError(
                               LegacyResources.FormatParseError_Unexpected_Character_At_Start_Of_CodeBlock_CS("/"),
                               new SourceLocation(1, 0, 1),
                               length: 1));
        }

        [Fact]
        public void ParseBlockTerminatesSingleLineCommentAtEndOfLine()
        {
            ParseBlockTest("if(!false) {" + Environment.NewLine
                         + "    // Foo" + Environment.NewLine
                         + "\t<p>A real tag!</p>" + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.Code($"if(!false) {{{Environment.NewLine}    // Foo{Environment.NewLine}").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("\t"),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharacters.None)),
                                   Factory.Markup("A real tag!"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharacters.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharacters.None)),
                               Factory.Code("}").AsStatement()
                               ));
        }
    }
}
