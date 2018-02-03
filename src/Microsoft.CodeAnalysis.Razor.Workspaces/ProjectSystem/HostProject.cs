// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal class HostProject
    {
        public HostProject(string projectFilePath, string languageVersion)
        {
            if (projectFilePath == null)
            {
                throw new ArgumentNullException(nameof(projectFilePath));
            }

            if (languageVersion == null)
            {
                throw new ArgumentNullException(nameof(languageVersion));
            }

            FilePath = projectFilePath;
            LanguageVersion = languageVersion;
        }

        public string FilePath { get; }

        public string LanguageVersion { get; }
    }
}