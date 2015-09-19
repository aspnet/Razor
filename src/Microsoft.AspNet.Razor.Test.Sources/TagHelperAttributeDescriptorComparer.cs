// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Framework.Internal;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Internal
{
    internal class TagHelperAttributeDescriptorComparer : IEqualityComparer<TagHelperAttributeDescriptor>
    {
        public static readonly TagHelperAttributeDescriptorComparer Default =
            new TagHelperAttributeDescriptorComparer();

        private TagHelperAttributeDescriptorComparer()
        {
        }

        public bool Equals(TagHelperAttributeDescriptor descriptorX, TagHelperAttributeDescriptor descriptorY)
        {
            if (descriptorX == descriptorY)
            {
                return true;
            }

            Assert.NotNull(descriptorX);
            Assert.NotNull(descriptorY);
            Assert.Equal(descriptorX.IsIndexer, descriptorY.IsIndexer);
            Assert.Equal(descriptorX.Name, descriptorY.Name, StringComparer.Ordinal);
            Assert.Equal(descriptorX.PropertyName, descriptorY.PropertyName, StringComparer.Ordinal);
            Assert.Equal(descriptorX.TypeName, descriptorY.TypeName, StringComparer.Ordinal);
            Assert.Equal(descriptorX.IsStringProperty, descriptorY.IsStringProperty);

            return TagHelperAttributeDesignTimeDescriptorComparer.Default.Equals(
                    descriptorX.DesignTimeDescriptor,
                    descriptorY.DesignTimeDescriptor);
        }

        public int GetHashCode(TagHelperAttributeDescriptor descriptor)
        {
            return HashCodeCombiner.Start()
                .Add(descriptor.IsIndexer)
                .Add(descriptor.Name, StringComparer.Ordinal)
                .Add(descriptor.PropertyName, StringComparer.Ordinal)
                .Add(descriptor.TypeName, StringComparer.Ordinal)
                .Add(descriptor.IsStringProperty)
                .Add(TagHelperAttributeDesignTimeDescriptorComparer.Default.GetHashCode(
                    descriptor.DesignTimeDescriptor))
                .CombinedHash;
        }
    }
}