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
        /// <param name="name">HTML name of the attribute.</param>
        /// <param name="descriptors">
        /// Collection of <see cref="TagHelperDescriptor"/>s targeting the containing element in the Razor source.
        /// </param>
        /// <returns>
        /// The full name of the <see cref="Type"/> of the property corresponding to an attribute named
        /// <paramref name="name"/>. <c>null</c> if the attribute is not bound.
        /// </returns>
        public static string GetPropertyType(
            [NotNull] string name,
            [NotNull] IEnumerable<TagHelperDescriptor> descriptors)
        {
            var firstBoundAttribute = FindFirstBoundAttribute(name, descriptors);

            return firstBoundAttribute?.TypeName;
        }

        /// <summary>
        /// Determines whether an attribute named <paramref name="name"/> is bound to a non-<see cref="string"/> tag
        /// helper property.
        /// </summary>
        /// <param name="name">HTML name of the attribute.</param>
        /// <param name="descriptors">
        /// Collection of <see cref="TagHelperDescriptor"/>s targeting the containing element in the Razor source.
        /// </param>
        /// <param name="isBoundNonStringAttribute">Set to <c>true</c> if the attribute named <paramref name="name"/>
        /// is bound to a non-<see cref="string"/> tag helper property. <c>false</c> otherwise.</param>
        /// <returns>
        /// <c>true</c> if the attribute named <paramref name="name"/> is bound and the associated property does not
        /// have <see cref="Type"/> <see cref="string"/>. <c>false</c> otherwise e.g. if the attribute is not bound.
        /// </returns>
        public static bool IsBoundAttribute(
            [NotNull] string name,
            [NotNull] IEnumerable<TagHelperDescriptor> descriptors,
            out bool isBoundNonStringAttribute)
        {
            var firstBoundAttribute = FindFirstBoundAttribute(name, descriptors);
            var isBoundAttribute = firstBoundAttribute != null;
            isBoundNonStringAttribute = isBoundAttribute && !!firstBoundAttribute.IsStringProperty;

            return isBoundAttribute;
        }

        // Find first TagHelperAttributeDescriptor matching given name.
        private static TagHelperAttributeDescriptor FindFirstBoundAttribute(
            string name,
            IEnumerable<TagHelperDescriptor> descriptors)
        {
            return descriptors
                .SelectMany(descriptor => descriptor.Attributes)
                .FirstOrDefault(attribute => string.Equals(attribute.Name, name, StringComparison.OrdinalIgnoreCase));
        }
    }
}