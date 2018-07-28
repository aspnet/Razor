// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class HtmlDocumentSyntax : HtmlNodeSyntax
    {
        HtmlDeclarationSyntax doctype;
        SyntaxNode precedingMisc;
        HtmlNodeSyntax body;
        SyntaxNode followingMisc;
        SkippedTokensTriviaSyntax skippedTokens;
        SyntaxToken eof;

        internal HtmlDocumentSyntax(Green green, SyntaxNode parent, int position)
            : base(green, parent, position)
        {
        }

        internal new Green GreenNode => (Green)base.GreenNode;

        internal HtmlDeclarationSyntax Doctype => GetRed(ref doctype, 0);
        internal SyntaxList<SyntaxNode> PrecedingMisc => new SyntaxList<SyntaxNode>(GetRed(ref precedingMisc, 1));
        internal HtmlNodeSyntax Body => GetRed(ref body, 2);
        internal SyntaxList<SyntaxNode> FollowingMisc => new SyntaxList<SyntaxNode>(GetRed(ref followingMisc, 3));
        internal SkippedTokensTriviaSyntax SkippedTokens => GetRed(ref skippedTokens, 4);
        internal SyntaxToken Eof => GetRed(ref eof, 5);

        internal override SyntaxNode Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitHtmlDocument(this);
        }

        internal override SyntaxNode GetCachedSlot(int index)
        {
            switch (index)
            {
                case 0: return doctype;
                case 1: return precedingMisc;
                case 2: return body;
                case 3: return followingMisc;
                case 4: return skippedTokens;
                case 5: return eof;
                default: return null;
            }
        }

        internal override SyntaxNode GetNodeSlot(int slot)
        {
            switch (slot)
            {
                case 0: return Doctype;
                case 1: return GetRed(ref precedingMisc, 1);
                case 2: return Body;
                case 3: return GetRed(ref followingMisc, 3);
                case 4: return SkippedTokens;
                case 5: return Eof;
                default: return null;
            }
        }

        //public IHtmlElementSyntax RootSyntax => Body as IHtmlElementSyntax;
        //public IHtmlElement Root => RootSyntax.AsElement;

        internal new class Green : HtmlNodeSyntax.Green
        {
            internal HtmlDeclarationSyntax.Green Doctype { get; private set; }
            internal GreenNode PrecedingMisc { get; private set; }
            internal HtmlNodeSyntax.Green Body { get; private set; }
            internal GreenNode FollowingMisc { get; private set; }
            internal SkippedTokensTriviaSyntax.Green SkippedTokens { get; private set; }
            internal SyntaxToken.Green Eof { get; private set; }

            internal Green(HtmlDeclarationSyntax.Green doctype, GreenNode precedingMisc, HtmlNodeSyntax.Green body, GreenNode followingMisc, SkippedTokensTriviaSyntax.Green skippedTokens, SyntaxToken.Green eof)
                : base(SyntaxKind.HtmlDocument)
            {
                SlotCount = 6;
                Doctype = doctype;
                AdjustWidth(doctype);
                PrecedingMisc = precedingMisc;
                AdjustWidth(precedingMisc);
                Body = body;
                AdjustWidth(body);
                FollowingMisc = followingMisc;
                AdjustWidth(followingMisc);
                SkippedTokens = skippedTokens;
                AdjustWidth(skippedTokens);
                Eof = eof;
                AdjustWidth(eof);
            }

            internal Green(HtmlDeclarationSyntax.Green doctype, GreenNode precedingMisc, HtmlNodeSyntax.Green body, GreenNode followingMisc, SkippedTokensTriviaSyntax.Green skippedTokens, SyntaxToken.Green eof, RazorDiagnostic[] diagnostics, SyntaxAnnotation[] annotations)
                : base(SyntaxKind.HtmlDocument, diagnostics, annotations)
            {
                SlotCount = 6;
                Doctype = doctype;
                AdjustWidth(doctype);
                PrecedingMisc = precedingMisc;
                AdjustWidth(precedingMisc);
                Body = body;
                AdjustWidth(body);
                FollowingMisc = followingMisc;
                AdjustWidth(followingMisc);
                SkippedTokens = skippedTokens;
                AdjustWidth(skippedTokens);
                Eof = eof;
                AdjustWidth(eof);
            }

            internal override SyntaxNode CreateRed(SyntaxNode parent, int position) => new HtmlDocumentSyntax(this, parent, position);

            internal override GreenNode GetSlot(int index)
            {
                switch (index)
                {
                    case 0: return Doctype;
                    case 1: return PrecedingMisc;
                    case 2: return Body;
                    case 3: return FollowingMisc;
                    case 4: return SkippedTokens;
                    case 5: return Eof;
                }
                throw new InvalidOperationException();
            }

            internal override GreenNode Accept(InternalSyntaxVisitor visitor)
            {
                return visitor.VisitHtmlDocument(this);
            }

            internal override GreenNode SetDiagnostics(RazorDiagnostic[] diagnostics)
            {
                return new Green(Doctype, PrecedingMisc, Body, FollowingMisc, SkippedTokens, Eof, diagnostics, GetAnnotations());
            }

            internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
            {
                return new Green(Doctype, PrecedingMisc, Body, FollowingMisc, SkippedTokens, Eof, GetDiagnostics(), annotations);
            }
        }
    }
}
