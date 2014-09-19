// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Razor.Test.TagHelpers
{
    internal class TestTagHelperDescriptorResolver : ITagHelperDescriptorResolver
    {
        public IEnumerable<TagHelperDescriptor> Resolve(string lookupText)
        {
            return Enumerable.Empty<TagHelperDescriptor>();
        }
    }
}