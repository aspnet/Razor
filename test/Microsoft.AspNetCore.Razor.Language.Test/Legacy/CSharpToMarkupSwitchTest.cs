// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    public class CSharpToMarkupSwitchTest : CsHtmlCodeParserTestBase
    {
        [Fact]
        public void SingleAngleBracketDoesNotCauseSwitchIfOuterBlockIsTerminated()
        {
            ParseBlockTest("{ List< }",
                new StatementBlock(
                    Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                    Factory.Code(" List< ")
                        .AsStatement()
                        .AutoCompleteWith(autoCompleteString: null),
                    Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void ParseBlockGivesSpacesToCodeOnAtTagTemplateTransitionInDesignTimeMode()
        {
            ParseBlockTest("Foo(    @<p>Foo</p>    )",
                           new ExpressionBlock(
                               Factory.Code("Foo(    ")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharactersInternal.Any),
                               new TemplateBlock(
                                   new MarkupBlock(
                                       Factory.MarkupTransition(),
                                       new MarkupTagBlock(
                                            Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                       Factory.Markup("Foo"),
                                       new MarkupTagBlock(
                                           Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None))
                                       )
                                   ),
                               Factory.Code("    )")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharactersInternal.NonWhiteSpace)
                               ), designTime: true);
        }

        [Fact]
        public void ParseBlockGivesSpacesToCodeOnAtColonTemplateTransitionInDesignTimeMode()
        {
            ParseBlockTest("Foo(    " + Environment.NewLine
                         + "@:<p>Foo</p>    " + Environment.NewLine
                         + ")",
                           new ExpressionBlock(
                               Factory.Code("Foo(    " + Environment.NewLine).AsImplicitExpression(CSharpCodeParser.DefaultKeywords),
                               new TemplateBlock(
                                   new MarkupBlock(
                                       Factory.MarkupTransition(),
                                       Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                       Factory.Markup("<p>Foo</p>    " + Environment.NewLine)
                                           .With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharactersInternal.None))
                                       )
                                   ),
                               Factory.Code(")")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharactersInternal.NonWhiteSpace)
                               ), designTime: true);
        }

        [Fact]
        public void ParseBlockGivesSpacesToCodeOnTagTransitionInDesignTimeMode()
        {
            ParseBlockTest("{" + Environment.NewLine
                         + "    <p>Foo</p>    " + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                               Factory.Code(Environment.NewLine + "    ")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               new MarkupBlock(
                                        new MarkupTagBlock(
                                            Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                       Factory.Markup("Foo"),
                                       new MarkupTagBlock(
                                           Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None))
                                   ),
                               Factory.Code("    " + Environment.NewLine).AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)
                               ), designTime: true);
        }

        [Fact]
        public void ParseBlockGivesSpacesToCodeOnInvalidAtTagTransitionInDesignTimeMode()
        {
            ParseBlockTest("{" + Environment.NewLine
                         + "    @<p>Foo</p>    " + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                               Factory.Code(Environment.NewLine + "    ")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               new MarkupBlock(
                                   Factory.MarkupTransition(),
                                        new MarkupTagBlock(
                                            Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                       Factory.Markup("Foo"),
                                       new MarkupTagBlock(
                                           Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None))
                                   ),
                               Factory.Code("    " + Environment.NewLine).AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)
                               ), true,
                           RazorDiagnosticFactory.CreateParsing_AtInCodeMustBeFollowedByColonParenOrIdentifierStart(
                               new SourceSpan(new SourceLocation(5 + Environment.NewLine.Length, 1, 4), contentLength: 1)));
        }

        [Fact]
        public void ParseBlockGivesSpacesToCodeOnAtColonTransitionInDesignTimeMode()
        {
            ParseBlockTest("{" + Environment.NewLine
                         + "    @:<p>Foo</p>    " + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                               Factory.Code(Environment.NewLine + "    ")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               new MarkupBlock(
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("<p>Foo</p>    " + Environment.NewLine)
                                       .With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharactersInternal.None))
                                   ),
                               Factory.EmptyCSharp().AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)
                               ), designTime: true);
        }

        [Fact]
        public void ParseBlockShouldSupportSingleLineMarkupContainingStatementBlock()
        {
            ParseBlockTest("Repeat(10," + Environment.NewLine
                         + "    @: @{}" + Environment.NewLine
                         + ")",
                           new ExpressionBlock(
                               Factory.Code($"Repeat(10,{Environment.NewLine}    ")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords),
                               new TemplateBlock(
                                   new MarkupBlock(
                                       Factory.MarkupTransition(),
                                       Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                       Factory.Markup(" ")
                                           .With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString)),
                                       new StatementBlock(
                                           Factory.CodeTransition(),
                                           Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                                           Factory.EmptyCSharp()
                                               .AsStatement()
                                               .AutoCompleteWith(autoCompleteString: null),
                                           Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)
                                           ),
                                       Factory.Markup(Environment.NewLine)
                                           .Accepts(AcceptedCharactersInternal.None)
                                   )
                               ),
                               Factory.Code(")")
                                   .AsImplicitExpression(CSharpCodeParser.DefaultKeywords)
                                   .Accepts(AcceptedCharactersInternal.NonWhiteSpace)
                               ));
        }

        [Fact]
        public void ParseBlockShouldSupportMarkupWithoutPreceedingWhitespace()
        {
            ParseBlockTest("foreach(var file in files){" + Environment.NewLine
                         + Environment.NewLine
                         + Environment.NewLine
                         + "@:Baz" + Environment.NewLine
                         + "<br/>" + Environment.NewLine
                         + "<a>Foo</a>" + Environment.NewLine
                         + "@:Bar" + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.Code(string.Format("foreach(var file in files){{{0}{0}{0}", Environment.NewLine)).AsStatement(),
                               new MarkupBlock(
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Baz" + Environment.NewLine)
                                       .With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharactersInternal.None))
                                   ),
                               new MarkupBlock(
                                   new MarkupTagBlock(
                                        Factory.Markup("<br/>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               new MarkupBlock(
                                   new MarkupTagBlock(
                                       Factory.Markup("<a>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Foo"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</a>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)),
                               new MarkupBlock(
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Bar" + Environment.NewLine)
                                       .With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharactersInternal.None))
                                   ),
                               Factory.Code("}").AsStatement().Accepts(AcceptedCharactersInternal.None)
                               ));
        }

        [Fact]
        public void ParseBlockGivesAllWhitespaceOnSameLineExcludingPreceedingNewlineButIncludingTrailingNewLineToMarkup()
        {
            ParseBlockTest("if(foo) {" + Environment.NewLine
                         + "    var foo = \"After this statement there are 10 spaces\";          " + Environment.NewLine
                         + "    <p>" + Environment.NewLine
                         + "        Foo" + Environment.NewLine
                         + "        @bar" + Environment.NewLine
                         + "    </p>" + Environment.NewLine
                         + "    @:Hello!" + Environment.NewLine
                         + "    var biz = boz;" + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.Code(
                                   $"if(foo) {{{Environment.NewLine}    var foo = \"After this statement there are " +
                                   "10 spaces\";          " + Environment.NewLine).AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("    "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup($"{Environment.NewLine}        Foo{Environment.NewLine}"),
                                   new ExpressionBlock(
                                       Factory.Code("        ").AsStatement(),
                                       Factory.CodeTransition(),
                                       Factory.Code("bar").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharactersInternal.NonWhiteSpace)
                                       ),
                                   Factory.Markup(Environment.NewLine + "    "),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               new MarkupBlock(
                                   Factory.Markup("    "),
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Hello!" + Environment.NewLine).With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharactersInternal.None))
                                   ),
                               Factory.Code($"    var biz = boz;{Environment.NewLine}}}").AsStatement()));
        }

        [Fact]
        public void ParseBlockAllowsMarkupInIfBodyWithBraces()
        {
            ParseBlockTest("if(foo) { <p>Bar</p> } else if(bar) { <p>Baz</p> } else { <p>Boz</p> }",
                           new StatementBlock(
                               Factory.Code("if(foo) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                    new MarkupTagBlock(
                                        Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup("Bar"),
                                    new MarkupTagBlock(
                                        Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup(" ").Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("} else if(bar) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                    new MarkupTagBlock(
                                        Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup("Baz"),
                                    new MarkupTagBlock(
                                        Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup(" ").Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("} else {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                    new MarkupTagBlock(
                                        Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup("Boz"),
                                    new MarkupTagBlock(
                                        Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup(" ").Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("}").AsStatement().Accepts(AcceptedCharactersInternal.None)
                               ));
        }

        [Fact]
        public void ParseBlockAllowsMarkupInIfBodyWithBracesWithinCodeBlock()
        {
            ParseBlockTest("{ if(foo) { <p>Bar</p> } else if(bar) { <p>Baz</p> } else { <p>Boz</p> } }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                               Factory.Code(" if(foo) {")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                    new MarkupTagBlock(
                                        Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup("Bar"),
                                    new MarkupTagBlock(
                                        Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup(" ").Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("} else if(bar) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                    new MarkupTagBlock(
                                        Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup("Baz"),
                                    new MarkupTagBlock(
                                        Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup(" ").Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("} else {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                    new MarkupTagBlock(
                                        Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup("Boz"),
                                    new MarkupTagBlock(
                                        Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup(" ").Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("} ").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)
                               ));
        }

        [Fact]
        public void ParseBlockSupportsMarkupInCaseAndDefaultBranchesOfSwitch()
        {
            // Arrange
            ParseBlockTest("switch(foo) {" + Environment.NewLine
                         + "    case 0:" + Environment.NewLine
                         + "        <p>Foo</p>" + Environment.NewLine
                         + "        break;" + Environment.NewLine
                         + "    case 1:" + Environment.NewLine
                         + "        <p>Bar</p>" + Environment.NewLine
                         + "        return;" + Environment.NewLine
                         + "    case 2:" + Environment.NewLine
                         + "        {" + Environment.NewLine
                         + "            <p>Baz</p>" + Environment.NewLine
                         + "            <p>Boz</p>" + Environment.NewLine
                         + "        }" + Environment.NewLine
                         + "    default:" + Environment.NewLine
                         + "        <p>Biz</p>" + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.Code($"switch(foo) {{{Environment.NewLine}    case 0:{Environment.NewLine}").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Foo"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code($"        break;{Environment.NewLine}    case 1:{Environment.NewLine}").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Bar"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code(
                                $"        return;{Environment.NewLine}    case 2:{Environment.NewLine}" +
                                "        {" + Environment.NewLine).AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("            "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Baz"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               new MarkupBlock(
                                   Factory.Markup("            "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Boz"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code($"        }}{Environment.NewLine}    default:{Environment.NewLine}").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Biz"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("}").AsStatement().Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void ParseBlockSupportsMarkupInCaseAndDefaultBranchesOfSwitchInCodeBlock()
        {
            // Arrange
            ParseBlockTest("{ switch(foo) {" + Environment.NewLine
                         + "    case 0:" + Environment.NewLine
                         + "        <p>Foo</p>" + Environment.NewLine
                         + "        break;" + Environment.NewLine
                         + "    case 1:" + Environment.NewLine
                         + "        <p>Bar</p>" + Environment.NewLine
                         + "        return;" + Environment.NewLine
                         + "    case 2:" + Environment.NewLine
                         + "        {" + Environment.NewLine
                         + "            <p>Baz</p>" + Environment.NewLine
                         + "            <p>Boz</p>" + Environment.NewLine
                         + "        }" + Environment.NewLine
                         + "    default:" + Environment.NewLine
                         + "        <p>Biz</p>" + Environment.NewLine
                         + "} }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                               Factory.Code($" switch(foo) {{{Environment.NewLine}    case 0:{Environment.NewLine}")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               new MarkupBlock(
                                   Factory.Markup("        "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Foo"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code($"        break;{Environment.NewLine}    case 1:{Environment.NewLine}").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Bar"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code(
                                $"        return;{Environment.NewLine}    case 2:{Environment.NewLine}" +
                                "        {" + Environment.NewLine).AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("            "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Baz"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               new MarkupBlock(
                                   Factory.Markup("            "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Boz"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code($"        }}{Environment.NewLine}    default:{Environment.NewLine}").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("Biz"),
                                   new MarkupTagBlock(
                                       Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("} ").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void ParseBlockParsesMarkupStatementOnOpenAngleBracket()
        {
            ParseBlockTest("for(int i = 0; i < 10; i++) { <p>Foo</p> }",
                           new StatementBlock(
                               Factory.Code("for(int i = 0; i < 10; i++) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                    new MarkupTagBlock(
                                        Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup("Foo"),
                                    new MarkupTagBlock(
                                        Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup(" ").Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("}").AsStatement().Accepts(AcceptedCharactersInternal.None)
                               ));
        }

        [Fact]
        public void ParseBlockParsesMarkupStatementOnOpenAngleBracketInCodeBlock()
        {
            ParseBlockTest("{ for(int i = 0; i < 10; i++) { <p>Foo</p> } }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                               Factory.Code(" for(int i = 0; i < 10; i++) {")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               new MarkupBlock(
                                    Factory.Markup(" "),
                                    new MarkupTagBlock(
                                        Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup("Foo"),
                                    new MarkupTagBlock(
                                        Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup(" ").Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("} ").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void ParseBlockParsesMarkupStatementOnSwitchCharacterFollowedByColon()
        {
            // Arrange
            ParseBlockTest("if(foo) { @:Bar" + Environment.NewLine
                         + "} zoop",
                           new StatementBlock(
                               Factory.Code("if(foo) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Bar" + Environment.NewLine)
                                    .With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharactersInternal.None))
                                   ),
                               Factory.Code("}").AsStatement()));
        }

        [Fact]
        public void ParseBlockParsesMarkupStatementOnSwitchCharacterFollowedByDoubleColon()
        {
            // Arrange
            ParseBlockTest("if(foo) { @::Sometext" + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.Code("if(foo) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup(":Sometext" + Environment.NewLine)
                                    .With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharactersInternal.None))
                                   ),
                               Factory.Code("}").AsStatement()));
        }


        [Fact]
        public void ParseBlockParsesMarkupStatementOnSwitchCharacterFollowedByTripleColon()
        {
            // Arrange
            ParseBlockTest("if(foo) { @:::Sometext" + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.Code("if(foo) {").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("::Sometext" + Environment.NewLine)
                                    .With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharactersInternal.None))
                                   ),
                               Factory.Code("}").AsStatement()));
        }

        [Fact]
        public void ParseBlockParsesMarkupStatementOnSwitchCharacterFollowedByColonInCodeBlock()
        {
            // Arrange
            ParseBlockTest("{ if(foo) { @:Bar" + Environment.NewLine
                         + "} } zoop",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                               Factory.Code(" if(foo) {")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               new MarkupBlock(
                                   Factory.Markup(" "),
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Bar" + Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                       .With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharactersInternal.None))
                                   ),
                               Factory.Code("} ").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void ParseBlockCorrectlyReturnsFromMarkupBlockWithPseudoTag()
        {
            ParseBlockTest("if (i > 0) { <text>;</text> }",
                           new StatementBlock(
                               Factory.Code("if (i > 0) { ").AsStatement(),
                               new MarkupBlock(
                                   new MarkupTagBlock(
                                        Factory.MarkupTransition("<text>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(";").Accepts(AcceptedCharactersInternal.None),
                                   new MarkupTagBlock(
                                        Factory.MarkupTransition("</text>").Accepts(AcceptedCharactersInternal.None))),
                               Factory.Code(" }").AsStatement()));
        }

        [Fact]
        public void ParseBlockCorrectlyReturnsFromMarkupBlockWithPseudoTagInCodeBlock()
        {
            ParseBlockTest("{ if (i > 0) { <text>;</text> } }",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                               Factory.Code(" if (i > 0) { ")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               new MarkupBlock(
                                   new MarkupTagBlock(
                                        Factory.MarkupTransition("<text>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup(";").Accepts(AcceptedCharactersInternal.None),
                                   new MarkupTagBlock(
                                        Factory.MarkupTransition("</text>").Accepts(AcceptedCharactersInternal.None))),
                               Factory.Code(" } ").AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }

        [Fact]
        public void ParseBlockSupportsAllKindsOfImplicitMarkupInCodeBlock()
        {
            ParseBlockTest("{" + Environment.NewLine
                         + "    if(true) {" + Environment.NewLine
                         + "        @:Single Line Markup" + Environment.NewLine
                         + "    }" + Environment.NewLine
                         + "    foreach (var p in Enumerable.Range(1, 10)) {" + Environment.NewLine
                         + "        <text>The number is @p</text>" + Environment.NewLine
                         + "    }" + Environment.NewLine
                         + "    if(!false) {" + Environment.NewLine
                         + "        <p>A real tag!</p>" + Environment.NewLine
                         + "    }" + Environment.NewLine
                         + "}",
                           new StatementBlock(
                               Factory.MetaCode("{").Accepts(AcceptedCharactersInternal.None),
                               Factory.Code($"{Environment.NewLine}    if(true) {{{Environment.NewLine}")
                                   .AsStatement()
                                   .AutoCompleteWith(autoCompleteString: null),
                               new MarkupBlock(
                                   Factory.Markup("        "),
                                   Factory.MarkupTransition(),
                                   Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                                   Factory.Markup("Single Line Markup" + Environment.NewLine)
                                    .With(new SpanEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharactersInternal.None))
                                   ),
                               Factory.Code($"    }}{Environment.NewLine}    foreach (var p in Enumerable.Range(1, 10)) {{{Environment.NewLine}        ").AsStatement(),
                               new MarkupBlock(
                                   new MarkupTagBlock(
                                        Factory.MarkupTransition("<text>").Accepts(AcceptedCharactersInternal.None)),
                                   Factory.Markup("The number is ").Accepts(AcceptedCharactersInternal.None),
                                   new ExpressionBlock(
                                       Factory.CodeTransition(),
                                       Factory.Code("p").AsImplicitExpression(CSharpCodeParser.DefaultKeywords).Accepts(AcceptedCharactersInternal.NonWhiteSpace)
                                       ),
                                   new MarkupTagBlock(
                                        Factory.MarkupTransition("</text>").Accepts(AcceptedCharactersInternal.None))),
                               Factory.Code($"{Environment.NewLine}    }}{Environment.NewLine}    if(!false) {{{Environment.NewLine}").AsStatement(),
                               new MarkupBlock(
                                   Factory.Markup("        "),
                                   new MarkupTagBlock(
                                       Factory.Markup("<p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup("A real tag!"),
                                    new MarkupTagBlock(
                                        Factory.Markup("</p>").Accepts(AcceptedCharactersInternal.None)),
                                    Factory.Markup(Environment.NewLine).Accepts(AcceptedCharactersInternal.None)
                                   ),
                               Factory.Code("    }" + Environment.NewLine).AsStatement(),
                               Factory.MetaCode("}").Accepts(AcceptedCharactersInternal.None)));
        }
    }
}
