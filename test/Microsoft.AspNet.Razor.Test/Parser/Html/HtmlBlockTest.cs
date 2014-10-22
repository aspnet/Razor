// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor.Editor;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Test.Framework;
using Microsoft.AspNet.Razor.Text;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Parser.Html
{
    public class HtmlBlockTest : CsHtmlMarkupParserTestBase
    {
        [Fact]
        public void ParseBlockMethodThrowsArgNullExceptionOnNullContext()
        {
            // Arrange
            var parser = new HtmlMarkupParser();

            // Act and Assert
            var exception = Assert.Throws<InvalidOperationException>(() => parser.ParseBlock());
            Assert.Equal(RazorResources.Parser_Context_Not_Set, exception.Message);
        }

        [Fact]
        public void ParseBlockHandlesOpenAngleAtEof()
        {
            ParseDocumentTest("@{" + Environment.NewLine
                            + "<",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        Factory.Code("\r\n").AsStatement(),
                        new MarkupBlock(
                            new MarkupTagBlock(
                                Factory.Markup("<"))))),
                new RazorError(
                    RazorResources.FormatParseError_Expected_EndOfBlock_Before_EOF(RazorResources.BlockName_Code, "}", "{"),
                    1, 0, 1));
        }

        [Fact]
        public void ParseBlockHandlesOpenAngleWithProperTagFollowingIt()
        {
            ParseDocumentTest("@{" + Environment.NewLine
                            + "<" + Environment.NewLine
                            + "</html>",
                new MarkupBlock(
                    Factory.EmptyHtml(),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.MetaCode("{").Accepts(AcceptedCharacters.None),
                        Factory.Code("\r\n").AsStatement(),
                        new MarkupBlock(
                            new MarkupTagBlock(
                                Factory.Markup("<\r\n"))
                        ),
                        new MarkupBlock(
                            new MarkupTagBlock(
                                Factory.Markup("</html>").Accepts(AcceptedCharacters.None))
                        ),
                        Factory.EmptyCSharp().AsStatement()
                    )
                ),
                designTimeParser: true,
                expectedErrors: new[]
                {
                    new RazorError(RazorResources.FormatParseError_UnexpectedEndTag("html"), 7, 2, 0),
                    new RazorError(RazorResources.FormatParseError_Expected_EndOfBlock_Before_EOF("code", "}", "{"), 1, 0, 1)
                });
        }

        [Fact]
        public void TagWithoutCloseAngleDoesNotTerminateBlock()
        {
            ParseBlockTest("<                      " + Environment.NewLine
                         + "   ",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<                      \r\n   "))),
                designTimeParser: true,
                expectedErrors: new RazorError(RazorResources.FormatParseError_UnfinishedTag(string.Empty), 0, 0, 0));
        }

        [Fact]
        public void ParseBlockAllowsStartAndEndTagsToDifferInCase()
        {
            ParseBlockTest("<li><p>Foo</P></lI>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<li>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<p>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("Foo"),
                    new MarkupTagBlock(
                        Factory.Markup("</P>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</lI>").Accepts(AcceptedCharacters.None))
                    ));
        }

        [Fact]
        public void ParseBlockReadsToEndOfLineIfFirstCharacterAfterTransitionIsColon()
        {
            ParseBlockTest("@:<li>Foo Bar Baz" + Environment.NewLine
                         + "bork",
                new MarkupBlock(
                    Factory.MarkupTransition(),
                    Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                    Factory.Markup("<li>Foo Bar Baz\r\n")
                           .With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString, AcceptedCharacters.None))
                ));
        }

        [Fact]
        public void ParseBlockStopsParsingSingleLineBlockAtEOFIfNoEOLReached()
        {
            ParseBlockTest("@:foo bar",
                new MarkupBlock(
                    Factory.MarkupTransition(),
                    Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                    Factory.Markup(@"foo bar")
                           .With(new SingleLineMarkupEditHandler(CSharpLanguageCharacteristics.Instance.TokenizeString))
                    ));
        }

        [Fact]
        public void ParseBlockStopsAtMatchingCloseTagToStartTag()
        {
            ParseBlockTest("<a><b></b></a><c></c>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<a>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<b>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</b>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</a>").Accepts(AcceptedCharacters.None))
                    ));
        }

        [Fact]
        public void ParseBlockParsesUntilMatchingEndTagIfFirstNonWhitespaceCharacterIsStartTag()
        {
            ParseBlockTest("<baz><boz><biz></biz></boz></baz>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<baz>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<boz>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<biz>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</biz>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</boz>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</baz>").Accepts(AcceptedCharacters.None))
                    ));
        }

        [Fact]
        public void ParseBlockAllowsUnclosedTagsAsLongAsItCanRecoverToAnExpectedEndTag()
        {
            ParseBlockTest("<foo><bar><baz></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<bar>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<baz>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))
                    ));
        }

        [Fact]
        public void ParseBlockWithSelfClosingTagJustEmitsTag()
        {
            ParseBlockTest("<foo />",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo />").Accepts(AcceptedCharacters.None))
                    ));
        }

        [Fact]
        public void ParseBlockCanHandleSelfClosingTagsWithinBlock()
        {
            ParseBlockTest("<foo><bar /></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<bar />").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))
                    ));
        }

        [Fact]
        public void ParseBlockSupportsTagsWithAttributes()
        {
            ParseBlockTest("<foo bar=\"baz\"><biz><boz zoop=zork/></biz></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo"),
                        new MarkupBlock(new AttributeBlockCodeGenerator("bar", new LocationTagged<string>(" bar=\"", 4, 0, 4), new LocationTagged<string>("\"", 13, 0, 13)),
                            Factory.Markup(" bar=\"").With(SpanCodeGenerator.Null),
                            Factory.Markup("baz").With(new LiteralAttributeCodeGenerator(new LocationTagged<string>(String.Empty, 10, 0, 10), new LocationTagged<string>("baz", 10, 0, 10))),
                            Factory.Markup("\"").With(SpanCodeGenerator.Null)),
                        Factory.Markup(">").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<biz>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<boz"),
                        new MarkupBlock(new AttributeBlockCodeGenerator("zoop", new LocationTagged<string>(" zoop=", 24, 0, 24), new LocationTagged<string>(String.Empty, 34, 0, 34)),
                            Factory.Markup(" zoop=").With(SpanCodeGenerator.Null),
                            Factory.Markup("zork").With(new LiteralAttributeCodeGenerator(new LocationTagged<string>(String.Empty, 30, 0, 30), new LocationTagged<string>("zork", 30, 0, 30)))),
                        Factory.Markup("/>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</biz>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockAllowsCloseAngleBracketInAttributeValueIfDoubleQuoted()
        {
            ParseBlockTest("<foo><bar baz=\">\" /></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<bar"),
                        new MarkupBlock(new AttributeBlockCodeGenerator("baz", new LocationTagged<string>(" baz=\"", 9, 0, 9), new LocationTagged<string>("\"", 16, 0, 16)),
                            Factory.Markup(" baz=\"").With(SpanCodeGenerator.Null),
                            Factory.Markup(">").With(new LiteralAttributeCodeGenerator(new LocationTagged<string>(String.Empty, 15, 0, 15), new LocationTagged<string>(">", 15, 0, 15))),
                            Factory.Markup("\"").With(SpanCodeGenerator.Null)),
                        Factory.Markup(" />").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockAllowsCloseAngleBracketInAttributeValueIfSingleQuoted()
        {
            ParseBlockTest("<foo><bar baz=\'>\' /></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<bar"),
                        new MarkupBlock(new AttributeBlockCodeGenerator("baz", new LocationTagged<string>(" baz='", 9, 0, 9), new LocationTagged<string>("'", 16, 0, 16)),
                            Factory.Markup(" baz='").With(SpanCodeGenerator.Null),
                            Factory.Markup(">").With(new LiteralAttributeCodeGenerator(new LocationTagged<string>(String.Empty, 15, 0, 15), new LocationTagged<string>(">", 15, 0, 15))),
                            Factory.Markup("'").With(SpanCodeGenerator.Null)),
                        Factory.Markup(" />").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockAllowsSlashInAttributeValueIfDoubleQuoted()
        {
            ParseBlockTest("<foo><bar baz=\"/\"></bar></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<bar"),
                        new MarkupBlock(new AttributeBlockCodeGenerator("baz", new LocationTagged<string>(" baz=\"", 9, 0, 9), new LocationTagged<string>("\"", 16, 0, 16)),
                            Factory.Markup(" baz=\"").With(SpanCodeGenerator.Null),
                            Factory.Markup("/").With(new LiteralAttributeCodeGenerator(new LocationTagged<string>(String.Empty, 15, 0, 15), new LocationTagged<string>("/", 15, 0, 15))),
                            Factory.Markup("\"").With(SpanCodeGenerator.Null)),
                        Factory.Markup(">").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</bar>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockAllowsSlashInAttributeValueIfSingleQuoted()
        {
            ParseBlockTest("<foo><bar baz=\'/\'></bar></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<bar"),
                        new MarkupBlock(new AttributeBlockCodeGenerator("baz", new LocationTagged<string>(" baz='", 9, 0, 9), new LocationTagged<string>("'", 16, 0, 16)),
                            Factory.Markup(" baz='").With(SpanCodeGenerator.Null),
                            Factory.Markup("/").With(new LiteralAttributeCodeGenerator(new LocationTagged<string>(String.Empty, 15, 0, 15), new LocationTagged<string>("/", 15, 0, 15))),
                            Factory.Markup("'").With(SpanCodeGenerator.Null)),
                        Factory.Markup(">").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</bar>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockTerminatesAtEOF()
        {
            ParseBlockTest("<foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None))),
                new RazorError(RazorResources.FormatParseError_MissingEndTag("foo"), new SourceLocation(0, 0, 0)));
        }

        [Fact]
        public void ParseBlockSupportsCommentAsBlock()
        {
            SingleSpanBlockTest("<!-- foo -->", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [Fact]
        public void ParseBlockSupportsCommentWithinBlock()
        {
            ParseBlockTest("<foo>bar<!-- zoop -->baz</foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("bar"), 
                    Factory.Markup("<!-- zoop -->").Accepts(AcceptedCharacters.None),
                    Factory.Markup("baz"),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockProperlyBalancesCommentStartAndEndTags()
        {
            SingleSpanBlockTest("<!--<foo></bar>-->", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [Fact]
        public void ParseBlockTerminatesAtEOFWhenParsingComment()
        {
            SingleSpanBlockTest("<!--<foo>", "<!--<foo>", BlockType.Markup, SpanKind.Markup);
        }

        [Fact]
        public void ParseBlockOnlyTerminatesCommentOnFullEndSequence()
        {
            SingleSpanBlockTest("<!--<foo>--</bar>-->", BlockType.Markup, SpanKind.Markup, acceptedCharacters: AcceptedCharacters.None);
        }

        [Fact]
        public void ParseBlockTerminatesCommentAtFirstOccurrenceOfEndSequence()
        {
            ParseBlockTest("<foo><!--<foo></bar-->--></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("<!--<foo></bar-->").Accepts(AcceptedCharacters.None),
                    Factory.Markup("-->"),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockTreatsMalformedTagsAsContent()
        {
            ParseBlockTest("<foo></!-- bar --></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</!-- bar -->").Accepts(AcceptedCharacters.None))),
                new RazorError(RazorResources.FormatParseError_MissingEndTag("foo"), 0, 0, 0));
        }


        [Fact]
        public void ParseBlockParsesSGMLDeclarationAsEmptyTag()
        {
            ParseBlockTest("<foo><!DOCTYPE foo bar baz></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("<!DOCTYPE foo bar baz>").Accepts(AcceptedCharacters.None),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockTerminatesSGMLDeclarationAtFirstCloseAngle()
        {
            ParseBlockTest("<foo><!DOCTYPE foo bar> baz></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("<!DOCTYPE foo bar>").Accepts(AcceptedCharacters.None),
                    Factory.Markup(" baz>"),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockParsesXMLProcessingInstructionAsEmptyTag()
        {
            ParseBlockTest("<foo><?xml foo bar baz?></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("<?xml foo bar baz?>").Accepts(AcceptedCharacters.None),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockTerminatesXMLProcessingInstructionAtQuestionMarkCloseAnglePair()
        {
            ParseBlockTest("<foo><?xml foo bar baz?> baz</foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("<?xml foo bar baz?>").Accepts(AcceptedCharacters.None),
                    Factory.Markup(" baz"),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockDoesNotTerminateXMLProcessingInstructionAtCloseAngleUnlessPreceededByQuestionMark()
        {
            ParseBlockTest("<foo><?xml foo bar> baz?></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("<?xml foo bar> baz?>").Accepts(AcceptedCharacters.None),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockSupportsScriptTagsWithLessThanSignsInThem()
        {
            ParseBlockTest(@"<script>if(foo<bar) { alert(""baz"");)</script>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<script>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup(@"if(foo<bar) { alert(""baz"");)"),
                    new MarkupTagBlock(
                        Factory.Markup("</script>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockSupportsScriptTagsWithSpacedLessThanSignsInThem()
        {
            ParseBlockTest(@"<script>if(foo < bar) { alert(""baz"");)</script>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<script>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup(@"if(foo < bar) { alert(""baz"");)"),
                    new MarkupTagBlock(
                        Factory.Markup("</script>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockAcceptsEmptyTextTag()
        {
            ParseBlockTest("<text/>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.MarkupTransition("<text/>"))
                ));
        }

        [Fact]
        public void ParseBlockAcceptsTextTagAsOuterTagButDoesNotRender()
        {
            ParseBlockTest("<text>Foo Bar <foo> Baz</text> zoop",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.MarkupTransition("<text>")),
                    Factory.Markup("Foo Bar ").Accepts(AcceptedCharacters.None),
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup(" Baz"),
                    new MarkupTagBlock(
                        Factory.MarkupTransition("</text>")),
                    Factory.Markup(" ").Accepts(AcceptedCharacters.None)
                ));
        }

        [Fact]
        public void ParseBlockRendersLiteralTextTagIfDoubled()
        {
            ParseBlockTest("<text><text>Foo Bar <foo> Baz</text></text> zoop",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.MarkupTransition("<text>")),
                    new MarkupTagBlock(
                        Factory.Markup("<text>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("Foo Bar "),
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup(" Baz"),
                    new MarkupTagBlock(
                        Factory.Markup("</text>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.MarkupTransition("</text>")),
                    Factory.Markup(" ").Accepts(AcceptedCharacters.None)
                ));
        }

        [Fact]
        public void ParseBlockDoesNotConsiderPsuedoTagWithinMarkupBlock()
        {
            ParseBlockTest("<foo><text><bar></bar></foo>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<foo>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<text>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("<bar>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</bar>").Accepts(AcceptedCharacters.None)),
                    new MarkupTagBlock(
                        Factory.Markup("</foo>").Accepts(AcceptedCharacters.None))
                ));
        }

        [Fact]
        public void ParseBlockStopsParsingMidEmptyTagIfEOFReached()
        {
            ParseBlockTest("<br/",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<br/"))),
                new RazorError(RazorResources.FormatParseError_UnfinishedTag("br"), SourceLocation.Zero));
        }

        [Fact]
        public void ParseBlockCorrectlyHandlesSingleLineOfMarkupWithEmbeddedStatement()
        {
            ParseBlockTest("<div>Foo @if(true) {} Bar</div>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<div>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("Foo "),
                    new StatementBlock(
                        Factory.CodeTransition(),
                        Factory.Code("if(true) {}").AsStatement()),
                    Factory.Markup(" Bar"),
                    new MarkupTagBlock(
                        Factory.Markup("</div>").Accepts(AcceptedCharacters.None))));
        }

        [Fact]
        public void ParseBlockIgnoresTagsInContentsOfScriptTag()
        {
            ParseBlockTest(@"<script>foo<bar baz='@boz'></script>",
                new MarkupBlock(
                    new MarkupTagBlock(
                        Factory.Markup("<script>").Accepts(AcceptedCharacters.None)),
                    Factory.Markup("foo<bar baz='"),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("boz")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: false)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    Factory.Markup("'>"),
                    new MarkupTagBlock(
                        Factory.Markup("</script>").Accepts(AcceptedCharacters.None))));
        }
    }
}
