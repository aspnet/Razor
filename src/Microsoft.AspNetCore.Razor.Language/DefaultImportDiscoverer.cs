// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultImportDiscoverer : RazorProjectEngineFeatureBase, IRazorImportDiscoverer
    {
        private IRazorImportItemFeature _importItemFeature;

        public int Order { get; }

        public void Execute(ImportDiscoveryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var importProjectItems = _importItemFeature.GetAssociatedImportItems(context.SourceDocument.FilePath);

            // We want items in descending order. GetAssociatedImportItems returns items in ascending order.
            for (var i = importProjectItems.Count - 1; i >= 0; i--)
            {
                var importProjectItem = importProjectItems[i];
                if (importProjectItem.Exists)
                {
                    var importSourceDocument = RazorSourceDocument.ReadFrom(importProjectItem);

                    context.Results.Add(importSourceDocument);
                }
            }
        }

        protected override void OnInitialized()
        {
            _importItemFeature = GetRequiredFeature<IRazorImportItemFeature>();
        }
    }
}
