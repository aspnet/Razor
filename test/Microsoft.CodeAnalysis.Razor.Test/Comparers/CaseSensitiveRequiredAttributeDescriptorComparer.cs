// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Evolution;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;
using Microsoft.Extensions.Internal;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.Workspaces.Test.Comparers
{
    internal class CaseSensitiveRequiredAttributeDescriptorComparer : RequiredAttributeDescriptorComparer
    {
        public new static readonly CaseSensitiveRequiredAttributeDescriptorComparer Default =
            new CaseSensitiveRequiredAttributeDescriptorComparer();

        private CaseSensitiveRequiredAttributeDescriptorComparer()
            : base()
        {
        }

        public override bool Equals(RequiredAttributeDescriptor descriptorX, RequiredAttributeDescriptor descriptorY)
        {
            if (descriptorX == descriptorY)
            {
                return true;
            }

            Assert.Equal(descriptorX.Name, descriptorY.Name, StringComparer.Ordinal);
            Assert.True(base.Equals(descriptorX, descriptorY));

            return true;
        }

        public override int GetHashCode(RequiredAttributeDescriptor descriptor)
        {
            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(base.GetHashCode(descriptor));
            hashCodeCombiner.Add(descriptor.Name, StringComparer.Ordinal);

            return hashCodeCombiner.CombinedHash;
        }
    }
}