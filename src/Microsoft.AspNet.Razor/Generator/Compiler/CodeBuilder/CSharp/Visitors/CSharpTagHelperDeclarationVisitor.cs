// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Razor.Generator.Compiler.CSharp
{
    public class CSharpTagHelperDeclarationVisitor : CodeVisitor<CSharpCodeWriter>
    {
        private bool _foundTagHelpers;
        private HashSet<TagHelperDescriptor> _declaredDescriptors;
        private GeneratedTagHelperContext _tagHelperContext;

        public CSharpTagHelperDeclarationVisitor([NotNull] CSharpCodeWriter writer,
                                                 [NotNull] CodeBuilderContext context)
            : base(writer, context)
        {
            _declaredDescriptors = new HashSet<TagHelperDescriptor>(TagHelperDescriptorComparer.Default);
            _tagHelperContext = Context.Host.GeneratedClassContext.GeneratedTagHelperContext;
        }

        protected override void Visit(TagHelperChunk chunk)
        {
            // We only want to setup tag helper manager variables if there are tag helpers, and only once
            if (!_foundTagHelpers)
            {
                _foundTagHelpers = true;

                Writer.WriteVariableDeclaration("var",
                                                CSharpTagHelperCodeRenderer.BufferedAttributeValueVariableName,
                                                "string.Empty");
            }

            foreach (var descriptor in chunk.Descriptors)
            {
                if (!_declaredDescriptors.Contains(descriptor))
                {
                    _declaredDescriptors.Add(descriptor);

                    Writer.Write(descriptor.TagHelperName)
                          .Write(" ")
                          .Write(CSharpTagHelperCodeRenderer.GetVariableName(descriptor))
                          .WriteLine(";");
                }
            }

            // We need to dive deeper to ensure we pick up any nested tag helpers.
            Accept(chunk.Children);
        }
    }
}