﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Framework.Internal;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Internal
{
    internal class TagHelperAttributeDesignTimeDescriptorComparer :
        IEqualityComparer<TagHelperAttributeDesignTimeDescriptor>
    {
        public static readonly TagHelperAttributeDesignTimeDescriptorComparer Default =
            new TagHelperAttributeDesignTimeDescriptorComparer();

        private TagHelperAttributeDesignTimeDescriptorComparer()
        {
        }

        public bool Equals(
            TagHelperAttributeDesignTimeDescriptor descriptorX,
            TagHelperAttributeDesignTimeDescriptor descriptorY)
        {
            if (descriptorX == descriptorY)
            {
                return true;
            }

            Assert.NotNull(descriptorX);
            Assert.NotNull(descriptorY);
            Assert.Equal(descriptorX.Summary, descriptorY.Summary, StringComparer.Ordinal);
            Assert.Equal(descriptorX.Remarks, descriptorY.Remarks, StringComparer.Ordinal);

            return true;
        }

        public int GetHashCode(TagHelperAttributeDesignTimeDescriptor descriptor)
        {
            return HashCodeCombiner
                .Start()
                .Add(descriptor.Summary, StringComparer.Ordinal)
                .Add(descriptor.Remarks, StringComparer.Ordinal)
                .CombinedHash;
        }
    }
}
