// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.VisualStudio.LiveShare.Razor
{
    public sealed class ProjectProxyChangeEventArgs
    {
        public ProjectProxyChangeEventArgs(
            Uri filePath, 
            ProjectProxyChangeKind kind)
        {
            FilePath = filePath;
            Kind = kind;
        }

        public Uri FilePath { get; }

        public ProjectProxyChangeKind Kind { get; }
    }
}
