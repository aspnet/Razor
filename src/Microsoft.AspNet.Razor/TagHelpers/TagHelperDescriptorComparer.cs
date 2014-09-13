// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Internal.Web.Utils;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// Defines a an <see cref="IEqualityComparer{TagHelperDescriptor}"/> that is used to check equality amongst
    /// two <see cref="TagHelperDescriptor"/>s.
    /// </summary>
    public class TagHelperDescriptorComparer : IEqualityComparer<TagHelperDescriptor>
    {
        /// <summary>
        /// A default instance of the <see cref="TagHelperDescriptorComparer"/>.
        /// </summary>
        public static readonly TagHelperDescriptorComparer Default = new TagHelperDescriptorComparer();

        /// <summary>
        /// Validates that the two given tag helpers are equal
        /// </summary>
        /// <param name="descriptorX">A <see cref="TagHelperDescriptor"/> to validate against the given 
        /// <paramref name="descriptorY"/>.</param>
        /// <param name="descriptorY">A <see cref="TagHelperDescriptor"/> to validate against the given 
        /// <paramref name="descriptorX"/>.</param>
        /// <returns><c>true</c> if <paramref name="descriptorX"/> and <paramref name="descriptorY"/> are equal,
        /// <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Validates equality based on <see cref="TagHelperDescriptor.TagHelperName"/>, 
        /// <see cref="TagHelperDescriptor.TagName"/> and <see cref="TagHelperDescriptor.ContentBehavior"/>.
        /// </remarks>
        public bool Equals(TagHelperDescriptor descriptorX, TagHelperDescriptor descriptorY)
        {
            return descriptorX.TagHelperName == descriptorY.TagHelperName &&
                   descriptorX.TagName == descriptorY.TagName &&
                   descriptorX.ContentBehavior == descriptorY.ContentBehavior;
        }

        /// <summary>
        /// Creates an <see cref="int"/> value that uniquely identifies the given <see cref="TagHelperDescriptor"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="TagHelperDescriptor"/> to create a hash code for.</param>
        /// <returns>A <see cref="int"/> that uniquely identifies the given <paramref name="descriptor"/>.</returns>
        /// <remarks>Generates a <see cref="int"/> based on <see cref="TagHelperDescriptor.TagName"/>, 
        /// <see cref="TagHelperDescriptor.TagHelperName"/> and <see cref="TagHelperDescriptor.ContentBehavior"/>
        /// </remarks>
        public int GetHashCode(TagHelperDescriptor descriptor)
        {
            return HashCodeCombiner.Start()
                                   .Add(descriptor.TagName)
                                   .Add(descriptor.TagHelperName)
                                   .Add(descriptor.ContentBehavior)
                                   .CombinedHash;
        }
    }
}