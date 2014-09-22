// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Used to override tag helpers default HTML tag name target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public sealed class TagNameAttribute : Attribute
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="TagNameAttribute"/> class.
        /// </summary>
        /// <param name="tag">The HTML tag name for the tag helper to target.</param>
        public TagNameAttribute([NotNull] string tag)
        {
            Tags = new[] { tag };
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagNameAttribute"/> class.
        /// </summary>
        /// <param name="tag">The HTML tag name for the tag helper to target.</param>
        /// <param name="additionalTags">Additional HTML tag names for the tag helper to target.</param>
        public TagNameAttribute([NotNull] string tag, [NotNull] params string[] additionalTags)
            : this(tag)
        {
            Tags = Tags.Concat(additionalTags);
        }

        /// <summary>
        /// An <see cref="IEnumerable{string}"/> of HTML tag names for the tag helper to target.
        /// </summary>
        public IEnumerable<string> Tags { get; private set; }
    }
}