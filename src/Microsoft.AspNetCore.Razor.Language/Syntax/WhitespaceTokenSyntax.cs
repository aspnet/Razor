// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class WhitespaceTokenSyntax : SyntaxToken
    {
        internal WhitespaceTokenSyntax(Green green, SyntaxNode parent, int position)
            : base(green, parent, position)
        {
        }

        public string Value => Text;

        internal override SyntaxToken WithLeadingTriviaCore(SyntaxNode trivia)
        {
            // TODO
            //return (HtmlTextTokenSyntax)new Green(Text, trivia?.GreenNode, GetTrailingTrivia().Node?.GreenNode).CreateRed(Parent, Start);
            throw new NotImplementedException();
        }

        internal override SyntaxToken WithTrailingTriviaCore(SyntaxNode trivia)
        {
            // TODO
            //return (HtmlTextTokenSyntax)new Green(Text, GetLeadingTrivia().Node?.GreenNode, trivia?.GreenNode).CreateRed(Parent, Start);
            throw new NotImplementedException();
        }

        internal new class Green : SyntaxToken.Green
        {
            internal Green(string text, params RazorDiagnostic[] diagnostics)
                : base(SyntaxKind.Whitespace, text, null, null, diagnostics, null)
            {
            }

            internal Green(string text, GreenNode leadingTrivia, GreenNode trailingTrivia)
                : base(SyntaxKind.Whitespace, text, leadingTrivia, trailingTrivia)
            {
            }

            protected Green(SyntaxKind kind, string name, GreenNode leadingTrivia, GreenNode trailingTrivia)
                : base(kind, name, leadingTrivia, trailingTrivia)
            {
            }

            protected Green(SyntaxKind kind, string name, GreenNode leadingTrivia, GreenNode trailingTrivia, RazorDiagnostic[] diagnostics, SyntaxAnnotation[] annotations)
                : base(kind, name, leadingTrivia, trailingTrivia, diagnostics, annotations)
            {
            }

            internal override SyntaxNode CreateRed(SyntaxNode parent, int position) => new WhitespaceTokenSyntax(this, parent, position);

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
