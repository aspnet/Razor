// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language.Syntax.InternalSyntax
{
    internal sealed partial class MarkupTagHelperElementSyntax
    {
        public MarkupTagHelperElementSyntax(
            SyntaxKind kind,
            MarkupTagHelperStartTagSyntax startTag,
            GenericBlockSyntax body,
            MarkupTagHelperEndTagSyntax endTag,
            RazorDiagnostic[] diagnostics,
            SyntaxAnnotation[] annotations,
            TagHelperInfo tagHelperInfo)
            : this(kind, startTag, body, endTag, diagnostics, annotations)
        {
            TagHelperInfo = tagHelperInfo;
        }

        public TagHelperInfo TagHelperInfo { get; }
    }
}
