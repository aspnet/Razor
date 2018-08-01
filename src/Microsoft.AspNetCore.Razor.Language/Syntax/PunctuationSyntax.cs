// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class PunctuationSyntax : SyntaxToken
    {
        internal new Green GreenNode => (Green)base.GreenNode;

        public string Punctuation => Text;

        internal PunctuationSyntax(Green green, SyntaxNode parent, int position)
            : base(green, parent, position)
        {
        }

        internal override SyntaxToken WithLeadingTriviaCore(SyntaxNode trivia)
        {
            return (PunctuationSyntax)new Green(Kind, Text, trivia?.GreenNode, GetTrailingTrivia().Node?.GreenNode).CreateRed(Parent, Start);
        }

        internal override SyntaxToken WithTrailingTriviaCore(SyntaxNode trivia)
        {
            return (PunctuationSyntax)new Green(Kind, Text, GetLeadingTrivia().Node?.GreenNode, trivia?.GreenNode).CreateRed(Parent, Start);
        }

        internal new class Green : SyntaxToken.Green
        {
            internal Green(SyntaxKind kind, string name, RazorDiagnostic[] diagnostics)
                : this(kind, name, null, null, diagnostics, null)
            {
            }

            internal Green(SyntaxKind kind, string name, GreenNode leadingTrivia, GreenNode trailingTrivia)
                : this(kind, name, leadingTrivia, trailingTrivia, null, null)
            {
            }

            internal Green(SyntaxKind kind, string name, GreenNode leadingTrivia, GreenNode trailingTrivia, RazorDiagnostic[] diagnostics, SyntaxAnnotation[] annotations)
                : base(kind, name, leadingTrivia, trailingTrivia, diagnostics, annotations)
            {
            }

            internal override SyntaxNode CreateRed(SyntaxNode parent, int position) => new PunctuationSyntax(this, parent, position);

            public override SyntaxToken.Green WithLeadingTrivia(GreenNode trivia)
            {
                return new Green(Kind, Text, trivia, TrailingTrivia);
            }

            public override SyntaxToken.Green WithTrailingTrivia(GreenNode trivia)
            {
                return new Green(Kind, Text, LeadingTrivia, trivia);
            }

            internal override GreenNode SetDiagnostics(RazorDiagnostic[] diagnostics)
            {
                return new Green(Kind, Text, LeadingTrivia, TrailingTrivia, diagnostics, GetAnnotations());
            }

            internal override GreenNode SetAnnotations(SyntaxAnnotation[] annotations)
            {
                return new Green(Kind, Text, LeadingTrivia, TrailingTrivia, GetDiagnostics(), annotations);
            }
        }
    }
}
