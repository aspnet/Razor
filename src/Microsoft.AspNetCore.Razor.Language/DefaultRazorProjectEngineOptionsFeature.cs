// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultRazorProjectEngineOptionsFeature : RazorProjectEngineFeatureBase, IRazorProjectEngineOptionsFeature
    {
        // REVIEWERS: This is least-designed as possible. Could take a similar approach to our syntax tree options but it felt like a heavy hammer
        // to add all of that code for a single options property.
        public RazorProjectEngineOptions Options { get; } = new RazorProjectEngineOptions();
    }
}
