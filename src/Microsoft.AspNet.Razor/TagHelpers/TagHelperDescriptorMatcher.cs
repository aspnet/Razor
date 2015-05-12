// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// Helper methods related to <see cref="TagHelperDescriptor"/> and related classes.
    /// </summary>
    public static class TagHelperDescriptorMatcher
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
        /// The full name of the <see cref="Type"/> of the property corresponding to an attribute named
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
                .FirstOrDefault(attribute => string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase));
            if (firstBoundAttribute == null)
            {
                return null;
            }

            bool isBoundNonStringAttribute;
            var isBoundAttribute = IsBoundAttribute(name, descriptors, out isBoundNonStringAttribute);
            Debug.Assert(isBoundAttribute);

            if (!isBoundNonStringAttribute)
            {
                // Must have been a string attribute since we know it is bound. Use that type.
                return typeof(string).FullName;
            }

            // Attribute is bound but never to a string property.
            return firstBoundAttribute.TypeName;
        }

        /// <summary>
        /// Determines whether an attribute named <paramref name="name"/> is bound to a tag helper property.
        /// </summary>
        /// <param name="name">Name of the attribute.</param>
        /// <param name="descriptors">
        /// Collection of <see cref="TagHelperDescriptor"/>s targeting the containing element in the Razor source.
        /// </param>
        /// <param name="isBoundNonStringAttribute">Set to <c>true</c> if the attribute named <paramref name="name"/>
        /// is bound to a non-<see cref="string"/> tag helper property. <c>false</c> otherwise.</param>
        /// <returns>
        /// <c>true</c> if the attribute named <paramref name="name"/> is bound and no associated property has
        /// <see cref="Type"/> <see cref="string"/>. <c>false</c> otherwise e.g. if the property is not bound.
        /// </returns>
        public static bool IsBoundAttribute(
            [NotNull] string name,
            [NotNull] IEnumerable<TagHelperDescriptor> descriptors,
            out bool isBoundNonStringAttribute)
        {
            // Find all TagHelperAttributeDescriptor's matching this name.
            var boundAttributes = descriptors
                .SelectMany(descriptor => descriptor.Attributes)
                .Where(attribute => string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase));

            // Check if any matching TagHelperAttributeDescriptor requires a string value.
            var isStringValue = boundAttributes.Any(attribute => attribute.IsStringProperty);

            var isBoundAttribute = boundAttributes.Any();
            isBoundNonStringAttribute = isBoundAttribute && !isStringValue;

            return isBoundAttribute;
        }
    }
}
