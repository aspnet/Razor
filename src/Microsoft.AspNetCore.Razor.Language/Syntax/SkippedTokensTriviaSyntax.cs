// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class SkippedTokensTriviaSyntax : SyntaxNode
    {
        internal class Green : GreenNode
        {
            readonly GreenNode tokens;

            internal Green(GreenNode tokens)
                : base(SyntaxKind.SkippedTokensTrivia)
            {
                this.SlotCount = 1;
                this.tokens = tokens;
                AdjustWidth(tokens);
            }

            internal Green(GreenNode tokens, RazorDiagnostic[] diagnostics, SyntaxAnnotation[] annotations)
                : base(SyntaxKind.SkippedTokensTrivia, diagnostics, annotations)
            {
                this.SlotCount = 1;
                this.tokens = tokens;
                AdjustWidth(tokens);
            }

            internal override SyntaxNode CreateRed(SyntaxNode parent, int position) => new SkippedTokensTriviaSyntax(this, parent, position);

            internal override GreenNode GetSlot(int index)
            {
                switch (index)
                {
                    case 0: return tokens;
                }
                throw new InvalidOperationException();
            }

            internal override GreenNode Accept(InternalSyntaxVisitor visitor)
            {
                return visitor.VisitSkippedTokensTrivia(this);
            }

            internal override GreenNode SetDiagnostics(RazorDiagnostic[] diagnostics)
            {
                return new Green(tokens, diagnostics, GetAnnotations());
            }

            internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
            {
                return new Green(tokens, GetDiagnostics(), annotations);
            }
        }

        internal new Green GreenNode => (Green)base.GreenNode;

        SyntaxNode textTokens;

        internal SyntaxList<SyntaxToken> Tokens => new SyntaxList<SyntaxToken>(GetRed(ref textTokens, 0));

        internal SkippedTokensTriviaSyntax(Green green, SyntaxNode parent, int position)
            : base(green, parent, position)
        {

        }

        public string Value => Tokens.Node?.ToFullString() ?? string.Empty;

        internal override SyntaxNode Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitSkippedTokensTrivia(this);
        }

        internal override SyntaxNode GetCachedSlot(int index)
        {
            switch (index)
            {
                case 0: return textTokens;
                default: return null;
            }
        }

        internal override SyntaxNode GetNodeSlot(int slot)
        {
            switch (slot)
            {
                case 0: return GetRed(ref textTokens, 0);
                default: return null;
            }
        }
    }
}
