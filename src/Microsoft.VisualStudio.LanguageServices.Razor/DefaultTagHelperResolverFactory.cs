// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Composition;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.Shell;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    [ExportLanguageServiceFactory(typeof(ITagHelperResolver), RazorLanguage.Name, ServiceLayer.Default)]
    internal class DefaultTagHelperResolverFactory : ILanguageServiceFactory
    {
        private readonly VisualStudioWorkspace _workspace;
        private readonly SVsServiceProvider _services;

        [ImportingConstructor]
        public DefaultTagHelperResolverFactory(VisualStudioWorkspace workspace, SVsServiceProvider services)
        {
            _workspace = workspace;
            _services = services;
        }

        public ILanguageService CreateLanguageService(HostLanguageServices languageServices)
        {
            return new DefaultTagHelperResolver()
            {
                Workspace = _workspace,
                Services = _services,
            };
        }
    }
}