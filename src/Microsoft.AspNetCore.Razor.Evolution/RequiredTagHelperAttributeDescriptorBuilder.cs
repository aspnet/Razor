// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public class RequiredTagHelperAttributeDescriptorBuilder
    {
        private string _name;
        private TagHelperRequiredAttributeNameComparison _nameComparison;
        private string _value;
        private TagHelperRequiredAttributeValueComparison _valueComparison;
        private List<RazorDiagnostic> _diagnostics;

        private RequiredTagHelperAttributeDescriptorBuilder()
        {
        }

        public static RequiredTagHelperAttributeDescriptorBuilder Create()
        {
            return new RequiredTagHelperAttributeDescriptorBuilder();
        }

        public RequiredTagHelperAttributeDescriptorBuilder Name(string name)
        {
            _name = name;

            return this;
        }

        public RequiredTagHelperAttributeDescriptorBuilder NameComparison(TagHelperRequiredAttributeNameComparison nameComparison)
        {
            _nameComparison = nameComparison;

            return this;
        }

        public RequiredTagHelperAttributeDescriptorBuilder Value(string value)
        {
            _value = value;

            return this;
        }

        public RequiredTagHelperAttributeDescriptorBuilder ValueComparison(TagHelperRequiredAttributeValueComparison valueComparison)
        {
            _valueComparison = valueComparison;

            return this;
        }

        public TagHelperRequiredAttributeDescriptor Build()
        {
            var rule = new DefaultTagHelperRequiredAttributeDescriptor(
                _name,
                _nameComparison,
                _value,
                _valueComparison,
                _diagnostics ?? Enumerable.Empty<RazorDiagnostic>());

            return rule;
        }

        private class DefaultTagHelperRequiredAttributeDescriptor : TagHelperRequiredAttributeDescriptor
        {
            public DefaultTagHelperRequiredAttributeDescriptor(
                string name,
                TagHelperRequiredAttributeNameComparison nameComparison,
                string value,
                TagHelperRequiredAttributeValueComparison valueComparison,
                IEnumerable<RazorDiagnostic> diagnostics)
            {
                Name = name;
                NameComparison = nameComparison;
                Value = value;
                ValueComparison = valueComparison;
                Diagnostics = diagnostics;
            }
        }
    }
}
