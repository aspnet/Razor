// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal class HostDocument
    {
        public HostDocument(string filePath, string generatedFilePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (generatedFilePath == null)
            {
                throw new ArgumentNullException(nameof(generatedFilePath));
            }

            FilePath = filePath;
            GeneratedFilePath = generatedFilePath;
        }

        public string FilePath { get; }

        public string GeneratedFilePath { get; }
    }
}
