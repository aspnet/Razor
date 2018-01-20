// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Language
{
    public sealed class ImportExpanderContext
    {
        public ImportExpanderContext(string sourceFilePath)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(sourceFilePath));
            }

            SourceFilePath = sourceFilePath;
            Results = new List<RazorSourceDocument>();
        }

        public string SourceFilePath { get; }

        public IList<RazorSourceDocument> Results { get; }
    }
}
