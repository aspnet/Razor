﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net;
using System.Text;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Class used to represent the output of an <see cref="ITagHelper"/>.
    /// </summary>
    public class TagHelperOutput
    {
        private string _content;
        private bool _contentSet;

        // Internal for testing
        internal TagHelperOutput(string tagName)
        {
            TagName = tagName;
            Attributes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        // Internal for testing
        internal TagHelperOutput(string tagName, [NotNull] IDictionary<string, string> attributes)
            : this(tagName, attributes, string.Empty)
        {
        }

        /// <summary>
        /// Instantiates a new instance of <see cref="TagHelperOutput"/>.
        /// </summary>
        /// <param name="tagName">The HTML element's tag name.</param>
        /// <param name="attributes">The HTML attributes.</param>
        public TagHelperOutput(string tagName, [NotNull] IDictionary<string, string> attributes)
        {
            TagName = tagName;
            Attributes = new Dictionary<string, string>(attributes, StringComparer.OrdinalIgnoreCase);
            PreContent = string.Empty;
            _content = string.Empty;
            PostContent = string.Empty;
        }

        /// <summary>
        /// The HTML element's tag name.
        /// </summary>
        /// <remarks>
        /// A whitespace or <c>null</c> value results in no start or end tag being rendered.
        /// </remarks>
        public string TagName { get; set; }

        /// <summary>
        /// The HTML element's pre content.
        /// </summary>
        public string PreContent { get; set; }

        /// <summary>
        /// The HTML element's content.
        /// </summary>
        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _contentSet = true;
                _content = value;
            }
        }

        /// <summary>
        /// The HTML element's post content.
        /// </summary>
        public string PostContent { get; set; }

        /// <summary>
        /// <c>true</c> if <see cref="Content"/> has been set, <c>false</c> otherwise.
        /// </summary>
        public bool ContentSet { get { return _contentSet; } }

        /// <summary>
        /// Indicates whether or not the tag is self closing.
        /// </summary>
        public bool SelfClosing { get; set; }

        /// <summary>
        /// The HTML element's attributes.
        /// </summary>
        public IDictionary<string, string> Attributes { get; }

        /// <summary>
        /// Generates the <see cref="TagHelperOutput"/>'s start tag.
        /// </summary>
        /// <returns><c>string.Empty</c> if <see cref="TagName"/> is <c>string.Empty</c> or whitespace. Otherwise, the
        /// <see cref="string"/> representation of the <see cref="TagHelperOutput"/>'s start tag.</returns>
        public string GenerateStartTag()
        {
            // Only render a start tag if the tag name is not whitespace
            if (string.IsNullOrWhiteSpace(TagName))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.Append('<')
              .Append(TagName);

            foreach (var attribute in Attributes)
            {
                var value = WebUtility.HtmlEncode(attribute.Value);
                sb.Append(' ')
                  .Append(attribute.Key)
                  .Append("=\"")
                  .Append(value)
                  .Append('"');
            }

            if (SelfClosing)
            {
                sb.Append(" /");
            }

            sb.Append('>');

            return sb.ToString();
        }

        /// <summary>
        /// Generates the <see cref="TagHelperOutput"/>'s <see cref="PreContent"/>.
        /// </summary>
        /// <returns><c>string.Empty</c> if <see cref="SelfClosing"/> is <c>true</c>. <see cref="PreContent"/> otherwise.
        /// </returns>
        public string GeneratePreContent()
        {
            if (SelfClosing)
            {
                return string.Empty;
            }

            return PreContent;
        }

        /// <summary>
        /// Generates the <see cref="TagHelperOutput"/>'s body.
        /// </summary>
        /// <returns><c>string.Empty</c> if <see cref="SelfClosing"/> is <c>true</c>. <see cref="Content"/> otherwise.
        /// </returns>
        public string GenerateContent()
        {
            if (SelfClosing)
            {
                return string.Empty;
            }

            return Content;
        }

        /// <summary>
        /// Generates the <see cref="TagHelperOutput"/>'s <see cref="PostContent"/>.
        /// </summary>
        /// <returns><c>string.Empty</c> if <see cref="SelfClosing"/> is <c>true</c>. <see cref="PostContent"/> otherwise.
        /// </returns>
        public string GeneratePostContent()
        {
            if (SelfClosing)
            {
                return string.Empty;
            }

            return PostContent;
        }

        /// <summary>
        /// Generates the <see cref="TagHelperOutput"/>'s end tag.
        /// </summary>
        /// <returns><c>string.Empty</c> if <see cref="TagName"/> is <c>string.Empty</c> or whitespace. Otherwise, the
        /// <see cref="string"/> representation of the <see cref="TagHelperOutput"/>'s end tag.</returns>
        public string GenerateEndTag()
        {
            if (SelfClosing || string.IsNullOrWhiteSpace(TagName))
            {
                return string.Empty;
            }

            return string.Format(CultureInfo.InvariantCulture, "</{0}>", TagName);
        }

        /// <summary>
        /// Changes the output of the <see cref="TagHelperOutput"/> to generate nothing.
        /// </summary>
        /// <remarks>
        /// Sets <see cref="TagName"/>, <see cref="PreContent"/>, <see cref="Content"/> and <see cref="PostContent"/> 
        /// to <c>null</c> to supress output.
        /// </remarks>
        public void SupressOutput()
        {
            TagName = null;
            PreContent = null;
            Content = null;
            PostContent = null;
        }
    }
}