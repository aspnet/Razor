// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language
{
    internal static class SyntaxFactory
    {
        internal static HtmlTextSyntax.Green HtmlText(SyntaxToken.Green textToken)
        {
            return new HtmlTextSyntax.Green(textToken);
        }

        internal static HtmlTextTokenSyntax.Green HtmlTextToken(string text)
        {
            return new HtmlTextTokenSyntax.Green(text, null, null);
        }
    }
}
