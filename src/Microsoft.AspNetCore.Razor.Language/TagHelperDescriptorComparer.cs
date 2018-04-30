// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class TagHelperDescriptorComparer : IEqualityComparer<TagHelperDescriptor>
    {
        /// <summary>
        /// A default instance of the <see cref="TagHelperDescriptorComparer"/>.
        /// </summary>
        public static readonly TagHelperDescriptorComparer Default = new TagHelperDescriptorComparer();

        /// <summary>
        /// A default instance of the <see cref="TagHelperDescriptorComparer"/> that does case-sensitive comparison.
        /// </summary>
        internal static readonly TagHelperDescriptorComparer CaseSensitive =
            new TagHelperDescriptorComparer(caseSensitive: true);

        private readonly StringComparer _stringComparer;
        private readonly StringComparison _stringComparison;
        private readonly AllowedChildTagDescriptorComparer _AllowedChildTagDescriptorComparer;
        private readonly BoundAttributeDescriptorComparer _boundAttributeComparer;
        private readonly TagMatchingRuleDescriptorComparer _tagMatchingRuleComparer;

        private TagHelperDescriptorComparer(bool caseSensitive = false)
        {
            if (caseSensitive)
            {
                _stringComparer = StringComparer.Ordinal;
                _stringComparison = StringComparison.Ordinal;
                _AllowedChildTagDescriptorComparer = AllowedChildTagDescriptorComparer.CaseSensitive;
                _boundAttributeComparer = BoundAttributeDescriptorComparer.CaseSensitive;
                _tagMatchingRuleComparer = TagMatchingRuleDescriptorComparer.CaseSensitive;
            }
            else
            {
                _stringComparer = StringComparer.OrdinalIgnoreCase;
                _stringComparison = StringComparison.OrdinalIgnoreCase;
                _AllowedChildTagDescriptorComparer = AllowedChildTagDescriptorComparer.Default;
                _boundAttributeComparer = BoundAttributeDescriptorComparer.Default;
                _tagMatchingRuleComparer = TagMatchingRuleDescriptorComparer.Default;
            }
        }

        public virtual bool Equals(TagHelperDescriptor descriptorX, TagHelperDescriptor descriptorY)
        {
            if (object.ReferenceEquals(descriptorX, descriptorY))
            {
                return true;
            }

            if (descriptorX == null ^ descriptorY == null)
            {
                return false;
            }

            return descriptorX != null &&
                string.Equals(descriptorX.Kind, descriptorY.Kind, StringComparison.Ordinal) &&
                string.Equals(descriptorX.AssemblyName, descriptorY.AssemblyName, StringComparison.Ordinal) &&
                string.Equals(descriptorX.Name, descriptorY.Name, StringComparison.Ordinal) &&
                Enumerable.SequenceEqual(
                    descriptorX.BoundAttributes.OrderBy(attribute => attribute.Name, _stringComparer),
                    descriptorY.BoundAttributes.OrderBy(attribute => attribute.Name, _stringComparer),
                    _boundAttributeComparer) &&
                Enumerable.SequenceEqual(
                    descriptorX.TagMatchingRules.OrderBy(rule => rule.TagName, _stringComparer),
                    descriptorY.TagMatchingRules.OrderBy(rule => rule.TagName, _stringComparer),
                    _tagMatchingRuleComparer) &&
                (descriptorX.AllowedChildTags == descriptorY.AllowedChildTags ||
                (descriptorX.AllowedChildTags != null &&
                descriptorY.AllowedChildTags != null &&
                Enumerable.SequenceEqual(
                    descriptorX.AllowedChildTags.OrderBy(childTag => childTag.Name, _stringComparer),
                    descriptorY.AllowedChildTags.OrderBy(childTag => childTag.Name, _stringComparer),
                    _AllowedChildTagDescriptorComparer))) &&
                string.Equals(descriptorX.Documentation, descriptorY.Documentation, StringComparison.Ordinal) &&
                string.Equals(descriptorX.DisplayName, descriptorY.DisplayName, StringComparison.Ordinal) &&
                string.Equals(descriptorX.TagOutputHint, descriptorY.TagOutputHint, _stringComparison) &&
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

            var hash = HashCodeCombiner.Start();
            hash.Add(descriptor.Kind, StringComparer.Ordinal);
            hash.Add(descriptor.AssemblyName, StringComparer.Ordinal);
            hash.Add(descriptor.Name, StringComparer.Ordinal);

            return hash.CombinedHash;
        }
    }
}