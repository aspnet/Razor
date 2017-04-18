// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Razor.Language.CodeGeneration
{
    internal abstract class HtmlAttributeWriter
    {
        public abstract void WriteHtmlAttributeValue(CSharpRenderingContext context, HtmlAttributeValueIRNode node);

        public abstract void WriteCSharpAttributeValue(CSharpRenderingContext context, CSharpAttributeValueIRNode node);
    }
}
