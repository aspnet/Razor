// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    public class HtmlToCodeSwitchTest : CsHtmlMarkupParserTestBase
    {
        [Fact]
        public void SwitchesWhenCharacterBeforeSwapIsNonAlphanumeric()
        {
            ParseBlockTest("<p>foo#@i</p>");
        }

        [Fact]
        public void SwitchesToCodeWhenSwapCharacterEncounteredMidTag()
        {
            ParseBlockTest("<foo @bar />");
        }

        [Fact]
        public void SwitchesToCodeWhenSwapCharacterEncounteredInAttributeValue()
        {
            ParseBlockTest("<foo bar=\"@baz\" />");
        }

        [Fact]
        public void SwitchesToCodeWhenSwapCharacterEncounteredInTagContent()
        {
            ParseBlockTest("<foo>@bar<baz>@boz</baz></foo>");
        }

        [Fact]
        public void ParsesCodeWithinSingleLineMarkup()
        {
            // TODO: Fix at a later date, HTML should be a tag block: https://github.com/aspnet/Razor/issues/101
            ParseBlockTest("@:<li>Foo @Bar Baz" + Environment.NewLine
                         + "bork");
        }

        [Fact]
        public void SupportsCodeWithinComment()
        {
            ParseBlockTest("<foo><!-- @foo --></foo>");
        }

        [Fact]
        public void SupportsCodeWithinSGMLDeclaration()
        {
            ParseBlockTest("<foo><!DOCTYPE foo @bar baz></foo>");
        }

        [Fact]
        public void SupportsCodeWithinCDataDeclaration()
        {
            ParseBlockTest("<foo><![CDATA[ foo @bar baz]]></foo>");
        }

        [Fact]
        public void SupportsCodeWithinXMLProcessingInstruction()
        {
            ParseBlockTest("<foo><?xml foo @bar baz?></foo>");
        }

        [Fact]
        public void ParseBlockDoesNotSwitchToCodeOnEmailAddressInText()
        {
            ParseBlockTest("<foo>anurse@microsoft.com</foo>");
        }

        [Fact]
        public void DoesNotSwitchToCodeOnEmailAddressInAttribute()
        {
            ParseBlockTest("<a href=\"mailto:anurse@microsoft.com\">Email me</a>");
        }

        [Fact]
        public void GivesWhitespacePreceedingAtToCodeIfThereIsNoMarkupOnThatLine()
        {
            ParseBlockTest("   <ul>" + Environment.NewLine
                         + "    @foreach(var p in Products) {" + Environment.NewLine
                         + "        <li>Product: @p.Name</li>" + Environment.NewLine
                         + "    }" + Environment.NewLine
                         + "    </ul>");
        }

        [Fact]
        public void ParseDocumentGivesWhitespacePreceedingAtToCodeIfThereIsNoMarkupOnThatLine()
        {
            ParseDocumentTest("   <ul>" + Environment.NewLine
                            + "    @foreach(var p in Products) {" + Environment.NewLine
                            + "        <li>Product: @p.Name</li>" + Environment.NewLine
                            + "    }" + Environment.NewLine
                            + "    </ul>");
        }

        [Fact]
        public void SectionContextGivesWhitespacePreceedingAtToCodeIfThereIsNoMarkupOnThatLine()
        {
            var sectionDescriptor = SectionDirective.Directive;
            ParseDocumentTest("@section foo {" + Environment.NewLine
                            + "    <ul>" + Environment.NewLine
                            + "        @foreach(var p in Products) {" + Environment.NewLine
                            + "            <li>Product: @p.Name</li>" + Environment.NewLine
                            + "        }" + Environment.NewLine
                            + "    </ul>" + Environment.NewLine
                            + "}",
                new[] { SectionDirective.Directive, });
        }

        [Fact]
        public void CSharpCodeParserDoesNotAcceptLeadingOrTrailingWhitespaceInDesignMode()
        {
            ParseBlockTest("   <ul>" + Environment.NewLine
                         + "    @foreach(var p in Products) {" + Environment.NewLine
                         + "        <li>Product: @p.Name</li>" + Environment.NewLine
                         + "    }" + Environment.NewLine
                         + "    </ul>",
                designTime: true);
        }

        // Tests for "@@" escape sequence:
        [Fact]
        public void TreatsTwoAtSignsAsEscapeSequence()
        {
            ParseBlockTest("<foo>@@bar</foo>");
        }

        [Fact]
        public void TreatsPairsOfAtSignsAsEscapeSequence()
        {
            ParseBlockTest("<foo>@@@@@bar</foo>");
        }

        [Fact]
        public void ParseDocumentTreatsTwoAtSignsAsEscapeSequence()
        {
            ParseDocumentTest("<foo>@@bar</foo>");
        }

        [Fact]
        public void ParseDocumentTreatsPairsOfAtSignsAsEscapeSequence()
        {
            ParseDocumentTest("<foo>@@@@@bar</foo>");
        }

        [Fact]
        public void SectionBodyTreatsTwoAtSignsAsEscapeSequence()
        {
            ParseDocumentTest(
                "@section Foo { <foo>@@bar</foo> }",
                new[] { SectionDirective.Directive, });
        }

        [Fact]
        public void SectionBodyTreatsPairsOfAtSignsAsEscapeSequence()
        {
            ParseDocumentTest(
                "@section Foo { <foo>@@@@@bar</foo> }",
                new[] { SectionDirective.Directive, });
        }
    }
}
