// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language.Syntax
{
    internal sealed partial class MarkupTagHelperAttributeSyntax
    {
        public TagHelperAttributeInfo TagHelperAttributeInfo
        {
            get
            {
                return ((InternalSyntax.MarkupTagHelperAttributeSyntax)Green).TagHelperAttributeInfo;
            }
        }

        public MarkupTagHelperAttributeSyntax WithTagHelperAttributeInfo(TagHelperAttributeInfo info)
        {
            var green = (InternalSyntax.MarkupTagHelperAttributeSyntax)Green;
            var newGreen = new InternalSyntax.MarkupTagHelperAttributeSyntax(
                green.Kind,
                green.NamePrefix,
                green.Name,
                green.NameSuffix,
                green.EqualsToken,
                green.ValuePrefix,
                green.Value,
                green.ValueSuffix,
                GetDiagnostics(),
                GetAnnotations(),
                info);

            return (MarkupTagHelperAttributeSyntax)newGreen.CreateRed(Parent, Position);
        }
    }
}
