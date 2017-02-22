﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public class TestRazorProject : RazorProject
    {
        private readonly Dictionary<string, RazorProjectItem> _lookup;

        public TestRazorProject(IList<RazorProjectItem> items)
        {
            _lookup = items.ToDictionary(item => item.Path);
        }

        public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath)
        {
            throw new NotImplementedException();
        }

        public override RazorProjectItem GetItem(string path)
        {
            if (!_lookup.TryGetValue(path, out var value))
            {
                value = new NotFoundProjectItem("", path);
            }

            return value;
        }
    }
}
