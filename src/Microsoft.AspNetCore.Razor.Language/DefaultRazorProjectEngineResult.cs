// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultRazorProjectEngineResult : RazorProjectEngineResult
    {
        public DefaultRazorProjectEngineResult(RazorCodeDocument codeDocument)
        {
            if (codeDocument == null)
            {
                throw new ArgumentNullException(nameof(codeDocument));
            }

            CodeDocument = codeDocument;
        }

        public override RazorCodeDocument CodeDocument { get; }
    }
}
