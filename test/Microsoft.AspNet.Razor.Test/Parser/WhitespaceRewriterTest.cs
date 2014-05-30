// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Test.Framework;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Parser
{
    public class WhitespaceRewriterTest
    {
        [Fact]
        public void Constructor_Requires_NonNull_SymbolConverter()
        {
            Assert.Throws<ArgumentNullException>("markupSpanFactory", () => new WhiteSpaceRewriter(null));
        }

        [Fact]
        public void Rewrite_Moves_Whitespace_Preceeding_ExpressionBlock_To_Parent_Block()
        {
            // Arrange
            var factory = SpanFactory.CreateCsHtml();
            Block start = new MarkupBlock(
                factory.Markup("test"),
                new ExpressionBlock(
                    factory.Code("    ").AsExpression(),
                    factory.CodeTransition(SyntaxConstants.TransitionString),
                    factory.Code("foo").AsExpression()
                    ),
                factory.Markup("test")
                );
            WhiteSpaceRewriter rewriter = new WhiteSpaceRewriter(new HtmlMarkupParser().BuildSpan);

            // Act
            Block actual = rewriter.Rewrite(start);

            factory.Reset();

            // Assert
            ParserTestBase.EvaluateParseTree(actual, new MarkupBlock(
                                                         factory.Markup("test"),
                                                         factory.Markup("    "),
                                                         new ExpressionBlock(
                                                             factory.CodeTransition(SyntaxConstants.TransitionString),
                                                             factory.Code("foo").AsExpression()
                                                             ),
                                                         factory.Markup("test")
                                                         ));
        }
    }
}
