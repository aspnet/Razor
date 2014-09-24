﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Defines a class used to represent the output of a tag helper.
    /// </summary>
    public class TagHelperOutput
    {
        private string _content;
        private string _tagName;

        // Internal for testing
        internal TagHelperOutput(string tagName)
            : this(tagName, attributes: new Dictionary<string, string>())
        {
        }

        /// <summary>
        /// Instantiates a new instance of <see cref="TagHelperOutput"/>.
        /// </summary>
        /// <param name="tagName">The HTML elements tag name.</param>
        /// <param name="attributes">The HTML attributes.</param>
        public TagHelperOutput(string tagName, Dictionary<string, string> attributes)
        {
            TagName = tagName;
            Content = string.Empty;
            Attributes = new Dictionary<string, string>(attributes);
        }

        /// <summary>
        /// The HTML elements tag name.
        /// </summary>
        public string TagName
        {
            get
            {
                return _tagName;
            }
            set
            {
                _tagName = value ?? string.Empty;
            }
        }

        /// <summary>
        /// The HTML elements content.
        /// </summary>
        public string Content
        {
            get
            {
                return _content;
            }
            set
            {
                _content = value ?? string.Empty;
            }
        }

        /// <summary>
        /// Indicates whether or not the tag is self closing.
        /// </summary>
        public bool SelfClosing { get; set; }

        /// <summary>
        /// The HTML elements attributes.
        /// </summary>
        public Dictionary<string, string> Attributes { get; private set; }

        /// <summary>
        /// Generates the <see cref="TagHelperOutput"/>'s start tag.
        /// </summary>
        /// <returns>The string representation of the <see cref="TagHelperOutput"/>s start tag.</returns>
        public string GenerateTagStart()
        {
            var sb = new StringBuilder();

            // Only render a start tag if the tag name is not whitespace
            if (!string.IsNullOrWhiteSpace(TagName))
            {
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
            }

            return sb.ToString();
        }

        /// <summary>
        /// Generates the <see cref="TagHelperOutput"/>'s body.
        /// </summary>
        /// <returns>The string representation of the <see cref="TagHelperOutput"/>s Inner HTML.</returns>
        public string GenerateTagContent()
        {
            if (SelfClosing)
            {
                return string.Empty;
            }

            return Content;
        }

        /// <summary>
        /// Generates the <see cref="TagHelperOutput"/>'s end tag.
        /// </summary>
        /// <returns>The string representation of the <see cref="TagHelperOutput"/>s end tag.</returns>
        public string GenerateTagEnd()
        {
            if (SelfClosing && !string.IsNullOrWhiteSpace(TagName))
            {
                return string.Empty;
            }

            var sb = new StringBuilder();

            sb.Append("</")
              .Append(TagName)
              .Append('>');

            return sb.ToString();
        }

        /// <summary>
        /// Creates a <see cref="TextWriter"/> to write directly to the <see cref="TagHelperOutput"/>s 
        /// <see cref="Content"/>.
        /// </summary>
        /// <returns>A <see cref="TextWriter"/> that writes directly to the <see cref="TagHelperOutput"/>s
        /// <see cref="Content"/>.</returns>
        public TextWriter GetContentWriter()
        {
            return new TagHelperOutputWriter(this);
        }

        private class TagHelperOutputWriter : TextWriter
        {
            private TagHelperOutput _tagHelperOutput;

            public TagHelperOutputWriter(TagHelperOutput tagHelperOutput)
            {
                _tagHelperOutput = tagHelperOutput;
            }

            public override void Write(char value)
            {
                _tagHelperOutput.Content += value;
            }

            // This is an optimization so we aren't always calling into Write(char) and causing unnecessary appends.
            public override void Write(string value)
            {
                _tagHelperOutput.Content += value;
            }

            public override Encoding Encoding
            {
                get
                {
                    return Encoding.UTF8;
                }
            }
        }
    }
}