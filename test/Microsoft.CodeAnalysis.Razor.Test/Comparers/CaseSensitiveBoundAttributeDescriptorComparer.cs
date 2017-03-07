// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.Extensions.Internal;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.Workspaces.Test.Comparers
{
    internal class CaseSensitiveBoundAttributeDescriptorComparer : BoundAttributeDescriptorComparer
    {
        public new static readonly CaseSensitiveBoundAttributeDescriptorComparer Default =
            new CaseSensitiveBoundAttributeDescriptorComparer();

        private CaseSensitiveBoundAttributeDescriptorComparer()
        {
        }

        public override bool Equals(BoundAttributeDescriptor descriptorX, BoundAttributeDescriptor descriptorY)
        {
            if (descriptorX == descriptorY)
            {
                return true;
            }

            Assert.Equal(descriptorX.Name, descriptorY.Name);
            Assert.Equal(descriptorX.IndexerNamePrefix, descriptorY.IndexerNamePrefix);
            Assert.True(base.Equals(descriptorX, descriptorY));

            return true;
        }

        public override int GetHashCode(BoundAttributeDescriptor descriptor)
        {
            if (descriptor == null)
            {
                throw new ArgumentNullException(nameof(descriptor));
            }

            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(base.GetHashCode(descriptor));
            hashCodeCombiner.Add(descriptor.Name, StringComparer.Ordinal);
            hashCodeCombiner.Add(descriptor.IndexerNamePrefix, StringComparer.Ordinal);

            return hashCodeCombiner.CombinedHash;
        }
    }
}