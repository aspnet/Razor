// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language
{
    internal static class SyntaxFactory
    {
        internal static HtmlTextSyntax.Green HtmlText(InternalSyntaxList<SyntaxToken.Green> textTokens)
        {
            return new HtmlTextSyntax.Green(textTokens.Node);
        }

        internal static HtmlTextTokenSyntax.Green HtmlTextToken(string text, params RazorDiagnostic[] diagnostics)
        {
            return new HtmlTextTokenSyntax.Green(text, diagnostics);
        }

        internal static WhitespaceTokenSyntax.Green WhitespaceToken(string text, params RazorDiagnostic[] diagnostics)
        {
            return new WhitespaceTokenSyntax.Green(text, diagnostics);
        }

        internal static NewLineTokenSyntax.Green NewLineToken(string text, params RazorDiagnostic[] diagnostics)
        {
            return new NewLineTokenSyntax.Green(text, diagnostics);
        }

        internal static PunctuationSyntax.Green Punctuation(SyntaxKind syntaxKind, string text, params RazorDiagnostic[] diagnostics)
        {
            return new PunctuationSyntax.Green(syntaxKind, text, diagnostics);
        }

        internal static UnknownTokenSyntax.Green UnknownToken(string text, params RazorDiagnostic[] diagnostics)
        {
            return new UnknownTokenSyntax.Green(text, diagnostics);
        }
    }
}
