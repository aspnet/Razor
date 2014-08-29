// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;

namespace Microsoft.AspNet.Razor.Parser.TagHelpers
{
    /// <summary>
    /// A <see cref="TagHelperBlockBuilder"/> used to create <see cref="TagHelperBlock"/>'s.
    /// </summary>
    public class TagHelperBlockBuilder : BlockBuilder
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperBlockBuilder"/> class
        /// with the provided <paramref name="tagName"/> and derives its <see cref="Attributes"/>
        /// and <see cref="BlockBuilder.Type"/>from the <paramref name="startTag"/>.
        /// </summary>
        /// <param name="tagName">An HTML tag name.</param>
        /// <param name="startTag">The <see cref="Block"/> that contains all information about the start
        /// of the HTML tag.</param>
        public TagHelperBlockBuilder(string tagName, Block startTag)
        {
            TagName = tagName;
            CodeGenerator = new TagHelperCodeGenerator();
            Type = startTag.Type;
            Attributes = GetTagAttributes(startTag);
        }

        // Internal for testing
        internal TagHelperBlockBuilder(string tagName,
                                       IDictionary<string, SyntaxTreeNode> attributes,
                                       IEnumerable<SyntaxTreeNode> children)
        {
            TagName = tagName;
            Attributes = attributes;
            Type = BlockType.Tag;
            CodeGenerator = new TagHelperCodeGenerator();

            // Children is IList, no AddRange
            foreach (var child in children)
            {
                Children.Add(child);
            }
        }

        /// <summary>
        /// The HTML tag name.
        /// </summary>
        public string TagName { get; private set; }

        /// <summary>
        /// The HTML attributes.
        /// </summary>
        public IDictionary<string, SyntaxTreeNode> Attributes { get; private set; }

        /// <summary>
        /// Constructs a new <see cref="TagHelperBlock"/>.
        /// </summary>
        /// <returns>A <see cref="TagHelperBlock"/>.</returns>
        public override Block Build()
        {
            return new TagHelperBlock(this);
        }

        /// <summary>
        /// Sets the <see cref="TagName"/> to <c>null</c> and clears the <see cref="Attributes"/>.
        /// </summary>
        /// <inheritdoc />
        public override void Reset()
        {
            TagName = null;
            Attributes = new Dictionary<string, SyntaxTreeNode>(StringComparer.OrdinalIgnoreCase);

            base.Reset();
        }

        private static IDictionary<string, SyntaxTreeNode> GetTagAttributes(Block tagBlock)
        {
            var attributes = new Dictionary<string, SyntaxTreeNode>(StringComparer.OrdinalIgnoreCase);
            var tagBlockChildCount = tagBlock.Children.Count();

            if (tagBlockChildCount == 1)
            {
                // There's only the tag, therefore no attributes, return the empty dictionary.

                return attributes;
            }

            var malformedTag = !(tagBlock.Children.Last() as Span).Content.EndsWith(">");
            // If the tag is malformed that means we need to remove 1 extra item to account for end part of the tag.
            var takeOffset = (malformedTag ? 1 : 2);
            // We skip the first child "<" and take everything up to the "ending" portion of the tag ">" or "/>".
            var attributeChildren = tagBlock.Children.Skip(1).Take(tagBlockChildCount - takeOffset);

            foreach (var child in attributeChildren)
            {
                KeyValuePair<string, SyntaxTreeNode> attribute;

                if (child.IsBlock)
                {
                    attribute = ParseBlock(child as Block);
                }
                else
                {
                    attribute = ParseSpan(child as Span);
                }

                attributes.Add(attribute.Key, attribute.Value);
            }

            return attributes;
        }

        private static Block RebuildCodeGenerators(Block block)
        {
            var builder = new BlockBuilder(block);

            bool isDynamic = builder.CodeGenerator is DynamicAttributeBlockCodeGenerator;

            // We don't want any attribute specific logic here, null out the block code generator.
            if (isDynamic || builder.CodeGenerator is AttributeBlockCodeGenerator)
            {
                builder.CodeGenerator = BlockCodeGenerator.Null;
            }

            for (var i = 0; i < builder.Children.Count; i++)
            {
                var child = builder.Children[i];

                if (child.IsBlock)
                {
                    // The child is a block, iterate down into the block to rebuild its children
                    builder.Children[i] = RebuildCodeGenerators(child as Block);
                }
                else
                {
                    var childSpan = child as Span;
                    ISpanCodeGenerator newCodeGenerator = null;

                    if (childSpan.CodeGenerator is LiteralAttributeCodeGenerator)
                    {
                        var generator = (childSpan.CodeGenerator as LiteralAttributeCodeGenerator);

                        newCodeGenerator = generator.ValueGenerator?.Value ?? new MarkupCodeGenerator();
                    }
                    else if (isDynamic && childSpan.CodeGenerator == SpanCodeGenerator.Null)
                    {
                        // Usually the dynamic code generator handles rendering the null code generators underneath
                        // it but since it doesn't make sense in terms of tag helpers we need to change null code 
                        // generators to markup code generators.

                        newCodeGenerator = new MarkupCodeGenerator();
                    }

                    // If we have a new code generator we'll need to re-build the child
                    if (newCodeGenerator != null)
                    {
                        var childSpanBuilder = new SpanBuilder(childSpan)
                        {
                            CodeGenerator = newCodeGenerator
                        };

                        builder.Children[i] = childSpanBuilder.Build();
                    }
                }
            }

            return builder.Build();
        }

        private static KeyValuePair<string, SyntaxTreeNode> ParseSpan(Span span)
        {
            var name = span.Symbols.FirstHtmlSymbolAs(HtmlSymbolType.Text)?.Content ?? string.Empty;
            var valueStartIndex = 0;

            // Iterate down through the symbols to find the name and the start of the value.
            foreach (var symbol in (IEnumerable<HtmlSymbol>)span.Symbols)
            {
                if (name == null && symbol.Type == HtmlSymbolType.Text)
                {
                    name = symbol.Content;
                }
                else if (IsAttributeValueStart(symbol))
                {
                    break;
                }

                valueStartIndex++;
            }

            // Get all symbols after the "value start", aka: class="foo" value start would be at the '|'
            // class="|foo"
            var valueSymbols = span.Symbols.Skip(valueStartIndex + 1);
            valueSymbols = valueSymbols.Take(valueSymbols.Count() - 1);

            var builder = new SpanBuilder
            {
                CodeGenerator = span.CodeGenerator,
                EditHandler = span.EditHandler,
                Kind = span.Kind
            };

            foreach (var symbol in valueSymbols)
            {
                builder.Accept(symbol);
            }

            return new KeyValuePair<string, SyntaxTreeNode>(name, builder.Build());
        }

        private static KeyValuePair<string, SyntaxTreeNode> ParseBlock(Block block)
        {
            Debug.Assert(block.Children.First() is Span);

            var builder = new BlockBuilder(block);

            // If there's only 1 child it means that it's plain text inside of the attribute.
            if (builder.Children.Count == 1)
            {
                Debug.Assert(builder.Children.First() is Span);

                return ParseSpan(builder.Children.First() as Span);
            }

            var childSpan = block.Children.First() as Span;
            var name = childSpan.Symbols.FirstHtmlSymbolAs(HtmlSymbolType.Text)?.Content ?? string.Empty;

            // Remove first and last child foo="  AND   "
            builder.Children.RemoveAt(0);
            builder.Children.RemoveAt(builder.Children.Count - 1);

            // We need to rebuild the code generators of the builder and its children (this is needed to
            // ensure we don't do special attribute code generation since this is a tag helper).
            block = RebuildCodeGenerators(builder.Build());

            return new KeyValuePair<string, SyntaxTreeNode>(name, block);
        }

        private static bool IsAttributeValueStart(HtmlSymbol htmlSymbol)
        {
            return htmlSymbol.Type == HtmlSymbolType.DoubleQuote ||
                   htmlSymbol.Type == HtmlSymbolType.SingleQuote;
        }
    }
}