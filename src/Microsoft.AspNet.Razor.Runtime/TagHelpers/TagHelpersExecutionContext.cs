// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Defines a class that is used to store information about a <see cref="ITagHelper"/>s execution lifetime.
    /// </summary>
    public class TagHelpersExecutionContext
    {
        /// <summary>
        /// Instantiates a new <see cref="TagHelpersExecutionContext"/>.
        /// </summary>
        /// <param name="tagName">The HTML tag name associated with the current <see cref="TagHelpersExecutionContext"/>.</param>
        public TagHelpersExecutionContext(string tagName)
        {
            AllAttributes = new Dictionary<string, object>(StringComparer.Ordinal);
            HTMLAttributes = new Dictionary<string, string>(StringComparer.Ordinal);
            TagHelpers = new List<ITagHelper>();
            TagName = tagName;
        }

        /// <summary>
        /// HTML attributes for the current execution context.
        /// </summary>
        public Dictionary<string, string> HTMLAttributes { get; private set; }

        /// <summary>
        /// <see cref="ITagHelper"/> bound attributes and HTML attributes.
        /// </summary>
        public Dictionary<string, object> AllAttributes { get; private set; }

        /// <summary>
        /// <see cref="ITagHelper"/>s that should be run for the current <see cref="TagHelpersExecutionContext"/>.
        /// </summary>
        public List<ITagHelper> TagHelpers { get; private set; }

        /// <summary>
        /// The HTML tag name for the current <see cref="TagHelpersExecutionContext"/>.
        /// </summary>
        public string TagName { get; private set; }

        /// <summary>
        /// The <see cref="TagHelperOutput"/> of the current <see cref="TagHelpersExecutionContext"/>.
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
        /// Tracks the HTML attribute.
        /// </summary>
        /// <param name="name">The HTML attribute name.</param>
        /// <param name="value">The HTML attribute value.</param>
        public void AddHtmlAttribute(string name, string value)
        {
            HTMLAttributes.Add(name, value);
            AllAttributes.Add(name, value);
        }

        /// <summary>
        /// Tracks the HTML attribute so it can be used by a <see cref="TagHelperContext.AllAttributes"/>.
        /// </summary>
        /// <param name="name">The HTML attribute name.</param>
        /// <param name="value">The attribute value.</param>
        public void AddTagHelperAttribute(string name, object value)
        {
            AllAttributes.Add(name, value);
        }
    }
}