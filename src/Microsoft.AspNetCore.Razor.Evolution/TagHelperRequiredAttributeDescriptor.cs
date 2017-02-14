// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public abstract class TagHelperRequiredAttributeDescriptor
    {
        public string Name { get; protected set; }

        public TagHelperRequiredAttributeNameComparison NameComparison { get; protected set; }

        public string Value { get; protected set; }

        public TagHelperRequiredAttributeValueComparison ValueComparison { get; protected set; }

        public IEnumerable<RazorDiagnostic> Diagnostics { get; protected set; }

        public bool HasAnyErrors
        {
            get
            {
                var anyErrors = Diagnostics.Any(diagnostic => diagnostic.Severity == RazorDiagnosticSeverity.Error);

                return anyErrors;
            }
        }

        /// <summary>
        /// Determines if the current <see cref="TagHelperRequiredAttributeDescriptor"/> matches the given
        /// <paramref name="attributeName"/> and <paramref name="attributeValue"/>.
        /// </summary>
        /// <param name="attributeName">An HTML attribute name.</param>
        /// <param name="attributeValue">An HTML attribute value.</param>
        /// <returns><c>true</c> if the current <see cref="TagHelperRequiredAttributeDescriptor"/> matches
        /// <paramref name="attributeName"/> and <paramref name="attributeValue"/>; <c>false</c> otherwise.</returns>
        public bool IsMatch(string attributeName, string attributeValue)
        {
            var nameMatches = false;
            if (NameComparison == TagHelperRequiredAttributeNameComparison.FullMatch)
            {
                nameMatches = string.Equals(Name, attributeName, StringComparison.OrdinalIgnoreCase);
            }
            else if (NameComparison == TagHelperRequiredAttributeNameComparison.PrefixMatch)
            {
                // attributeName cannot equal the Name if comparing as a PrefixMatch.
                nameMatches = attributeName.Length != Name.Length &&
                    attributeName.StartsWith(Name, StringComparison.OrdinalIgnoreCase);
            }
            else
            {
                Debug.Assert(false, "Unknown name comparison.");
            }

            if (!nameMatches)
            {
                return false;
            }

            switch (ValueComparison)
            {
                case TagHelperRequiredAttributeValueComparison.None:
                    return true;
                case TagHelperRequiredAttributeValueComparison.PrefixMatch: // Value starts with
                    return attributeValue.StartsWith(Value, StringComparison.Ordinal);
                case TagHelperRequiredAttributeValueComparison.SuffixMatch: // Value ends with
                    return attributeValue.EndsWith(Value, StringComparison.Ordinal);
                case TagHelperRequiredAttributeValueComparison.FullMatch: // Value equals
                    return string.Equals(attributeValue, Value, StringComparison.Ordinal);
                default:
                    Debug.Assert(false, "Unknown value comparison.");
                    return false;
            }
        }
    }
}
