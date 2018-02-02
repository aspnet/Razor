﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.CodeAnalysis.Razor
{
    public sealed class CompilationTagHelperFeature : RazorEngineFeatureBase, ITagHelperFeature
    {
        private ITagHelperDescriptorProvider[] _providers;
        private IMetadataReferenceFeature _referenceFeature;

        public CSharpCompilation Compilation { get; set; }

        public IReadOnlyList<TagHelperDescriptor> GetDescriptors()
        {
            var results = new List<TagHelperDescriptor>();

            var context = TagHelperDescriptorProviderContext.Create(results);
            var compilation = Compilation ?? CSharpCompilation.Create("__TagHelpers", references: _referenceFeature.References);
            context.SetCompilation(compilation);

            for (var i = 0; i < _providers.Length; i++)
            {
                _providers[i].Execute(context);
            }

            return results;
        }

        protected override void OnInitialized()
        {
            _referenceFeature = Engine.Features.OfType<IMetadataReferenceFeature>().FirstOrDefault();
            _providers = Engine.Features.OfType<ITagHelperDescriptorProvider>().OrderBy(f => f.Order).ToArray();
        }
    }
}
