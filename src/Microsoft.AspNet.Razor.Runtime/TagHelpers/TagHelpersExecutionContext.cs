// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public class TagHelpersExecutionContext<TTagHelper>
    {
        public TagHelpersExecutionContext()
        {
            AllAttributes = new Dictionary<string, object>(StringComparer.Ordinal);
            HTMLAttributes = new Dictionary<string, string>(StringComparer.Ordinal);
            ActiveTagHelpers = new List<TTagHelper>();
        }

        public Dictionary<string, string> HTMLAttributes { get; private set; }

        public Dictionary<string, object> AllAttributes { get; private set; }

        public List<TTagHelper> ActiveTagHelpers { get; private set; }
    }
}