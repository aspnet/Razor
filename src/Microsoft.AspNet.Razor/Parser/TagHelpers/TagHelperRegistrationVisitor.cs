// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Razor.Parser.TagHelpers.Internal
{
    public class TagHelperRegistrationVisitor : ParserVisitor
    {
        private HashSet<TagHelperDescriptor> _descriptors;
        private ITagHelperDescriptorResolver _descriptorResolver;

        public TagHelperRegistrationVisitor(ITagHelperDescriptorResolver descriptorResolver)
        {
            _descriptorResolver = descriptorResolver;
        }

        public TagHelperDescriptorProvider CreateProvider(Block root)
        {
            _descriptors = new HashSet<TagHelperDescriptor>(TagHelperDescriptorComparer.Default);

            // This will recurse through the syntax tree.
            VisitBlock(root);

            return new TagHelperDescriptorProvider(_descriptors);
        }

        public override void VisitSpan(Span span)
        {
            // We're only interested in spans with AddTagHelperCodeGenerator's.
            if (span.CodeGenerator is AddTagHelperCodeGenerator)
            {
                var addGenerator = (AddTagHelperCodeGenerator)span.CodeGenerator;

                // Lookup all the descriptors associated with the "LookupText".
                var descriptors = _descriptorResolver.Resolve(addGenerator.LookupText);

                // Add all the found descriptors to our HashSet.  Duplicates are handled by the HashSet.
                foreach (var descriptor in descriptors)
                {
                    _descriptors.Add(descriptor);
                }
            }
        }
    }
}