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
    [ExportLanguageServiceFactory(typeof(VisualStudioRazorParserFactory), RazorLanguage.Name, ServiceLayer.Default)]
    internal class DefaultVisualStudioRazorParserFactoryFactory : ILanguageServiceFactory
    {
        public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        {
            if (languageServices == null)
            {
                throw new ArgumentNullException(nameof(languageServices));
            }

            var workspaceServices = languageServices.WorkspaceServices;
            var dispatcher = workspaceServices.GetRequiredService<ForegroundDispatcher>();
            var errorReporter = workspaceServices.GetRequiredService<ErrorReporter>();
            var completionBroker = languageServices.GetRequiredService<VisualStudioCompletionBroker>();
            var projectEngineFactoryService = languageServices.GetRequiredService<RazorProjectEngineFactoryService>();

            return new DefaultVisualStudioRazorParserFactory(
                dispatcher,
                errorReporter,
                completionBroker,
                projectEngineFactoryService);
        }
    }
}