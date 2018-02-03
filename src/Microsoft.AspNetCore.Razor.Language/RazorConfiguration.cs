// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    public abstract class RazorConfiguration
    {
        public static readonly RazorConfiguration Default = new DefaultRazorConfiguration(
            RazorLanguageVersion.Latest, 
            "unnamed",
            Array.Empty<RazorExtension>(),
            designTime: false);

        // This is used only in some back-compat scenarios. We don't expose it because there's no
        // use case for anyone else to use it.
        internal static readonly RazorConfiguration DefaultDesignTime = new DefaultRazorConfiguration(
            RazorLanguageVersion.Latest,
            "unnamed",
            Array.Empty<RazorExtension>(),
            designTime: true);

        public static RazorConfiguration Create(
            RazorLanguageVersion languageVersion,
            string configurationName,
            IEnumerable<RazorExtension> extensions,
            bool designTime)
        {
            if (languageVersion == null)
            {
                throw new ArgumentNullException(nameof(languageVersion));
            }

            if (configurationName == null)
            {
                throw new ArgumentNullException(nameof(configurationName));
            }

            if (extensions == null)
            {
                throw new ArgumentNullException(nameof(extensions));
            }

            return new DefaultRazorConfiguration(languageVersion, configurationName, extensions.ToArray(), designTime);
        }

        public abstract string ConfigurationName { get; }

        public abstract IReadOnlyList<RazorExtension> Extensions { get; }

        public abstract RazorLanguageVersion LanguageVersion { get; }

        public abstract bool DesignTime { get; }

        private class DefaultRazorConfiguration : RazorConfiguration
        {
            public DefaultRazorConfiguration(
                RazorLanguageVersion languageVersion,
                string configurationName,
                RazorExtension[] extensions,
                bool designTime)
            {
                LanguageVersion = languageVersion;
                ConfigurationName = configurationName;
                Extensions = extensions;
                DesignTime = designTime;
            }

            public override string ConfigurationName { get; }

            public override IReadOnlyList<RazorExtension> Extensions { get; }

            public override RazorLanguageVersion LanguageVersion { get; }

            public override bool DesignTime { get; }
        }
    }
}
