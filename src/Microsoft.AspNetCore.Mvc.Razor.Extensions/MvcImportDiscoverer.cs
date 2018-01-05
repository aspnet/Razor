// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Razor.Language;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions
{
    internal class MvcImportDiscoverer : RazorProjectEngineFeatureBase, IRazorImportDiscoverer
    {
        // Need to run prior to the default import discoverers so these are overridden by any user imports.
        public int Order => -10;

        public void Execute(ImportDiscoveryContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            using (var stream = new MemoryStream())
            using (var writer = new StreamWriter(stream, Encoding.UTF8))
            {
                writer.WriteLine("@using System");
                writer.WriteLine("@using System.Collections.Generic");
                writer.WriteLine("@using System.Linq");
                writer.WriteLine("@using System.Threading.Tasks");
                writer.WriteLine("@using Microsoft.AspNetCore.Mvc");
                writer.WriteLine("@using Microsoft.AspNetCore.Mvc.Rendering");
                writer.WriteLine("@using Microsoft.AspNetCore.Mvc.ViewFeatures");
                writer.WriteLine("@inject global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<TModel> Html");
                writer.WriteLine("@inject global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json");
                writer.WriteLine("@inject global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component");
                writer.WriteLine("@inject global::Microsoft.AspNetCore.Mvc.IUrlHelper Url");
                writer.WriteLine("@inject global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider");
                writer.WriteLine("@addTagHelper Microsoft.AspNetCore.Mvc.Razor.TagHelpers.UrlResolutionTagHelper, Microsoft.AspNetCore.Mvc.Razor");
                writer.WriteLine("@addTagHelper Microsoft.AspNetCore.Mvc.Razor.TagHelpers.HeadTagHelper, Microsoft.AspNetCore.Mvc.Razor");
                writer.WriteLine("@addTagHelper Microsoft.AspNetCore.Mvc.Razor.TagHelpers.BodyTagHelper, Microsoft.AspNetCore.Mvc.Razor");
                writer.Flush();

                stream.Position = 0;
                var defaultMvcImports = RazorSourceDocument.ReadFrom(stream, fileName: null, encoding: Encoding.UTF8);
                context.Results.Add(defaultMvcImports);
            }
        }
    }
}
