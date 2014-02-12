﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Test.Framework;
using Microsoft.AspNet.Razor.Text;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;
using Microsoft.TestCommon;

namespace Microsoft.AspNet.Razor.Test.Parser.CSharp
{
    public class CSharpAutoCompleteTest : CsHtmlCodeParserTestBase
    {
        [Fact]
        public void FunctionsDirectiveAutoCompleteAtEOF()
        {
            ParseBlockTest("@functions{",
                           new FunctionsBlock(
                               Factory.CodeTransition("@")
                                   .Accepts(AcceptedCharacters.None),
                               Factory.MetaCode("functions{")
                                   .Accepts(AcceptedCharacters.None),
                               Factory.EmptyCSharp()
                                   .AsFunctionsBody()
                                   .With(new AutoCompleteEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString)
                                   {
                                       AutoCompleteString = "}"
                                   })),
                           new RazorError(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF("functions", "}", "{"),
                                          1, 0, 1));
        }

        [Fact]
        public void HelperDirectiveAutoCompleteAtEOF()
        {
            ParseBlockTest("@helper Strong(string value) {",
                           new HelperBlock(new HelperCodeGenerator(new LocationTagged<string>("Strong(string value) {", 8, 0, 8), headerComplete: true),
                                           Factory.CodeTransition(),
                                           Factory.MetaCode("helper ")
                                               .Accepts(AcceptedCharacters.None),
                                           Factory.Code("Strong(string value) {")
                                               .Hidden()
                                               .Accepts(AcceptedCharacters.None),
                                           new StatementBlock(
                                               Factory.EmptyCSharp()
                                                   .AsStatement()
                                                   .With(new AutoCompleteEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString) { AutoCompleteString = "}" })
                                               )
                               ),
                           new RazorError(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF("helper", "}", "{"),
                                          1, 0, 1));
        }

        [Fact]
        public void SectionDirectiveAutoCompleteAtEOF()
        {
            ParseBlockTest("@section Header {",
                new SectionBlock(new SectionCodeGenerator("Header"),
                    Factory.CodeTransition(),
                    Factory.MetaCode("section Header {")
                           .AutoCompleteWith("}", atEndOfSpan: true)
                           .Accepts(AcceptedCharacters.Any),
                    new MarkupBlock()),
                new RazorError(
                    RazorResources.ParseError_Expected_X("}"),
                    17, 0, 17));
        }

        [Fact]
        public void VerbatimBlockAutoCompleteAtEOF()
        {
            ParseBlockTest("@{",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.EmptyCSharp()
                                   .AsStatement()
                                   .With(new AutoCompleteEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString) { AutoCompleteString = "}" })
                               ),
                           new RazorError(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF(RazorResources.BlockName_Code, "}", "{"),
                                          1, 0, 1));
        }

        [Fact]
        public void FunctionsDirectiveAutoCompleteAtStartOfFile()
        {
            ParseBlockTest("@functions{" + Environment.NewLine
                         + "foo",
                           new FunctionsBlock(
                               Factory.CodeTransition("@")
                                   .Accepts(AcceptedCharacters.None),
                               Factory.MetaCode("functions{")
                                   .Accepts(AcceptedCharacters.None),
                               Factory.Code("\r\nfoo")
                                   .AsFunctionsBody()
                                   .With(new AutoCompleteEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString)
                                   {
                                       AutoCompleteString = "}"
                                   })),
                           new RazorError(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF("functions", "}", "{"),
                                          1, 0, 1));
        }

        [Fact]
        public void HelperDirectiveAutoCompleteAtStartOfFile()
        {
            ParseBlockTest("@helper Strong(string value) {" + Environment.NewLine
                         + "<p></p>",
                           new HelperBlock(new HelperCodeGenerator(new LocationTagged<string>("Strong(string value) {", 8, 0, 8), headerComplete: true),
                                           Factory.CodeTransition(),
                                           Factory.MetaCode("helper ")
                                               .Accepts(AcceptedCharacters.None),
                                           Factory.Code("Strong(string value) {")
                                               .Hidden()
                                               .Accepts(AcceptedCharacters.None),
                                           new StatementBlock(
                                               Factory.Code("\r\n")
                                                   .AsStatement()
                                                   .With(new AutoCompleteEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString) { AutoCompleteString = "}" }),
                                               new MarkupBlock(
                                                   Factory.Markup(@"<p></p>")
                                                       .With(new MarkupCodeGenerator())
                                                       .Accepts(AcceptedCharacters.None)
                                                   ),
                                               Factory.Span(SpanKind.Code, new CSharpSymbol(Factory.LocationTracker.CurrentLocation, String.Empty, CSharpSymbolType.Unknown))
                                                   .With(new StatementCodeGenerator())
                                               )
                               ),
                           new RazorError(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF("helper", "}", "{"),
                                          1, 0, 1));
        }

        [Fact]
        public void SectionDirectiveAutoCompleteAtStartOfFile()
        {
            ParseBlockTest("@section Header {" + Environment.NewLine
                         + "<p>Foo</p>",
                new SectionBlock(new SectionCodeGenerator("Header"),
                    Factory.CodeTransition(),
                    Factory.MetaCode("section Header {")
                           .AutoCompleteWith("}", atEndOfSpan: true)
                           .Accepts(AcceptedCharacters.Any),
                    new MarkupBlock(
                        Factory.Markup("\r\n<p>Foo</p>"))),
                new RazorError(RazorResources.ParseError_Expected_X("}"),
                                29, 1, 10));
        }

        [Fact]
        public void VerbatimBlockAutoCompleteAtStartOfFile()
        {
            ParseBlockTest("@{" + Environment.NewLine
                         + "<p></p>",
                           new StatementBlock(
                               Factory.CodeTransition(),
                               Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                               Factory.Code("\r\n")
                                   .AsStatement()
                                   .With(new AutoCompleteEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString) { AutoCompleteString = "}" }),
                               new MarkupBlock(
                                   Factory.Markup(@"<p></p>")
                                       .With(new MarkupCodeGenerator())
                                       .Accepts(AcceptedCharacters.None)
                                   ),
                               Factory.Span(SpanKind.Code, new CSharpSymbol(Factory.LocationTracker.CurrentLocation, String.Empty, CSharpSymbolType.Unknown))
                                   .With(new StatementCodeGenerator())
                               ),
                           new RazorError(RazorResources.ParseError_Expected_EndOfBlock_Before_EOF(RazorResources.BlockName_Code, "}", "{"),
                                          1, 0, 1));
        }
    }
}
