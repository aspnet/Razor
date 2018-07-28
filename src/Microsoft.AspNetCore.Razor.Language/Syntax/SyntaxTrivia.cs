// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class SyntaxTrivia : SyntaxNode
    {
        internal SyntaxTrivia(Green green, SyntaxNode parent, int position)
            : base(green, parent, position)
        {
        }

        internal new Green GreenNode => (Green)base.GreenNode;

        public string Text => GreenNode.Text;

        internal override sealed SyntaxNode GetCachedSlot(int index)
        {
            throw new InvalidOperationException();
        }

        internal override sealed SyntaxNode GetNodeSlot(int slot)
        {
            throw new InvalidOperationException();
        }

        internal override SyntaxNode Accept(SyntaxVisitor visitor)
        {
            return visitor.VisitSyntaxTrivia(this);
        }

        protected override int GetTextWidth()
        {
            return Text.Length;
        }

        public sealed override SyntaxTriviaList GetTrailingTrivia()
        {
            return default(SyntaxTriviaList);
        }

        public sealed override SyntaxTriviaList GetLeadingTrivia()
        {
            return default(SyntaxTriviaList);
        }

        public override string ToString() => Text;

        public sealed override string ToFullString() => Text;

        internal class Green : GreenNode
        {
            internal Green(SyntaxKind kind, string text)
                : base(kind, text.Length)
            {
                Text = text;
            }

            internal Green(SyntaxKind kind, string text, RazorDiagnostic[] diagnostics, SyntaxAnnotation[] annotations)
                : base(kind, text.Length, diagnostics, annotations)
            {
                Text = text;
            }

            public string Text { get; }

            public override int Width => Text.Length;

            internal override void WriteToOrFlatten(TextWriter writer, Stack<GreenNode> stack)
            {
                writer.Write(Text);
            }

            public sealed override string ToFullString() => Text;

            public sealed override int GetLeadingTriviaWidth() => 0;
            public sealed override int GetTrailingTriviaWidth() => 0;

            protected override sealed int GetSlotCount() => 0;

            internal override sealed GreenNode GetSlot(int index)
            {
                throw new InvalidOperationException();
            }

            internal override SyntaxNode CreateRed(SyntaxNode parent, int position) => new SyntaxTrivia(this, parent, position);

            internal override GreenNode Accept(InternalSyntaxVisitor visitor)
            {
                return visitor.VisitSyntaxTrivia(this);
            }

            internal override GreenNode SetDiagnostics(RazorDiagnostic[] diagnostics)
            {
                return new Green(Kind, Text, diagnostics, GetAnnotations());
            }

            internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
            {
                return new Green(Kind, Text, GetDiagnostics(), annotations);
            }
        }
    }
}
