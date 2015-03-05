// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public class TagHelperAttribute<TValue> where TValue : class
    {
        private string _key;

        public TagHelperAttribute()
        {
        }

        public TagHelperAttribute([NotNull] string key)
            : this(key, value: null)
        {
        }

        public TagHelperAttribute([NotNull] string key, TValue value)
        {
            Key = key;
            Value = value;
        }

        public string Key
        {
            get
            {
                return _key;
            }
            set
            {
                // Keys aren't allowed to be null.
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(Key));
                }

                _key = value;
            }
        }

        public TValue Value { get; set; }

        public bool Minimized { get; set; }
    }
}