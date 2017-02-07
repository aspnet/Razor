// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public class CorrelationRequirement
    {
        private string _tagName;
        private IEnumerable<TagHelperRequiredAttributeDescriptor> _attributes =
            Enumerable.Empty<TagHelperRequiredAttributeDescriptor>();

        public CorrelationRequirement()
        {
        }

        public CorrelationRequirement(CorrelationRequirement requirement)
        {
            TagName = requirement.TagName;
            Attributes = requirement.Attributes;
            Parent = requirement.Parent;
            TagStructure = requirement.TagStructure;
        }

        /// <summary>
        /// The tag name that the tag helper should target.
        /// </summary>
        public string TagName
        {
            get
            {
                return _tagName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _tagName = value;
            }
        }

        /// <summary>
        /// The list of required attribute names the tag helper expects to target an element.
        /// </summary>
        /// <remarks>
        /// <c>*</c> at the end of an attribute name acts as a prefix match.
        /// </remarks>
        public IEnumerable<TagHelperRequiredAttributeDescriptor> Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _attributes = value;
            }
        }

        /// <summary>
        /// Get the name of the HTML element required as the immediate parent.
        /// </summary>
        /// <remarks><c>null</c> indicates no restriction on parent tag.</remarks>
        public string Parent { get; set; }

        /// <summary>
        /// The expected tag structure.
        /// </summary>
        /// <remarks>
        /// If <see cref="TagStructure.Unspecified"/> and no other tag helpers applying to the same element specify
        /// their <see cref="TagStructure"/> the <see cref="TagStructure.NormalOrSelfClosing"/> behavior is used:
        /// <para>
        /// <code>
        /// &lt;my-tag-helper&gt;&lt;/my-tag-helper&gt;
        /// &lt;!-- OR --&gt;
        /// &lt;my-tag-helper /&gt;
        /// </code>
        /// Otherwise, if another tag helper applying to the same element does specify their behavior, that behavior
        /// is used.
        /// </para>
        /// <para>
        /// If <see cref="TagStructure.WithoutEndTag"/> HTML elements can be written in the following formats:
        /// <code>
        /// &lt;my-tag-helper&gt;
        /// &lt;!-- OR --&gt;
        /// &lt;my-tag-helper /&gt;
        /// </code>
        /// </para>
        /// </remarks>
        public TagStructure TagStructure { get; set; }
    }
}
