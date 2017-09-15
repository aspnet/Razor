// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal abstract class RazorParserFeatureFlags
    {
        internal static readonly RazorParserVersion LatestRazorParserVersion = RazorParserVersion.Version2_1;

        public static RazorParserFeatureFlags Create(RazorParserVersion version)
        {
            if (version == LatestRazorParserVersion)
            {
                return new DefaultRazorParserFeatureFlags(allowMinimizedBooleanTagHelperAttributes: true);
            }

            return new DefaultRazorParserFeatureFlags(allowMinimizedBooleanTagHelperAttributes: false);
        }

        public abstract bool AllowMinimizedBooleanTagHelperAttributes { get; }

        private class DefaultRazorParserFeatureFlags : RazorParserFeatureFlags
        {
            public DefaultRazorParserFeatureFlags(bool allowMinimizedBooleanTagHelperAttributes)
            {
                AllowMinimizedBooleanTagHelperAttributes = allowMinimizedBooleanTagHelperAttributes;
            }

            public override bool AllowMinimizedBooleanTagHelperAttributes { get; }
        }
    }
}
