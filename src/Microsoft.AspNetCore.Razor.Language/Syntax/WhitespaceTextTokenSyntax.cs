//// Copyright (c) .NET Foundation. All rights reserved.
//// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//namespace Microsoft.AspNetCore.Razor.Language
//{
//    internal class WhitespaceTextTokenSyntax : HtmlTextTokenSyntax
//    {
//        internal WhitespaceTextTokenSyntax(Green green, SyntaxNode parent, int position)
//            : base(green, parent, position)
//        {
//        }

//        internal new Green GreenNode => (Green)base.GreenNode;

//        internal new class Green : HtmlTextTokenSyntax.Green
//        {
//            internal Green(string text, params RazorDiagnostic[] diagnostics)
//                : base(SyntaxKind.Whitespace, text, null, null, diagnostics, null)
//            {
//            }

//            internal Green(string text, GreenNode leadingTrivia, GreenNode trailingTrivia)
//                : base(SyntaxKind.Whitespace, text, leadingTrivia, trailingTrivia)
//            {
//            }

//            internal override SyntaxNode CreateRed(SyntaxNode parent, int position) => new WhitespaceTextTokenSyntax(this, parent, position);
//        }
//    }
//}
