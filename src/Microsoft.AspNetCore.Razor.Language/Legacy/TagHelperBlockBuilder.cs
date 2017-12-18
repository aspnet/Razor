// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    /// <summary>
    /// A <see cref="BlockBuilder"/> used to create <see cref="TagHelperBlock"/>s.
    /// </summary>
    internal class TagHelperBlockBuilder : BlockBuilder
    {
        /// <summary>
        /// Instantiates a new <see cref="TagHelperBlockBuilder"/> instance based on the given
        /// <paramref name="original"/>.
        /// </summary>
        /// <param name="original">The original <see cref="TagHelperBlock"/> to copy data from.</param>
        public TagHelperBlockBuilder(TagHelperBlock original)
            : base(original)
        {
            SourceStartTag = original.SourceStartTag;
            SourceEndTag = original.SourceEndTag;
            TagMode = original.TagMode;
            BindingResult = original.Binding;
            Attributes = new List<TagHelperAttributeNode>(original.Attributes);
            TagName = original.TagName;
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperBlockBuilder"/> class
        /// with the provided values.
        /// </summary>
        /// <param name="tagName">An HTML tag name.</param>
        /// <param name="tagMode">HTML syntax of the element in the Razor source.</param>
        /// <param name="start">Starting location of the <see cref="TagHelperBlock"/>.</param>
        /// <param name="attributes">Attributes of the <see cref="TagHelperBlock"/>.</param>
        /// <param name="bindingResult"></param>
        public TagHelperBlockBuilder(
            string tagName,
            TagMode tagMode,
            SourceLocation start,
            IList<TagHelperAttributeNode> attributes,
            TagHelperBinding bindingResult)
        {
            TagName = tagName;
            TagMode = tagMode;
            Start = start;
            BindingResult = bindingResult;
            Attributes = new List<TagHelperAttributeNode>(attributes);
            Type = BlockKindInternal.Tag;
            ChunkGenerator = new TagHelperChunkGenerator();
        }

        // Internal for testing
        internal TagHelperBlockBuilder(
            string tagName,
            TagMode tagMode,
            IList<TagHelperAttributeNode> attributes,
            IEnumerable<SyntaxTreeNode> children)
        {
            TagName = tagName;
            TagMode = tagMode;
            Attributes = attributes;
            Type = BlockKindInternal.Tag;
            ChunkGenerator = new TagHelperChunkGenerator();

            // Children is IList, no AddRange
            foreach (var child in children)
            {
                Children.Add(child);
            }
        }

        /// <summary>
        /// Gets or sets the unrewritten source start tag.
        /// </summary>
        /// <remarks>This is used by design time to properly format <see cref="TagHelperBlock"/>s.</remarks>
        public Block SourceStartTag { get; set; }

        /// <summary>
        /// Gets or sets the unrewritten source end tag.
        /// </summary>
        /// <remarks>This is used by design time to properly format <see cref="TagHelperBlock"/>s.</remarks>
        public Block SourceEndTag { get; set; }

        /// <summary>
        /// Gets the HTML syntax of the element in the Razor source.
        /// </summary>
        public TagMode TagMode { get; }

        /// <summary>
        /// <see cref="TagHelperDescriptor"/>s for the HTML element.
        /// </summary>
        public TagHelperBinding BindingResult { get; }

        /// <summary>
        /// The HTML attributes.
        /// </summary>
        public IList<TagHelperAttributeNode> Attributes { get; }

        /// <summary>
        /// The HTML tag name.
        /// </summary>
        public string TagName { get; set; }

        /// <summary>
        /// Constructs a new <see cref="TagHelperBlock"/>.
        /// </summary>
        /// <returns>A <see cref="TagHelperBlock"/>.</returns>
        public override Block Build()
        {
            return new TagHelperBlock(this);
        }

        /// <inheritdoc />
        /// <remarks>
        /// Sets the <see cref="TagName"/> to <c>null</c> and clears the <see cref="Attributes"/>.
        /// </remarks>
        public override void Reset()
        {
            TagName = null;

            if (Attributes != null)
            {
                Attributes.Clear();
            }

            base.Reset();
        }

        /// <summary>
        /// The starting <see cref="SourceLocation"/> of the tag helper.
        /// </summary>
        public SourceLocation Start { get; set; }
    }
}