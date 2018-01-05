// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultRazorImportFeature : RazorProjectEngineFeatureBase, IRazorImportFeature
    {
        private IEnumerable<IRazorImportDiscoverer> _discoverers;

        public IReadOnlyList<RazorSourceDocument> GetImports(RazorSourceDocument sourceDocument)
        {
            if (sourceDocument == null)
            {
                throw new ArgumentNullException(nameof(sourceDocument));
            }

            if (!_discoverers.Any())
            {
                return Array.Empty<RazorSourceDocument>();
            }

            var importDiscoveryContext = new ImportDiscoveryContext(sourceDocument);
            foreach (var discoverer in _discoverers)
            {
                discoverer.Execute(importDiscoveryContext);
            }

            return importDiscoveryContext.Results;
        }

        protected override void OnInitialized()
        {
            _discoverers = Engine.Features.OfType<IRazorImportDiscoverer>().OrderBy(discoverer => discoverer.Order);
        }
    }
}
