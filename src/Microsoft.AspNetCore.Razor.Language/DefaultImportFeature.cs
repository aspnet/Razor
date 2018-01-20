// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultImportFeature : RazorProjectEngineFeatureBase, IRazorImportFeature
    {
        private IEnumerable<IRazorImportExpander> _expanders;

        public IReadOnlyList<RazorSourceDocument> GetImports(string sourceFilePath)
        {
            if (string.IsNullOrEmpty(sourceFilePath))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(sourceFilePath));
            }

            if (!_expanders.Any())
            {
                return Array.Empty<RazorSourceDocument>();
            }

            var importExpanderContext = new ImportExpanderContext(sourceFilePath);
            foreach (var expander in _expanders)
            {
                expander.Populate(importExpanderContext);
            }

            var imports = importExpanderContext.Results.ToArray();
            return imports;
        }

        protected override void OnInitialized()
        {
            _expanders = ProjectEngine.Features.OfType<IRazorImportExpander>().OrderBy(expander => expander.Order);
        }
    }
}
