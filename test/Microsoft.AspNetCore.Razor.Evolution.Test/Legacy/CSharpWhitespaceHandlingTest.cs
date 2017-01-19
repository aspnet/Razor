// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    public class CSharpWhitespaceHandlingTest : CsHtmlMarkupParserTestBase
    {
        private CSharpLanguageCharacteristics _language = new CSharpLanguageCharacteristics(new DefaultCSharpSymbolFactory());

        [Fact]
        public void StatementBlockDoesNotAcceptTrailingNewlineIfNewlinesAreSignificantToAncestor()
        {
            ParseBlockTest("@: @if (true) { }" + Environment.NewLine
                         + "}",
                           new MarkupBlock(
                               Factory.MarkupTransition()
                                   .Accepts(AcceptedCharacters.None),
                               Factory.MetaMarkup(":", HtmlSymbolType.Colon),
                               Factory.Markup(" ")
                                   .With(new SpanEditHandler(
                                       _language.TokenizeString,
                                       AcceptedCharacters.Any)),
                               new StatementBlock(
                                   Factory.CodeTransition()
                                       .Accepts(AcceptedCharacters.None),
                                   Factory.Code("if (true) { }")
                                       .AsStatement()
                                   ),
                               Factory.Markup(Environment.NewLine)
                                   .Accepts(AcceptedCharacters.None)));
        }
    }
}
