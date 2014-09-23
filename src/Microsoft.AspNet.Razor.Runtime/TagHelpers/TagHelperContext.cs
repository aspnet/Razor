// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Summary description for TagHelperContext
    /// </summary>
    public class TagHelperContext
    {
        public TagHelperContext(Dictionary<string, object> allAttributes)
        {
            // We don't want to use the existing attribute list, must not affect other tag helpers.
            AllAttributes = new Dictionary<string, object>(allAttributes);
        }

        public Dictionary<string, object> AllAttributes { get; private set; }
    }
}