// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.Internal.Web.Utils;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// A class describing a tag helper.
    /// </summary>
    public class TagHelperDescriptor
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperDescriptor"/> class.
        /// </summary>
        /// <param name="tagName">The tag name that the tag helper targets. '*' indicates a catch-all
        /// <see cref="TagHelperDescriptor"/> which applies to every HTML tag.</param>
        /// <param name="tagHelperName">The code class that is used to render the tag helper. Corresponds to
        /// the tag helpers <see cref="System.Type.FullName"/>.</param>
        /// <param name="contentBehavior">The <see cref="Microsoft.AspNet.Razor.ContentBehavior"/>
        /// of the tag helper.</param>
        public TagHelperDescriptor(string tagName,
                                   string tagHelperName,
                                   ContentBehavior contentBehavior)
        {
            TagName = tagName;
            TagHelperName = tagHelperName;
            ContentBehavior = contentBehavior;
            Attributes = new List<TagHelperAttributeInfo>();
        }

        /// <summary>
        /// The tag name that the tag helper should target.
        /// </summary>
        public string TagName { get; private set; }

        /// <summary>
        /// The code class that is used to render the tag helper.
        /// </summary>
        public string TagHelperName { get; private set; }

        /// <summary>
        /// The content <see cref="Microsoft.AspNet.Razor.ContentBehavior"/> of the tag helper.
        /// </summary>
        public ContentBehavior ContentBehavior { get; private set; }

        /// <summary>
        /// The list of attributes that the tag helper expects.
        /// </summary>
        public virtual List<TagHelperAttributeInfo> Attributes { get; private set; }

        /// <inheritdoc />
        /// <remarks>
        /// Builds a hash code combination of <see cref="TagName"/>, <see cref="TagHelperName"/> and 
        /// <see cref="ContentBehavior"/>.
        /// </remarks>
        public override int GetHashCode()
        {
            return HashCodeCombiner.Start()
                                   .Add(TagName)
                                   .Add(TagHelperName)
                                   .Add(ContentBehavior)
                                   .CombinedHash;
        }

        /// <inheritdoc />
        /// <remarks>
        /// Compares the <see cref="TagName"/>, <see cref="TagHelperName"/> and <see cref="ContentBehavior"/>
        /// of the current <see cref="TagHelperDescriptor"/> to the given <paramref name="obj"/>.
        /// </remarks>
        public override bool Equals(object obj)
        {
            var other = obj as TagHelperDescriptor;

            return other != null &&
                   other.TagHelperName == TagHelperName &&
                   other.TagName == TagName &&
                   other.ContentBehavior == ContentBehavior;
        }
    }
}