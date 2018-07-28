// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language
{
    internal abstract class InternalSyntaxVisitor
    {
        public virtual GreenNode Visit(GreenNode node)
        {
            if (node != null)
            {
                return node.Accept(this);
            }

            return null;
        }

        public virtual GreenNode VisitSyntaxNode(GreenNode node)
        {
            return node;
        }

        public virtual GreenNode VisitHtmlNode(HtmlNodeSyntax.Green node)
        {
            return VisitSyntaxNode(node);
        }

        public virtual GreenNode VisitHtmlText(HtmlTextSyntax.Green node)
        {
            return VisitHtmlNode(node);
        }

        public virtual GreenNode VisitHtmlDocument(HtmlDocumentSyntax.Green node)
        {
            return VisitHtmlNode(node);
        }

        public virtual GreenNode VisitHtmlDeclaration(HtmlDeclarationSyntax.Green node)
        {
            return VisitHtmlNode(node);
        }

        public virtual SyntaxToken.Green VisitSyntaxToken(SyntaxToken.Green token)
        {
            return token;
        }

        public virtual SyntaxTrivia.Green VisitSyntaxTrivia(SyntaxTrivia.Green trivia)
        {
            return trivia;
        }

        public virtual GreenNode VisitSkippedTokensTrivia(SkippedTokensTriviaSyntax.Green node)
        {
            return VisitSyntaxNode(node);
        }
    }
}
