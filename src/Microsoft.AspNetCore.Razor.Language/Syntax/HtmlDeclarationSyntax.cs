// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class HtmlDeclarationSyntax : HtmlNodeSyntax
    {
        internal new Green GreenNode => (Green)base.GreenNode;

        PunctuationSyntax lessThanExclamationToken;
        SyntaxToken doctypeKeyword;
        SyntaxToken htmlKeyword;
        PunctuationSyntax greaterThanToken;

        internal HtmlDeclarationSyntax(Green green, SyntaxNode parent, int position)
            : base(green, parent, position)
        {
        }

        public PunctuationSyntax LessThanExclamationToken => GetRed(ref lessThanExclamationToken, 0);
        public SyntaxToken DoctypeKeyword => GetRed(ref doctypeKeyword, 1);
        public SyntaxToken HtmlKeyword => GetRed(ref htmlKeyword, 2);
        public PunctuationSyntax GreaterThanToken => GetRed(ref greaterThanToken, 3);

        internal override SyntaxNode Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitHtmlDeclaration(this);
        }

        internal override SyntaxNode GetCachedSlot(int index)
        {
            switch (index)
            {
                case 0: return lessThanExclamationToken;
                case 1: return doctypeKeyword;
                case 2: return htmlKeyword;
                case 3: return greaterThanToken;
                default: return null;
            }
        }

        internal override SyntaxNode GetNodeSlot(int slot)
        {
            switch (slot)
            {
                case 0: return LessThanExclamationToken;
                case 1: return DoctypeKeyword;
                case 2: return HtmlKeyword;
                case 3: return GreaterThanToken;
                default: return null;
            }
        }

        internal new class Green : HtmlNodeSyntax.Green
        {
            internal Green(
                PunctuationSyntax.Green lessThanExclamationToken,
                GreenNode doctypeKeyword,
                GreenNode htmlKeyword,
                PunctuationSyntax.Green greaterThanToken)
                : base(SyntaxKind.HtmlDeclaration)
            {
                SlotCount = 4;
                LessThanExclamationToken = lessThanExclamationToken;
                AdjustWidth(lessThanExclamationToken);
                DoctypeKeyword = doctypeKeyword;
                AdjustWidth(doctypeKeyword);
                HtmlKeyword = htmlKeyword;
                AdjustWidth(htmlKeyword);
                GreaterThanToken = greaterThanToken;
                AdjustWidth(greaterThanToken);
            }

            internal Green(
                PunctuationSyntax.Green lessThanExclamationToken,
                GreenNode doctypeKeyword,
                GreenNode htmlKeyword,
                PunctuationSyntax.Green greaterThanToken,
                RazorDiagnostic[] diagnostics,
                SyntaxAnnotation[] annotations)
                : base(SyntaxKind.HtmlDeclaration, diagnostics, annotations)
            {
                SlotCount = 4;
                LessThanExclamationToken = lessThanExclamationToken;
                AdjustWidth(lessThanExclamationToken);
                DoctypeKeyword = doctypeKeyword;
                AdjustWidth(doctypeKeyword);
                HtmlKeyword = htmlKeyword;
                AdjustWidth(htmlKeyword);
                GreaterThanToken = greaterThanToken;
                AdjustWidth(greaterThanToken);
            }

            internal PunctuationSyntax.Green LessThanExclamationToken { get; private set; }
            internal GreenNode DoctypeKeyword { get; private set; }
            internal GreenNode HtmlKeyword { get; private set; }
            internal PunctuationSyntax.Green GreaterThanToken { get; private set; }

            internal override SyntaxNode CreateRed(SyntaxNode parent, int position) => new HtmlDeclarationSyntax(this, parent, position);

            internal override GreenNode GetSlot(int index)
            {
                switch (index)
                {
                    case 0: return LessThanExclamationToken;
                    case 1: return DoctypeKeyword;
                    case 2: return HtmlKeyword;
                    case 3: return GreaterThanToken;
                }
                throw new InvalidOperationException();
            }

            internal override GreenNode Accept(InternalSyntaxVisitor visitor)
            {
                return visitor.VisitHtmlDeclaration(this);
            }

            internal override GreenNode SetDiagnostics(RazorDiagnostic[] diagnostics)
            {
                return new Green(LessThanExclamationToken, DoctypeKeyword, HtmlKeyword, GreaterThanToken, diagnostics, GetAnnotations());
            }

            internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
            {
                return new Green(LessThanExclamationToken, DoctypeKeyword, HtmlKeyword, GreaterThanToken, GetDiagnostics(), annotations);
            }
        }
    }
}
