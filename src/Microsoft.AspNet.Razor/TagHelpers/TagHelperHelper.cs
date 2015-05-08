// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// Helper methods related to <see cref="TagHelperDescriptor"/> and related classes.
    /// </summary>
    public static class TagHelperHelper
    {
        /// <summary>
        /// Determine the full name of the <see cref="Type"/> of the property corresponding to an attribute named
        /// <paramref name="name"/>.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="descriptors">
        /// Collection of <see cref="TagHelperDescriptor"/>s targeting the containing element in the Razor source.
        /// </param>
        /// <returns>
        /// The full name of the <see cref="Type"/> of property or dictionary value (in the case of a
        /// <see cref="TagHelperAttributeDescriptor.Prefix"/> match) corresponding to an attribute named
        /// <paramref name="name"/>. <c>null</c> if the attribute is not bound.
        /// </returns>
        /// <remarks>
        /// This method cannot use Reflection to generally determine the most specific <see cref="Type"/> when
        /// multiple tag helpers bind the attribute. However when it can determine an attribute is bound to a
        /// <see cref="string"/> property or dictionary value, it always returns <c>"System.String"</c>. Otherwise it
        /// returns the full <see cref="Type"/> name for the first binding it finds, if any.
        /// </remarks>
        public static string GetPropertyType(
            [NotNull] string name,
            [NotNull] IEnumerable<TagHelperDescriptor> descriptors)
        {
            // Find first TagHelperAttributeDescriptor matching this name.
            var firstBoundAttribute = descriptors
                .SelectMany(descriptor => descriptor.Attributes)
                .FirstOrDefault(attribute =>
                    string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase) ||
                    (attribute.Prefix != null &&
                     name.StartsWith(attribute.Prefix, StringComparison.OrdinalIgnoreCase)));
            if (firstBoundAttribute == null)
            {
                return null;
            }

            if (!IsBoundNonStringAttribute(name, descriptors))
            {
                // Must have been a string attribute since we know it is bound. Use that type.
                return typeof(string).FullName;
            }

            if (string.Equals(firstBoundAttribute.Name, name, StringComparison.OrdinalIgnoreCase))
            {
                return firstBoundAttribute.TypeName;
            }

            // Attribute is bound and doesn't match Name. Must be a Prefix match.
            return firstBoundAttribute.PrefixedValueTypeName;
        }

        /// <summary>
        /// Determines whether an attribute named <paramref name="name"/> is bound exclusively to
        /// non-<see cref="string"/> tag helper properties.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="descriptors">
        /// Collection of <see cref="TagHelperDescriptor"/>s targeting the containing element in the Razor source.
        /// </param>
        /// <returns>
        /// <c>true</c> if the attribute named <paramref name="name"/> is bound and no associated property or
        /// dictionary value (in the case of a <see cref="TagHelperAttributeDescriptor.Prefix"/> match) has
        /// <see cref="Type"/> <see cref="string"/>. <c>false</c> otherwise e.g. if the property is not bound.
        /// </returns>
        public static bool IsBoundNonStringAttribute(
            [NotNull] string name,
            [NotNull] IEnumerable<TagHelperDescriptor> descriptors)
        {
            // Find all TagHelperAttributeDescriptor's matching this name.
            var boundAttributes = descriptors
                .SelectMany(descriptor => descriptor.Attributes)
                .Where(attribute => string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase) ||
                    (attribute.Prefix != null &&
                     name.StartsWith(attribute.Prefix, StringComparison.OrdinalIgnoreCase)));

            // Check if any matching TagHelperAttributeDescriptor requires a string value.
            var isStringValue = boundAttributes.Any(attribute =>
            {
                if (string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase))
                {
                    return attribute.IsStringProperty;
                }

                // Attribute is bound and doesn't match Name. Must be a Prefix match.
                return attribute.AreStringPrefixedValues;
            });

            return boundAttributes.Any() && !isStringValue;
        }
    }
}
