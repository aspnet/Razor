// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.Internal.Web.Utils;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers.Test
{
    public class TagHelperAttributeComparer<TAttributeValue> : IEqualityComparer<TagHelperAttribute<TAttributeValue>>
        where TAttributeValue : class
    {
        public readonly static TagHelperAttributeComparer<TAttributeValue> Default =
            new TagHelperAttributeComparer<TAttributeValue>();

        private TagHelperAttributeComparer()
        {
        }

        public bool Equals(TagHelperAttribute<TAttributeValue> attributeX, TagHelperAttribute<TAttributeValue> attributeY)
        {
            return 
                string.Equals(attributeX.Key, attributeY.Key, StringComparison.OrdinalIgnoreCase) &&
                Equals(attributeX.Value, attributeY.Value);
        }

        public int GetHashCode(TagHelperAttribute<TAttributeValue> attribute)
        {
            return HashCodeCombiner
                .Start()
                .Add(attribute.Key, StringComparer.OrdinalIgnoreCase)
                .Add(attribute.Value)
                .CombinedHash;
        }
    }
}