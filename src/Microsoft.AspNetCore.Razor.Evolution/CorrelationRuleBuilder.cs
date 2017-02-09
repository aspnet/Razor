// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public class CorrelationRuleBuilder
    {
        private string _tagName;
        private string _parent;
        private TagStructure _tagStructure;
        private List<RazorDiagnostic> _diagnostics;
        private List<TagHelperRequiredAttributeDescriptor> _requiredAttributeDescriptors;

        private CorrelationRuleBuilder()
        {
        }

        public static CorrelationRuleBuilder Create()
        {
            return new CorrelationRuleBuilder();
        }

        public CorrelationRuleBuilder RequireTagName(string tagName)
        {
            _tagName = tagName;

            return this;
        }

        public CorrelationRuleBuilder RequireParent(string parent)
        {
            _parent = parent;

            return this;
        }

        public CorrelationRuleBuilder RequireTagStructure(TagStructure tagStructure)
        {
            _tagStructure = tagStructure;

            return this;
        }

        public CorrelationRuleBuilder RequireAttribute(Action<RequiredTagHelperAttributeDescriptorBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = RequiredTagHelperAttributeDescriptorBuilder.Create();

            configure(builder);

            var requiredAttributeDescriptor = builder.Build();

            EnsureRequiredAttributeDescriptors();
            _requiredAttributeDescriptors.Add(requiredAttributeDescriptor);

            return this;
        }

        public CorrelationRule Build()
        {
            var rule = new DefaultCorrelationRule(
                _tagName,
                _parent,
                _tagStructure,
                _requiredAttributeDescriptors ?? Enumerable.Empty<TagHelperRequiredAttributeDescriptor>(),
                _diagnostics ?? Enumerable.Empty<RazorDiagnostic>());

            return rule;
        }

        private void EnsureRequiredAttributeDescriptors()
        {
            if (_requiredAttributeDescriptors == null)
            {
                _requiredAttributeDescriptors = new List<TagHelperRequiredAttributeDescriptor>();
            }
        }

        private class DefaultCorrelationRule : CorrelationRule
        {
            public DefaultCorrelationRule(
                string tagName,
                string parent,
                TagStructure tagStructure,
                IEnumerable<TagHelperRequiredAttributeDescriptor> requiredAttributeDescriptors,
                IEnumerable<RazorDiagnostic> diagnostics)
            {
                TagName = tagName;
                Parent = parent;
                TagStructure = tagStructure;
                Attributes = requiredAttributeDescriptors;
                Diagnostics = diagnostics;
            }
        }
    }
}
