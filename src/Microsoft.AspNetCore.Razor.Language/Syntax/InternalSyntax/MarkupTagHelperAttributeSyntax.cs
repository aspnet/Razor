// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language.Syntax.InternalSyntax
{
    internal sealed partial class MarkupTagHelperAttributeSyntax
    {
        public MarkupTagHelperAttributeSyntax(
            SyntaxKind kind,
            MarkupTextLiteralSyntax namePrefix,
            MarkupTextLiteralSyntax name,
            MarkupTextLiteralSyntax nameSuffix,
            SyntaxToken equalsToken,
            MarkupTextLiteralSyntax valuePrefix,
            RazorBlockSyntax value,
            MarkupTextLiteralSyntax valueSuffix,
            RazorDiagnostic[] diagnostics,
            SyntaxAnnotation[] annotations,
            TagHelperAttributeInfo tagHelperAttributeInfo)
            : this(kind, namePrefix, name, nameSuffix, equalsToken, valuePrefix, value, valueSuffix, diagnostics, annotations)
        {
            TagHelperAttributeInfo = tagHelperAttributeInfo;
        }

        public TagHelperAttributeInfo TagHelperAttributeInfo { get; }
    }
}
