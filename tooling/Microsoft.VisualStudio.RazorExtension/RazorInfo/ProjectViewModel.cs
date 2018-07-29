// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if RAZOR_EXTENSION_DEVELOPER_MODE
using System.IO;
using Microsoft.CodeAnalysis;

namespace Microsoft.VisualStudio.RazorExtension.RazorInfo
{
    public class ProjectViewModel : NotifyPropertyChanged
    {
        internal ProjectViewModel(string filePath, ProjectId projectId)
        {
            FilePath = filePath;
            ProjectId = projectId;
        }

        public ProjectId ProjectId { get; }

        public string FilePath { get; }

        public string Name => Path.GetFileNameWithoutExtension(FilePath);
    }
}
#endif