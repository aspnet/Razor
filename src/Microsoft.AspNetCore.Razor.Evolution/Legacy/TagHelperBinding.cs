// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    internal sealed class TagHelperBinding
    {
        public IReadOnlyDictionary<TagHelperDescriptor, IEnumerable<TagMatchingRule>> _applicableDescriptorMappings;

        public TagHelperBinding(IReadOnlyDictionary<TagHelperDescriptor, IEnumerable<TagMatchingRule>> applicableDescriptorMappings)
        {
            _applicableDescriptorMappings = applicableDescriptorMappings;
            Descriptors = _applicableDescriptorMappings.Keys;
        }

        public IEnumerable<TagHelperDescriptor> Descriptors { get; }

        public IEnumerable<TagMatchingRule> GetBoundRules(TagHelperDescriptor descriptor)
        {
            return _applicableDescriptorMappings[descriptor];
        }
    }
}