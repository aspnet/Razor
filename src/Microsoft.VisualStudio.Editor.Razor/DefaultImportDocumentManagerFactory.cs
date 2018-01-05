﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Composition;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Razor;

namespace Microsoft.VisualStudio.Editor.Razor
{
    [Shared]
    [ExportLanguageServiceFactory(typeof(ImportDocumentManager), RazorLanguage.Name, ServiceLayer.Default)]
    internal class DefaultImportDocumentManagerFactory : ILanguageServiceFactory
    {
        public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        {
            if (languageServices == null)
            {
                throw new ArgumentNullException(nameof(languageServices));
            }

            var dispatcher = languageServices.WorkspaceServices.GetRequiredService<ForegroundDispatcher>();
            var errorReporter = languageServices.WorkspaceServices.GetRequiredService<ErrorReporter>();
            var fileChangeTrackerFactory = languageServices.GetRequiredService<FileChangeTrackerFactory>();
            var projectEngineFactoryService = languageServices.GetRequiredService<RazorProjectEngineFactoryService>();

            return new DefaultImportDocumentManager(
                dispatcher,
                errorReporter,
                fileChangeTrackerFactory,
                projectEngineFactoryService);
        }
    }
}
