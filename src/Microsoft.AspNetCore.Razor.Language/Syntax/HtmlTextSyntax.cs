// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class HtmlTextSyntax : HtmlNodeSyntax
    {
        private SyntaxNode _textTokens;

        internal HtmlTextSyntax(Green green, SyntaxNode parent, int position) : base(green, parent, position)
        {
        }

        public SyntaxList<SyntaxNode> TextTokens => new SyntaxList<SyntaxNode>(GetRed(ref _textTokens, 0));

        public string Value => TextTokens[0]?.ToFullString() ?? string.Empty;

        internal override SyntaxNode Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitHtmlText(this);
        }

        internal override SyntaxNode GetCachedSlot(int index)
        {
            switch (index)
            {
                case 0: return _textTokens;
                default: return null;
            }
        }

        internal override SyntaxNode GetNodeSlot(int slot)
        {
            switch (slot)
            {
                case 0: return GetRed(ref _textTokens, 0);
                default: return null;
            }
        }

        internal new class Green : HtmlNodeSyntax.Green
        {
            private readonly GreenNode _value;

            internal Green(GreenNode value) : base(SyntaxKind.HtmlText)
            {
                SlotCount = 1;
                _value = value;
                AdjustWidth(value);
            }

            internal Green(GreenNode value, RazorDiagnostic[] diagnostics, SyntaxAnnotation[] annotations)
                : base(SyntaxKind.HtmlText, diagnostics, annotations)
            {
                SlotCount = 1;
                _value = value;
                AdjustWidth(value);
            }

            internal InternalSyntaxList<GreenNode> TextTokens => new InternalSyntaxList<GreenNode>(_value);

            internal override GreenNode GetSlot(int index)
            {
                switch (index)
                {
                    case 0: return _value;
                }

                throw new InvalidOperationException();
            }

            internal override GreenNode Accept(InternalSyntaxVisitor visitor)
            {
                return visitor.VisitHtmlText(this);
            }

            internal override SyntaxNode CreateRed(SyntaxNode parent, int position) => new HtmlTextSyntax(this, parent, position);

            internal override GreenNode SetDiagnostics(RazorDiagnostic[] diagnostics)
            {
                return new Green(_value, diagnostics, GetAnnotations());
            }

            internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
            {
                return new Green(_value, GetDiagnostics(), annotations);
            }
        }
    }
}
