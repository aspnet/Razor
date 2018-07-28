// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal partial class MyHtmlMarkupParser : TokenizerBackedParser<HtmlTokenizer, HtmlToken, HtmlTokenType>
    {
        public void ParseDocumentNew()
        {
            if (Context == null)
            {
                throw new InvalidOperationException(Resources.Parser_Context_Not_Set);
            }

            var doctype = ParseDoctype();

            GreenNode node = doctype;
            var precedingMisc = ParseHtmlMisc(ref node);

            var body = ParseHtmlElement();

            node = body;
            var followingMisc = ParseHtmlMisc(ref node);

            var skippedTokens = ParseSkippedTokens();

            Debug.Assert(EndOfFile);
            var document = SyntaxFactory.HtmlDocument(doctype, precedingMisc, body, followingMisc, skippedTokens, CurrentSyntaxToken);
        }

        private HtmlNodeSyntax.Green ParseHtmlElement()
        {
            return null;
        }

        private HtmlDeclarationSyntax.Green ParseDoctype()
        {
            return null;
        }

        private SkippedTokensTriviaSyntax.Green ParseSkippedTokens()
        {
            return null;
        }

        private InternalSyntaxList<HtmlNodeSyntax.Green> ParseHtmlMisc(ref GreenNode outerNode)
        {
            return null;
        }
    }
}
