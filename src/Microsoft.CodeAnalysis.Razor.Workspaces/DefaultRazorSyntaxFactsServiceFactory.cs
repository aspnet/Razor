﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Razor;

namespace Microsoft.CodeAnalysis.Razor
{
    [ExportLanguageServiceFactory(typeof(RazorSyntaxFactsService), RazorLanguage.Name, ServiceLayer.Default)]
    internal class DefaultRazorSyntaxFactsServiceFactory : ILanguageServiceFactory
    {
        public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        {
            return new DefaultRazorSyntaxFactsService();
        }
    }
}