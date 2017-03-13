// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    internal class TagHelperDescriptorComparer : IEqualityComparer<TagHelperDescriptor>
    {
        public static readonly TagHelperDescriptorComparer Default = new TagHelperDescriptorComparer();

        protected TagHelperDescriptorComparer()
        {
        }

        public virtual bool Equals(TagHelperDescriptor descriptorX, TagHelperDescriptor descriptorY)
        {
            if (descriptorX == descriptorY)
            {
                return true;
            }

            return descriptorX != null &&
                string.Equals(descriptorX.Kind, descriptorY.Kind, StringComparison.Ordinal) &&
                string.Equals(descriptorX.AssemblyName, descriptorY.AssemblyName, StringComparison.Ordinal) &&
                Enumerable.SequenceEqual(
                    descriptorX.BoundAttributes.OrderBy(attribute => attribute.Name, StringComparer.OrdinalIgnoreCase),
                    descriptorY.BoundAttributes.OrderBy(attribute => attribute.Name, StringComparer.OrdinalIgnoreCase),
                    BoundAttributeDescriptorComparer.Default) &&
                Enumerable.SequenceEqual(
                    descriptorX.TagMatchingRules.OrderBy(rule => rule.TagName, StringComparer.OrdinalIgnoreCase),
                    descriptorY.TagMatchingRules.OrderBy(rule => rule.TagName, StringComparer.OrdinalIgnoreCase),
                    TagMatchingRuleComparer.Default) &&
                (descriptorX.AllowedChildTags == descriptorY.AllowedChildTags ||
                (descriptorX.AllowedChildTags != null &&
                descriptorY.AllowedChildTags != null &&
                Enumerable.SequenceEqual(
                    descriptorX.AllowedChildTags.OrderBy(child => child, StringComparer.OrdinalIgnoreCase),
                    descriptorY.AllowedChildTags.OrderBy(child => child, StringComparer.OrdinalIgnoreCase),
                    StringComparer.OrdinalIgnoreCase))) &&
                string.Equals(descriptorX.Documentation, descriptorY.Documentation, StringComparison.Ordinal) &&
                string.Equals(descriptorX.DisplayName, descriptorY.DisplayName, StringComparison.Ordinal) &&
                string.Equals(descriptorX.TagOutputHint, descriptorY.TagOutputHint, StringComparison.OrdinalIgnoreCase) &&
                Enumerable.SequenceEqual(descriptorX.Diagnostics, descriptorY.Diagnostics) &&
                Enumerable.SequenceEqual(
                    descriptorX.Metadata.OrderBy(metadataX => metadataX.Key, StringComparer.Ordinal),
                    descriptorY.Metadata.OrderBy(metadataY => metadataY.Key, StringComparer.Ordinal));
        }

        /// <inheritdoc />
        public virtual int GetHashCode(TagHelperDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(descriptor.Kind);
            hashCodeCombiner.Add(descriptor.AssemblyName, StringComparer.Ordinal);

            var rules = descriptor.TagMatchingRules.OrderBy(rule => rule.TagName, StringComparer.OrdinalIgnoreCase);
            foreach (var rule in rules)
            {
                hashCodeCombiner.Add(TagMatchingRuleComparer.Default.GetHashCode(rule));
            }

            hashCodeCombiner.Add(descriptor.Documentation, StringComparer.Ordinal);
            hashCodeCombiner.Add(descriptor.DisplayName, StringComparer.Ordinal);
            hashCodeCombiner.Add(descriptor.TagOutputHint, StringComparer.OrdinalIgnoreCase);

            if (descriptor.AllowedChildTags != null)
            {
                var allowedChildren = descriptor.AllowedChildTags.OrderBy(child => child, StringComparer.OrdinalIgnoreCase);
                foreach (var child in allowedChildren)
                {
                    hashCodeCombiner.Add(child, StringComparer.OrdinalIgnoreCase);
                }
            }

            return hashCodeCombiner.CombinedHash;
        }
    }
}