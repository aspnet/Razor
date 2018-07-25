// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal class HtmlToken : TokenBase<HtmlTokenType>
    {
        internal static readonly HtmlToken Hyphen = new HtmlToken("-", HtmlTokenType.Text);

        public HtmlToken(string content, HtmlTokenType type)
            : base(content, type, RazorDiagnostic.EmptyArray)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
        }

        public HtmlToken(
            string content,
            HtmlTokenType type,
            IReadOnlyList<RazorDiagnostic> errors)
            : base(content, type, errors)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }
        }

        protected override SyntaxToken.Green GetSyntaxToken()
        {
            switch (Type)
            {
                case HtmlTokenType.Text:
                    return SyntaxFactory.HtmlTextToken(Content, Errors.ToArray());
                default:
                    return SyntaxFactory.UnknownToken(Content, Errors.ToArray());
            }
        }
    }
}
