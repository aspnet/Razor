// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions
{
    internal class RelativePathCodeDocumentProcessor : RazorProjectEngineFeatureBase, IRazorCodeDocumentProcessor
    {
        public void Process(RazorCodeDocument codeDocument)
        {
            if (codeDocument == null)
            {
                throw new ArgumentNullException(nameof(codeDocument));
            }

            var projectItem = Engine.Project.GetItem(codeDocument.Source.FilePath);
            codeDocument.SetRelativePath(projectItem.FilePath);
        }
    }
}
