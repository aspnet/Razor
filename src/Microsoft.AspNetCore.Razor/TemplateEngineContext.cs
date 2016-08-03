// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.AspNetCore.Razor
{
    public class TemplateEngineContext
    {
        public TemplateEngineContext(Stream inputStream, string filePath)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(CommonResources.Argument_Cannot_Be_Null_Or_Empty, nameof(filePath));
            }

            InputStream = inputStream;
            FilePath = filePath;
            FileName = Path.GetFileName(filePath);
        }

        public Stream InputStream { get; }

        public string ClassName { get; set; }

        public string FilePath { get; }

        public string FileName { get; set; }

        public string RelativePath { get; set; }

        public string RootNamespace { get; set; }
    }
}