// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    public static class RazorProjectEngineBuilderExtensions
    {
        // REVIEWERS: See DefaultRazorProjectEngineOptionsFeature for implications
        public static void SetImportFileName(this RazorProjectEngineBuilder builder, string name)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(name));
            }

            var optionsFeature = builder.Features.OfType<IRazorProjectEngineOptionsFeature>().FirstOrDefault();
            if (optionsFeature == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatMissingFeatureDependency(
                        typeof(RazorProjectEngineBuilder).FullName,
                        typeof(IRazorProjectEngineOptionsFeature).FullName));
            }

            optionsFeature.Options.ImportsFileName = name;
        }
    }
}
