﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Composition;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.Editor.Razor;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    [Shared]
    [ExportLanguageServiceFactory(typeof(FileChangeTrackerFactory), RazorLanguage.Name, ServiceLayer.Default)]
    internal class DefaultFileChangeTrackerFactoryFactory : ILanguageServiceFactory
    {
        private readonly IVsFileChangeEx _fileChangeService;

        [ImportingConstructor]
        public DefaultFileChangeTrackerFactoryFactory(SVsServiceProvider serviceProvider)
        {
            if (serviceProvider == null)
            {
                throw new ArgumentNullException(nameof(serviceProvider));
            }

            _fileChangeService = serviceProvider.GetService(typeof(SVsFileChangeEx)) as IVsFileChangeEx;
        }

        public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        {
            if (languageServices == null)
            {
                throw new ArgumentNullException(nameof(languageServices));
            }

            var foregroundDispatcher = languageServices.WorkspaceServices.GetRequiredService<ForegroundDispatcher>();
            var errorReporter = languageServices.WorkspaceServices.GetRequiredService<ErrorReporter>();
            return new DefaultFileChangeTrackerFactory(foregroundDispatcher, errorReporter, _fileChangeService);
        }
    }
}
