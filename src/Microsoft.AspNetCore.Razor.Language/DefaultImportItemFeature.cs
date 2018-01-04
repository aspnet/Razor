// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultImportItemFeature : RazorProjectEngineFeatureBase, IRazorImportItemFeature
    {
        private string _importsFileName;

        public IReadOnlyList<RazorProjectItem> GetAssociatedImportItems(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(filePath));
            }

            var importProjectItems = Engine.Project.FindHierarchicalItems(filePath, _importsFileName).ToArray();
            return importProjectItems;
        }

        protected override void OnInitialized()
        {
            var optionsFeature = GetRequiredFeature<IRazorProjectEngineOptionsFeature>();
            _importsFileName = optionsFeature.Options.ImportsFileName;
        }
    }
}
