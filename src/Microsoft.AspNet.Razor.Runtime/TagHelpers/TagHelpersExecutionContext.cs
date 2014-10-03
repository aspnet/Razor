// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Class used to store information about a <see cref="ITagHelper"/>'s execution lifetime.
    /// </summary>
    public class TagHelpersExecutionContext
    {
        /// <summary>
        /// Instantiates a new <see cref="TagHelpersExecutionContext"/>.
        /// </summary>
        /// <param name="tagName">The HTML tag name.</param>
        public TagHelpersExecutionContext(string tagName)
        {
            AllAttributes = new Dictionary<string, object>(StringComparer.Ordinal);
            HTMLAttributes = new Dictionary<string, string>(StringComparer.Ordinal);
            TagHelpers = new List<ITagHelper>();
            TagName = tagName;
        }

        /// <summary>
        /// HTML attributes.
        /// </summary>
        public Dictionary<string, string> HTMLAttributes { get; private set; }

        /// <summary>
        /// <see cref="ITagHelper"/> bound attributes and HTML attributes.
        /// </summary>
        public Dictionary<string, object> AllAttributes { get; private set; }

        /// <summary>
        /// <see cref="ITagHelper"/>s that should be run.
        /// </summary>
        public List<ITagHelper> TagHelpers { get; private set; }

        /// <summary>
        /// The HTML tag name.
        /// </summary>
        public string TagName { get; private set; }

        /// <summary>
        /// The <see cref="TagHelperOutput"/>.
        /// </summary>
        public TagHelperOutput Output { get; set; }

        /// <summary>
        /// Tracks the given <paramref name="tagHelper"/>.
        /// </summary>
        /// <param name="tagHelper">The tag helper to track.</param>
        public void Add(ITagHelper tagHelper)
        {
            TagHelpers.Add(tagHelper);
        }

        /// <summary>
        /// Tracks the HTML attribute in <see cref="AllAttributes"/> and <see cref="HTMLAttributes"/>.
        /// </summary>
        /// <param name="name">The HTML attribute name.</param>
        /// <param name="value">The HTML attribute value.</param>
        public void AddHtmlAttribute(string name, string value)
        {
            HTMLAttributes.Add(name, value);
            AllAttributes.Add(name, value);
        }

        /// <summary>
        /// Tracks the <see cref="ITagHelper"/> bound attribute in <see cref="AllAttributes"/>.
        /// </summary>
        /// <param name="name">The HTML attribute name.</param>
        /// <param name="value">The attribute value.</param>
        public void AddTagHelperAttribute(string name, object value)
        {
            AllAttributes.Add(name, value);
        }
    }
}