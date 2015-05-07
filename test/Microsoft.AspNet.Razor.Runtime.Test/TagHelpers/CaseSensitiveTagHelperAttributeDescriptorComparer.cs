// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.Internal.Web.Utils;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public class CaseSensitiveTagHelperAttributeDescriptorComparer : IEqualityComparer<TagHelperAttributeDescriptor>
    {
        public static readonly CaseSensitiveTagHelperAttributeDescriptorComparer Default =
            new CaseSensitiveTagHelperAttributeDescriptorComparer();

        private CaseSensitiveTagHelperAttributeDescriptorComparer()
        {
        }

        public bool Equals(TagHelperAttributeDescriptor descriptorX, TagHelperAttributeDescriptor descriptorY)
        {
            return
                // Normal comparer doesn't care about case, in tests we do. Also double-check ObjectCreationExpression
                // and both bool properties though all are inferred from TypeName.
                descriptorX.AreStringPrefixedValues == descriptorY.AreStringPrefixedValues &&
                descriptorX.IsStringProperty == descriptorY.IsStringProperty &&
                string.Equals(descriptorX.Name, descriptorY.Name, StringComparison.Ordinal) &&
                string.Equals(
                    descriptorX.ObjectCreationExpression,
                    descriptorY.ObjectCreationExpression,
                    StringComparison.Ordinal) &&
                string.Equals(descriptorX.Prefix, descriptorY.Prefix, StringComparison.Ordinal) &&
                string.Equals(descriptorX.PropertyName, descriptorY.PropertyName, StringComparison.Ordinal) &&
                string.Equals(descriptorX.TypeName, descriptorY.TypeName, StringComparison.Ordinal);
        }

        public int GetHashCode(TagHelperAttributeDescriptor descriptor)
        {
            // Rarely if ever hash TagHelperAttributeDescriptor. If we do, ignore ObjectCreationExpression and both
            // bool properties since they should not vary for a given TypeName i.e. will not change the bucket.
            return HashCodeCombiner
                .Start()
                .Add(descriptor.Name, StringComparer.Ordinal)
                .Add(descriptor.Prefix, StringComparer.Ordinal)
                .Add(descriptor.PropertyName, StringComparer.Ordinal)
                .Add(descriptor.TypeName, StringComparer.Ordinal)
                .CombinedHash;
        }
    }
}