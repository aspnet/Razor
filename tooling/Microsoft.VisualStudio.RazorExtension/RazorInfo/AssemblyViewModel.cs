﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#if RAZOR_EXTENSION_DEVELOPER_MODE
using Microsoft.CodeAnalysis.Razor.ProjectSystem;

namespace Microsoft.VisualStudio.RazorExtension.RazorInfo
{
    public class AssemblyViewModel : NotifyPropertyChanged
    {
        private readonly ProjectExtensibilityAssembly _assembly;

        internal AssemblyViewModel(ProjectExtensibilityAssembly assembly)
        {
            _assembly = assembly;

            Name = _assembly.Identity.GetDisplayName();
        }

        public string Name { get; }
    }
}
#endif