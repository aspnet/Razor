// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public sealed class RequiredAttributeDescriptorBuilder
    {
        private static ICollection<char> InvalidNonWhitespaceAttributeNameCharacters { get; } = new HashSet<char>(
            new[] { '@', '!', '<', '/', '?', '[', '>', ']', '=', '"', '\'', '*' });

        private string _name;
        private RequiredAttributeDescriptor.NameComparisonMode _nameComparison;
        private string _value;
        private RequiredAttributeDescriptor.ValueComparisonMode _valueComparison;

        private RequiredAttributeDescriptorBuilder()
        {
        }

        public static RequiredAttributeDescriptorBuilder Create()
        {
            return new RequiredAttributeDescriptorBuilder();
        }

        public RequiredAttributeDescriptorBuilder Name(string name)
        {
            _name = name;

            return this;
        }

        public RequiredAttributeDescriptorBuilder NameComparisonMode(RequiredAttributeDescriptor.NameComparisonMode nameComparison)
        {
            _nameComparison = nameComparison;

            return this;
        }

        public RequiredAttributeDescriptorBuilder Value(string value)
        {
            _value = value;

            return this;
        }

        public RequiredAttributeDescriptorBuilder ValueComparisonMode(RequiredAttributeDescriptor.ValueComparisonMode valueComparison)
        {
            _valueComparison = valueComparison;

            return this;
        }

        public RequiredAttributeDescriptor Build()
        {
            var diagnostics = Validate();

            var rule = new DefaultTagHelperRequiredAttributeDescriptor(
                _name,
                _nameComparison,
                _value,
                _valueComparison,
                diagnostics);

            return rule;
        }

        private IEnumerable<RazorDiagnostic> Validate()
        {
            if (string.IsNullOrWhiteSpace(_name))
            {
                var diagnostic = RazorDiagnosticFactory.CreateTagHelper_InvalidTargetedAttributeNameNullOrWhitespace();

                yield return diagnostic;
            }
            else
            {
                foreach (var character in _name)
                {
                    if (char.IsWhiteSpace(character) || InvalidNonWhitespaceAttributeNameCharacters.Contains(character))
                    {
                        var diagnostic = RazorDiagnosticFactory.CreateTagHelper_InvalidTargetedAttributeName(_name, character);

                        yield return diagnostic;
                    }
                }
            }
        }

        private class DefaultTagHelperRequiredAttributeDescriptor : RequiredAttributeDescriptor
        {
            public DefaultTagHelperRequiredAttributeDescriptor(
                string name,
                NameComparisonMode nameComparison,
                string value,
                ValueComparisonMode valueComparison,
                IEnumerable<RazorDiagnostic> diagnostics)
            {
                Name = name;
                NameComparison = nameComparison;
                Value = value;
                ValueComparison = valueComparison;
                Diagnostics = new List<RazorDiagnostic>(diagnostics);
            }
        }
    }
}
