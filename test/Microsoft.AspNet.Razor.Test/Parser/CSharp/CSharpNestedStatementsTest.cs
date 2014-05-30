// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Test.Framework;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Parser.CSharp
{
    public class CSharpNestedStatementsTest : CsHtmlCodeParserTestBase
    {
        [Fact]
        public void NestedSimpleStatement()
        {
            ParseBlockTest("@while(true) { foo(); }",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("while(true) { foo(); }")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void NestedKeywordStatement()
        {
            ParseBlockTest("@while(true) { for(int i = 0; i < 10; i++) { foo(); } }",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("while(true) { for(int i = 0; i < 10; i++) { foo(); } }")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void NestedCodeBlock()
        {
            ParseBlockTest("@while(true) { { { { foo(); } } } }",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("while(true) { { { { foo(); } } } }")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void NestedImplicitExpression()
        {
            ParseBlockTest("@while(true) { @foo }",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("while(true) { ")
                           .AsStatement(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.Code("foo")
                               .AsImplicitExpression(CSharpCodeParser.DefaultKeywords, acceptTrailingDot: true)
                               .Accepts(AcceptedCharacters.NonWhiteSpace)),
                    Factory.Code(" }")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void NestedExplicitExpression()
        {
            ParseBlockTest("@while(true) { @(foo) }",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("while(true) { ")
                           .AsStatement(),
                    new ExpressionBlock(
                        Factory.CodeTransition(),
                        Factory.MetaCode("(")
                               .Accepts(AcceptedCharacters.None),
                        Factory.Code("foo")
                               .AsExpression(),
                        Factory.MetaCode(")")
                               .Accepts(AcceptedCharacters.None)),
                    Factory.Code(" }")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }

        [Fact]
        public void NestedMarkupBlock()
        {
            ParseBlockTest("@while(true) { <p>Hello</p> }",
                new StatementBlock(
                    Factory.CodeTransition(),
                    Factory.Code("while(true) {")
                           .AsStatement(),
                    new MarkupBlock(
                        Factory.Markup(" <p>Hello</p> ")
                               .With(new MarkupCodeGenerator())
                               .Accepts(AcceptedCharacters.None)),
                    Factory.Code("}")
                           .AsStatement()
                           .Accepts(AcceptedCharacters.None)));
        }
    }
}
