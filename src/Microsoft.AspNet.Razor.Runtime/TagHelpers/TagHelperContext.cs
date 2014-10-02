// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Contains information related to the execution of <see cref="ITagHelper"/>s.
    /// </summary>
    public class TagHelperContext
    {
        /// <summary>
        /// Instantiates a new <see cref="TagHelperContext"/>.
        /// </summary>
        /// <param name="allAttributes">Every attribute associated with the current HTML element.</param>
        public TagHelperContext(Dictionary<string, object> allAttributes)
        {
            // We don't want to use the existing attribute list, must not affect other tag helpers.
            AllAttributes = new Dictionary<string, object>(allAttributes);
        }

        /// <summary>
        /// Every attribute associated with the current HTML element.
        /// </summary>
        public Dictionary<string, object> AllAttributes { get; private set; }
    }
}