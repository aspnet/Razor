// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public class TagHelpersExecutionContext
    {
        public TagHelpersExecutionContext()
        {
            AllAttributes = new Dictionary<string, object>(StringComparer.Ordinal);
            HTMLAttributes = new Dictionary<string, string>(StringComparer.Ordinal);
            ActiveTagHelpers = new List<TagHelper>();
        }

        public Dictionary<string, string> HTMLAttributes { get; private set; }

        public Dictionary<string, object> AllAttributes { get; private set; }

        public List<TagHelper> ActiveTagHelpers { get; private set; }

        public TagHelperOutput TagHelperOutput { get; private set; }

        public void CreateTagHelperOutput(string tagName, Dictionary<string, string> attributes)
        {
            TagHelperOutput = new TagHelperOutput(tagName, attributes);
        }
    }
}