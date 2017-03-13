// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    /// <summary>
    /// An <see cref="IEqualityComparer{TagHelperRequiredAttributeDescriptor}"/> used to check equality between
    /// two <see cref="RequiredAttributeDescriptor"/>s.
    /// </summary>
    internal class RequiredAttributeDescriptorComparer : IEqualityComparer<RequiredAttributeDescriptor>
    {
        /// <summary>
        /// A default instance of the <see cref="RequiredAttributeDescriptor"/>.
        /// </summary>
        public static readonly RequiredAttributeDescriptorComparer Default =
            new RequiredAttributeDescriptorComparer();

        /// <summary>
        /// Initializes a new <see cref="RequiredAttributeDescriptor"/> instance.
        /// </summary>
        protected RequiredAttributeDescriptorComparer()
        {
        }

        /// <inheritdoc />
        public virtual bool Equals(
            RequiredAttributeDescriptor descriptorX,
            RequiredAttributeDescriptor descriptorY)
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
                descriptorX.NameComparison == descriptorY.NameComparison &&
                descriptorX.ValueComparison == descriptorY.ValueComparison &&
                string.Equals(descriptorX.Name, descriptorY.Name, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(descriptorX.Value, descriptorY.Value, StringComparison.Ordinal) &&
                Enumerable.SequenceEqual(descriptorX.Diagnostics, descriptorY.Diagnostics);
        }

        /// <inheritdoc />
        public virtual int GetHashCode(RequiredAttributeDescriptor descriptor)
        {
            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(descriptor.NameComparison);
            hashCodeCombiner.Add(descriptor.ValueComparison);
            hashCodeCombiner.Add(descriptor.Name, StringComparer.OrdinalIgnoreCase);
            hashCodeCombiner.Add(descriptor.Value, StringComparer.Ordinal);

            return hashCodeCombiner.CombinedHash;
        }
    }
}
