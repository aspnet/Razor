// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.Extensions.Internal;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.Workspaces.Test.Comparers
{
    internal class CaseSensitiveTagMatchingRuleComparer : TagMatchingRuleComparer
    {
        public new static readonly CaseSensitiveTagMatchingRuleComparer Default =
            new CaseSensitiveTagMatchingRuleComparer();

        private CaseSensitiveTagMatchingRuleComparer()
            : base()
        {
        }

        public override bool Equals(TagMatchingRule ruleX, TagMatchingRule ruleY)
        {
            if (ruleX == ruleY)
            {
                return true;
            }

            Assert.Equal(ruleX.TagName, ruleY.TagName);
            Assert.Equal(ruleX.ParentTag, ruleY.ParentTag);
            Assert.Equal(ruleX.Attributes, ruleY.Attributes, CaseSensitiveRequiredAttributeDescriptorComparer.Default);
            Assert.True(base.Equals(ruleX, ruleY));

            return true;
        }

        public override int GetHashCode(TagMatchingRule rule)
        {
            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(base.GetHashCode(rule));
            hashCodeCombiner.Add(rule.TagName, StringComparer.Ordinal);
            hashCodeCombiner.Add(rule.ParentTag, StringComparer.Ordinal);
            var attributes = rule.Attributes.OrderBy(attribute => attribute.Name, StringComparer.Ordinal);
            foreach (var attribute in attributes)
            {
                hashCodeCombiner.Add(CaseSensitiveRequiredAttributeDescriptorComparer.Default.GetHashCode(attribute));
            }

            return hashCodeCombiner.CombinedHash;
        }
    }
}