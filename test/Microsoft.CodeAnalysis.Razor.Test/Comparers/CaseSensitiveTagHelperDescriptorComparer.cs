// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;
using Microsoft.Extensions.Internal;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.Workspaces.Test.Comparers
{
    internal class CaseSensitiveTagHelperDescriptorComparer : TagHelperDescriptorComparer
    {
        public new static readonly CaseSensitiveTagHelperDescriptorComparer Default =
            new CaseSensitiveTagHelperDescriptorComparer();

        private CaseSensitiveTagHelperDescriptorComparer()
            : base()
        {
        }

        public override bool Equals(TagHelperDescriptor descriptorX, TagHelperDescriptor descriptorY)
        {
            if (descriptorX == descriptorY)
            {
                return true;
            }

            // Normal comparer doesn't care about the case, required attribute order, allowed children order,
            // attributes or prefixes. In tests we do.
            Assert.Equal(descriptorX.TagOutputHint, descriptorY.TagOutputHint);
            Assert.Equal(descriptorX.BoundAttributes, descriptorY.BoundAttributes, CaseSensitiveBoundAttributeDescriptorComparer.Default);
            Assert.Equal(descriptorX.TagMatchingRules, descriptorY.TagMatchingRules, CaseSensitiveTagMatchingRuleComparer.Default);
            Assert.True(base.Equals(descriptorX, descriptorY));

            if (descriptorX.AllowedChildTags != descriptorY.AllowedChildTags)
            {
                Assert.Equal(descriptorX.AllowedChildTags, descriptorY.AllowedChildTags, StringComparer.Ordinal);
            }

            return true;
        }

        public override int GetHashCode(TagHelperDescriptor descriptor)
        {
            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(base.GetHashCode(descriptor));
            hashCodeCombiner.Add(descriptor.TagOutputHint, StringComparer.Ordinal);

            var orderedAttributeHashCodes = descriptor.BoundAttributes
                .Select(attribute => CaseSensitiveBoundAttributeDescriptorComparer.Default.GetHashCode(attribute))
                .OrderBy(hashcode => hashcode);
            foreach (var attributeHashCode in orderedAttributeHashCodes)
            {
                hashCodeCombiner.Add(attributeHashCode);
            }

            foreach (var rule in descriptor.TagMatchingRules)
            {
                hashCodeCombiner.Add(CaseSensitiveTagMatchingRuleComparer.Default.GetHashCode(rule));
            }

            if (descriptor.AllowedChildTags != null)
            {
                foreach (var child in descriptor.AllowedChildTags.OrderBy(child => child))
                {
                    hashCodeCombiner.Add(child, StringComparer.Ordinal);
                }
            }

            return hashCodeCombiner.CombinedHash;
        }
    }
}