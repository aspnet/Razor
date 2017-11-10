// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.using System;

using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis.Razor;

namespace Microsoft.VisualStudio.Editor.Razor
{
    [System.Composition.Shared]
    [Export(typeof(TagHelperCompletionService))]
    internal class ExportedTagHelperCompletionService : DefaultTagHelperCompletionService
    {
        [ImportingConstructor]
        public ExportedTagHelperCompletionService(TagHelperFactsService tagHelperFactsService) : base(tagHelperFactsService)
        {
        }
    }
}
