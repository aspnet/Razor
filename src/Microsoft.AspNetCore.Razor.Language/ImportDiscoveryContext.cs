// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Language
{
    public sealed class ImportDiscoveryContext
    {
        public ImportDiscoveryContext(RazorSourceDocument sourceDocument)
        {
            if (sourceDocument == null)
            {
                throw new ArgumentNullException(nameof(sourceDocument));
            }

            SourceDocument = sourceDocument;
            Results = new List<RazorSourceDocument>();
        }

        public RazorSourceDocument SourceDocument { get; }

        public List<RazorSourceDocument> Results { get; }
    }
}
