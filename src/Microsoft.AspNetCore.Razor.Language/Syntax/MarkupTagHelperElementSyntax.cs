// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language.Syntax
{
    internal sealed partial class MarkupTagHelperElementSyntax
    {
        public TagHelperInfo TagHelperInfo
        {
            get
            {
                return ((InternalSyntax.MarkupTagHelperElementSyntax)Green).TagHelperInfo;
            }
        }

        public MarkupTagHelperElementSyntax WithTagHelperInfo(TagHelperInfo info)
        {
            var green = (InternalSyntax.MarkupTagHelperElementSyntax)Green;
            var newGreen = new InternalSyntax.MarkupTagHelperElementSyntax(
                green.Kind,
                green.StartTag,
                green.Body,
                green.EndTag,
                GetDiagnostics(),
                GetAnnotations(),
                info);

            return (MarkupTagHelperElementSyntax)newGreen.CreateRed(Parent, Position);
        }
    }
}
