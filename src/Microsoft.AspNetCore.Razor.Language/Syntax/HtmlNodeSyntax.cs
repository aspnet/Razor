// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language
{
    internal abstract class HtmlNodeSyntax : SyntaxNode
    {
        internal new Green GreenNode => (Green)base.GreenNode;

        internal HtmlNodeSyntax(Green green, SyntaxNode parent, int position) : base(green, parent, position)
        {
        }

        internal override SyntaxNode Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitHtmlNode(this);
        }

        internal abstract class Green : GreenNode
        {
            protected Green(SyntaxKind kind)
                : base(kind)
            {
            }

            protected Green(SyntaxKind kind, int fullWidth)
                : base(kind, fullWidth)
            {
            }

            protected Green(SyntaxKind kind, RazorDiagnostic[] diagnostics, SyntaxAnnotation[] annotations)
                : base(kind, diagnostics, annotations)
            {
            }

            internal override GreenNode Accept(InternalSyntaxVisitor visitor)
            {
                return visitor.VisitHtmlNode(this);
            }
        }
    }
}
