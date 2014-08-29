// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;

namespace Microsoft.AspNet.Razor.Parser.TagHelpers
{
    /// <summary>
    /// A <see cref="Block"/> that reprents a "special" HTML element.
    /// </summary>
    [DebuggerDisplay("'{TagName}' Tag Helper Block")]
    public class TagHelperBlock : Block
    {
        /// <summary>
        /// Instantiates a new instance of a <see cref="TagHelperBlock"/>.
        /// </summary>
        /// <param name="source">A <see cref="TagHelperBlockBuilder"/> used to construct a valid
        /// <see cref="TagHelperBlock"/>.</param>
        public TagHelperBlock(TagHelperBlockBuilder source)
            : base(source.Type.Value, source.Children, source.CodeGenerator)
        {
            TagName = source.TagName;
            Attributes = new Dictionary<string, SyntaxTreeNode>(source.Attributes);

            source.Reset();

            foreach (SyntaxTreeNode node in Children)
            {
                node.Parent = this;
            }

            foreach (var attributeChildren in Attributes.Values)
            {
                attributeChildren.Parent = this;
            }
        }

        /// <summary>
        /// The HTML tag name.
        /// </summary>
        public string TagName { get; protected set; }

        /// <summary>
        /// The HTML attributes.
        /// </summary>
        public IDictionary<string, SyntaxTreeNode> Attributes { get; protected set; }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(CultureInfo.CurrentCulture,
                                 "{0} Tag Helper Block at {1}::{2} (Gen:{3})",
                                 Type, Start, Length, CodeGenerator);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Compares the <see cref="TagName"/> and <see cref="Attributes"/> of the current <see cref="TagHelperBlock"/>
        /// to the given <paramref name="obj"/>.
        /// </remarks>
        public override bool Equals(object obj)
        {
            var other = obj as TagHelperBlock;

            return obj != null &&
                   TagName == other.TagName &&
                   Attributes.SequenceEqual(other.Attributes) &&
                   base.Equals(obj);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}