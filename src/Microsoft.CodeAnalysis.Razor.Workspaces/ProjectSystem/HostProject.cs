// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal class HostProject
    {
        public HostProject(string projectFilePath, string targetFrameworkMoniker, string languageVersion, List<HostDocument> documents)
        {
            if (projectFilePath == null)
            {
                throw new ArgumentNullException(nameof(projectFilePath));
            }

            if (targetFrameworkMoniker == null)
            {
                throw new ArgumentNullException(nameof(targetFrameworkMoniker));
            }

            if (languageVersion == null)
            {
                throw new ArgumentNullException(nameof(languageVersion));
            }

            if (documents == null)
            {
                throw new ArgumentNullException(nameof(documents));
            }

            FilePath = projectFilePath;
            TargetFrameworkMoniker = targetFrameworkMoniker;
            LanguageVersion = languageVersion;
            Documents = documents;
        }

        public string FilePath { get; set; }

        public string TargetFrameworkMoniker { get; set; }

        public string LanguageVersion { get; set; }

        public List<HostDocument> Documents { get; }
    }
}
