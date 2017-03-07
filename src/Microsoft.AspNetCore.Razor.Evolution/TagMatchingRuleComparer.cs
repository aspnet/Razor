// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    internal class TagMatchingRuleComparer : IEqualityComparer<TagMatchingRule>
    {
        public static readonly TagMatchingRuleComparer Default = new TagMatchingRuleComparer();

        protected TagMatchingRuleComparer()
        {
        }

        public virtual bool Equals(TagMatchingRule ruleX, TagMatchingRule ruleY)
        {
            if (ruleX == ruleY)
            {
                return true;
            }

            return ruleX != null &&
                string.Equals(ruleX.TagName, ruleY.TagName, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(ruleX.ParentTag, ruleY.ParentTag, StringComparison.OrdinalIgnoreCase) &&
                ruleX.TagStructure == ruleY.TagStructure &&
                Enumerable.SequenceEqual(ruleX.Attributes, ruleY.Attributes, RequiredAttributeDescriptorComparer.Default) &&
                Enumerable.SequenceEqual(ruleX.Diagnostics, ruleY.Diagnostics);
        }

        public virtual int GetHashCode(TagMatchingRule rule)
        {
            if (rule == null)
            {
                throw new ArgumentNullException(nameof(rule));
            }

            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(rule.TagName, StringComparer.OrdinalIgnoreCase);
            hashCodeCombiner.Add(rule.ParentTag, StringComparer.OrdinalIgnoreCase);
            hashCodeCombiner.Add(rule.TagStructure);

            var attributes = rule.Attributes.OrderBy(attribute => attribute.Name, StringComparer.OrdinalIgnoreCase);
            foreach (var attribute in attributes)
            {
                hashCodeCombiner.Add(RequiredAttributeDescriptorComparer.Default.GetHashCode(attribute));
            }

            return hashCodeCombiner.CombinedHash;
        }
    }
}