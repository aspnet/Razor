// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Used to override a <see cref="ITagHelper"/> properties HTML attribute name target.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public sealed class HtmlAttributeNameAttribute : Attribute
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="HtmlAttributeNameAttribute"/> class.
        /// </summary>
        /// <param name="name">HTML attribute name for the <see cref="ITagHelper"/>'s property to target.</param>
        public HtmlAttributeNameAttribute([NotNull] string name)
        {
            Name = name;
        }

        /// <summary>
        /// HTML attribute name for the <see cref="ITagHelper"/>'s property to target.
        /// </summary>
        public string Name { get; private set; }
    }
}