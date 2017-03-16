// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public class ITagHelperDescriptorBuilder
    {
        public static readonly string DescriptorKind = "ITagHelper";
        public static readonly string ITagHelperTypeNameKey = "ITagHelper.TypeName";

        private string _assemblyName;
        private string _typeName;
        private string _documentation;
        private string _outputElementHint;
        private List<string> _allowedChildren;
        private List<TagHelperAttributeDescriptor> _attributeDescriptors;
        private List<CorrelationRule> _correlationRules;
        private List<RazorDiagnostic> _diagnostics;
        private readonly Dictionary<string, string> _propertyBag;

        private ITagHelperDescriptorBuilder(string typeName, string assemblyName)
        {
            _assemblyName = assemblyName;
            _typeName = typeName;
            _propertyBag = new Dictionary<string, string>(StringComparer.Ordinal);
        }

        public static ITagHelperDescriptorBuilder Create(string typeName, string assemblyName)
        {
            return new ITagHelperDescriptorBuilder(typeName, assemblyName);
        }

        public ITagHelperDescriptorBuilder BindAttribute(Action<ITagHelperAttributeDescriptorBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = ITagHelperAttributeDescriptorBuilder.Create(_typeName);

            configure(builder);

            var attributeDescriptor = builder.Build();

            EnsureAttributeDescriptors();
            _attributeDescriptors.Add(attributeDescriptor);

            return this;
        }

        public ITagHelperDescriptorBuilder CorrelationRule(Action<CorrelationRuleBuilder> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = CorrelationRuleBuilder.Create();

            configure(builder);

            var rule = builder.Build();

            EnsureCorrelationRules();
            _correlationRules.Add(rule);

            return this;
        }

        public ITagHelperDescriptorBuilder AllowChild(string allowedChild)
        {
            EnsureAllowedChildren();
            _allowedChildren.Add(allowedChild);

            return this;
        }

        public ITagHelperDescriptorBuilder OutputElementHint(string hint)
        {
            _outputElementHint = hint;

            return this;
        }

        public ITagHelperDescriptorBuilder Documentation(string documentation)
        {
            _documentation = documentation;

            return this;
        }

        public ITagHelperDescriptorBuilder AddMetadata(string key, string value)
        {
            _propertyBag[key] = value;

            return this;
        }

        public TagHelperDescriptor Build()
        {
            var descriptor = new ITagHelperDescriptor(
                _typeName,
                _assemblyName,
                _typeName /* DisplayName */,
                _documentation,
                _outputElementHint,
                _correlationRules ?? Enumerable.Empty<CorrelationRule>(),
                _attributeDescriptors ?? Enumerable.Empty<TagHelperAttributeDescriptor>(),
                _allowedChildren,
                _propertyBag,
                _diagnostics ?? Enumerable.Empty<RazorDiagnostic>());

            return descriptor;
        }

        private void EnsureAttributeDescriptors()
        {
            if (_attributeDescriptors == null)
            {
                _attributeDescriptors = new List<TagHelperAttributeDescriptor>();
            }
        }

        private void EnsureCorrelationRules()
        {
            if (_correlationRules == null)
            {
                _correlationRules = new List<CorrelationRule>();
            }
        }

        private void EnsureAllowedChildren()
        {
            if (_allowedChildren == null)
            {
                _allowedChildren = new List<string>();
            }
        }

        private class ITagHelperDescriptor : TagHelperDescriptor
        {
            public ITagHelperDescriptor(
                string typeName,
                string assemblyName,
                string displayName,
                string documentation,
                string outputElementHint,
                IEnumerable<CorrelationRule> correlationRules,
                IEnumerable<TagHelperAttributeDescriptor> attributeDescriptors,
                IEnumerable<string> allowedChildren,
                Dictionary<string, string> propertyBag,
                IEnumerable<RazorDiagnostic> diagnostics) : base(DescriptorKind)
            {
                AssemblyName = assemblyName;
                DisplayName = displayName;
                Documentation = documentation;
                OutputElementHint = outputElementHint;
                CorrelationRules = correlationRules;
                Attributes = attributeDescriptors;
                AllowedChildren = allowedChildren;
                Diagnostics = diagnostics;

                propertyBag[ITagHelperTypeNameKey] = typeName;
                PropertyBag = propertyBag;
            }
        }
    }
}
