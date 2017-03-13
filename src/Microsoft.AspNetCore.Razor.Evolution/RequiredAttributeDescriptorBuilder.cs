﻿// Copyright (c) .NET Foundation. All rights reserved.
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
        private HashSet<RazorDiagnostic> _diagnostics;

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

        public RequiredAttributeDescriptorBuilder AddDiagnostic(RazorDiagnostic diagnostic)
        {
            EnsureDiagnostics();
            _diagnostics.Add(diagnostic);

            return this;
        }

        public IEnumerable<RazorDiagnostic> Validate()
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

        public RequiredAttributeDescriptor Build()
        {
            var rule = new DefaultTagHelperRequiredAttributeDescriptor(
                _name,
                _nameComparison,
                _value,
                _valueComparison,
                _diagnostics ?? Enumerable.Empty<RazorDiagnostic>());

            return rule;
        }

        private void EnsureDiagnostics()
        {
            if (_diagnostics == null)
            {
                _diagnostics = new HashSet<RazorDiagnostic>();
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
                Diagnostics = diagnostics;
            }
        }
    }
}
