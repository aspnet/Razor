// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language
{
    public abstract class RazorProjectEngineResult
    {
        public abstract RazorCodeDocument CodeDocument { get; }

        public static RazorProjectEngineResult Create(RazorCodeDocument codeDocument)
        {
            if (codeDocument == null)
            {
                throw new ArgumentNullException(nameof(codeDocument));
            }

            var result = new DefaultRazorProjectEngineResult(codeDocument);
            return result;
        }
    }
}
