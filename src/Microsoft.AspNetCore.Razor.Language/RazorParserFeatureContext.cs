// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal abstract class RazorParserFeatureContext
    {
        internal static readonly Version LatestRazorVersion = new Version(2, 1, 0);

        public static RazorParserFeatureContext Create(Version version)
        {
            if (version == null)
            {
                throw new ArgumentNullException(nameof(version));
            }

            if (version == LatestRazorVersion)
            {
                return new DefaultRazorParserFeatureContext(allowMinimizedBooleanTagHelperAttributes: true);
            }

            return new DefaultRazorParserFeatureContext(allowMinimizedBooleanTagHelperAttributes: false);
        }

        public abstract bool AllowMinimizedBooleanTagHelperAttributes { get; }

        private class DefaultRazorParserFeatureContext : RazorParserFeatureContext
        {
            public DefaultRazorParserFeatureContext(bool allowMinimizedBooleanTagHelperAttributes)
            {
                AllowMinimizedBooleanTagHelperAttributes = allowMinimizedBooleanTagHelperAttributes;
            }

            public override bool AllowMinimizedBooleanTagHelperAttributes { get; }
        }
    }
}
