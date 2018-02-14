﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;

namespace Microsoft.VisualStudio.Editor.Razor
{
    [ExportLanguageServiceFactory(typeof(RazorProjectEngineFactoryService), RazorLanguage.Name, ServiceLayer.Default)]
    internal class DefaultProjectEngineFactoryServiceFactory : ILanguageServiceFactory
    {
        public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        {
            return new DefaultProjectEngineFactoryService(languageServices.GetRequiredService<ProjectSnapshotManager>());
        }
    }
}