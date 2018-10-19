// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    public class RazorParserTest
    {
        [Fact]
        public void CanParseStuff()
        {
            var parser = new RazorParser();
            var sourceDocument = TestRazorSourceDocument.CreateResource("TestFiles/Source/BasicMarkup.cshtml", GetType());
            var output = parser.Parse(sourceDocument, legacy: false);

            Assert.NotNull(output);
        }

        [Fact]
        public void ParseMethodCallsParseDocumentOnMarkupParserAndReturnsResults()
        {
            // Arrange
            var factory = new SpanFactory();
            var parser = new RazorParser();
            var expected =
@"RazorDocument - [0..12)::12 - [foo @bar baz]
    MarkupBlock - [0..12)::12
        MarkupTextLiteral - [0..4)::4 - [foo ] - Gen<Markup> - SpanEditHandler;Accepts:Any
            Text;[foo];
            Whitespace;[ ];
        CSharpCodeBlock - [4..8)::4
            CSharpImplicitExpression - [4..8)::4
                CSharpTransition - [4..5)::1 - Gen<None> - SpanEditHandler;Accepts:None
                    Transition;[@];
                CSharpImplicitExpressionBody - [5..8)::3
                    CSharpCodeBlock - [5..8)::3
                        CSharpExpressionLiteral - [5..8)::3 - [bar] - Gen<Expr> - ImplicitExpressionEditHandler;Accepts:NonWhitespace;ImplicitExpression[RTD];K14
                            Identifier;[bar];
        MarkupTextLiteral - [8..12)::4 - [ baz] - Gen<Markup> - SpanEditHandler;Accepts:Any
            Whitespace;[ ];
            Text;[baz];
";

            // Act
            var syntaxTree = parser.Parse(TestRazorSourceDocument.Create("foo @bar baz"), legacy: false);

            // Assert
            var actual = SyntaxNodeSerializer.Serialize(syntaxTree.Root);
            Assert.Equal(expected, actual);
        }

        [Fact(Skip = "Uses old tree")]
        public void Parse_SyntaxTreeSpansAreLinked()
        {
            // Arrange
            var factory = new SpanFactory();
            var parser = new RazorParser();

            // Act
            var results = parser.Parse(TestRazorSourceDocument.Create("foo @bar baz"));

            // Assert
            var spans = results.LegacyRoot.Flatten().ToArray();
            for (var i = 0; i < spans.Length - 1; i++)
            {
                Assert.Same(spans[i + 1], spans[i].Next);
            }

            for (var i = spans.Length - 1; i > 0; i--)
            {
                Assert.Same(spans[i - 1], spans[i].Previous);
            }
        }
    }
}
