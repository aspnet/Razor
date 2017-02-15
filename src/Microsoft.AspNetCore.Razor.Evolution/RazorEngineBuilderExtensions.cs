﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Evolution.CodeGeneration;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public static class RazorEngineBuilderExtensions
    {
        public static IRazorEngineBuilder AddDirective(this IRazorEngineBuilder builder, DirectiveDescriptor directive)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (directive == null)
            {
                throw new ArgumentNullException(nameof(directive));
            }

            var directiveFeature = GetDirectiveFeature(builder);
            directiveFeature.Directives.Add(directive);

            return builder;
        }

        public static IRazorEngineBuilder AddTargetExtension(this IRazorEngineBuilder builder, IRuntimeTargetExtension extension)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            var targetExtensionFeature = GetTargetExtensionFeature(builder);
            targetExtensionFeature.TargetExtensions.Add(extension);

            return builder;
        }

        private static IRazorDirectiveFeature GetDirectiveFeature(IRazorEngineBuilder builder)
        {
            var directiveFeature = builder.Features.OfType<IRazorDirectiveFeature>().FirstOrDefault();
            if (directiveFeature == null)
            {
                directiveFeature = new DefaultRazorDirectiveFeature();
                builder.Features.Add(directiveFeature);
            }

            return directiveFeature;
        }

        private static IRazorTargetExtensionFeature GetTargetExtensionFeature(IRazorEngineBuilder builder)
        {
            var targetExtensionFeature = builder.Features.OfType<IRazorTargetExtensionFeature>().FirstOrDefault();
            if (targetExtensionFeature == null)
            {
                targetExtensionFeature = new DefaultRazorTargetExtensionFeature();
                builder.Features.Add(targetExtensionFeature);
            }

            return targetExtensionFeature;
        }
    }
}
