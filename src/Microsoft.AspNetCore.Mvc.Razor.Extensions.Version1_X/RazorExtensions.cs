﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Extensions;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions.Version1_X
{
    public static class RazorExtensions
    {
        public static void Register(IRazorEngineBuilder builder)
        {
            EnsureDesignTime(builder);

            InjectDirective.Register(builder);
            ModelDirective.Register(builder);

            FunctionsDirective.Register(builder);
            InheritsDirective.Register(builder);

            // Register section directive with the 1.x compatible target extension.
            builder.AddDirective(SectionDirective.Directive);
            builder.Features.Add(new SectionDirectivePass());
            builder.AddTargetExtension(new LegacySectionTargetExtension());

            builder.AddTargetExtension(new TemplateTargetExtension()
            {
                TemplateTypeName = "global::Microsoft.AspNetCore.Mvc.Razor.HelperResult",
            });

            builder.Features.Add(new ModelExpressionPass());
            builder.Features.Add(new MvcViewDocumentClassifierPass());
        }

        public static void Register(RazorProjectEngineBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.SetImportFileName("_ViewImports.cshtml");

            builder.Features.Add(new MvcImportDiscoverer());
            builder.Features.Add(new RelativePathCodeDocumentProcessor());
        }

        public static void RegisterViewComponentTagHelpers(IRazorEngineBuilder builder)
        {
            EnsureDesignTime(builder);

            builder.Features.Add(new ViewComponentTagHelperPass());
            builder.AddTargetExtension(new ViewComponentTagHelperTargetExtension());
        }

        private static void EnsureDesignTime(IRazorEngineBuilder builder)
        {
            if (builder.DesignTime)
            {
                return;
            }

            throw new NotSupportedException(Resources.RuntimeCodeGenerationNotSupported);
        }
    }
}
