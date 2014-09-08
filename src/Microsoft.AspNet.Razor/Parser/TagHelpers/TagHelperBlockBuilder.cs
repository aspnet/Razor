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
    /// A <see cref="TagHelperBlockBuilder"/> used to create <see cref="TagHelperBlock"/>s.
    /// </summary>
    public class TagHelperBlockBuilder : BlockBuilder
    {
        /// <summary>
        /// Copies the given <paramref name="original"/>'s <see cref="TagHelperBlock.TagName"/>,
        /// <see cref="TagHelperBlock.Attributes"/>, <see cref="Block.Type"/>, <see cref="Block.Children"/>
        /// and <see cref="Block.CodeGenerator"/> into a new <see cref="TagHelperBlockBuilder"/>.
        /// </summary>
        /// <param name="original"></param>
        public TagHelperBlockBuilder(TagHelperBlock original)
            : base(original)
        {
            TagName = original.TagName;
            Attributes = new Dictionary<string, SyntaxTreeNode>(original.Attributes);
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperBlockBuilder"/> class
        /// with the provided <paramref name="tagName"/> and derives its <see cref="Attributes"/>
        /// and <see cref="BlockBuilder.Type"/> from the <paramref name="startTag"/>.
        /// </summary>
        /// <param name="tagName">An HTML tag name.</param>
        /// <param name="startTag">The <see cref="Block"/> that contains all information about the start
        /// of the HTML element.</param>
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
        public string TagName { get; set; }

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

            if (Attributes != null)
            {
                Attributes.Clear();
            }

            base.Reset();
        }

        private static IDictionary<string, SyntaxTreeNode> GetTagAttributes(Block tagBlock)
        {
            var attributes = new Dictionary<string, SyntaxTreeNode>(StringComparer.OrdinalIgnoreCase);
            var tagBlockChildCount = tagBlock.Children.Count();

            // TODO: Handle malformed tags: https://github.com/aspnet/razor/issues/104

            // We skip the first child "<tagname" and take everything up to the "ending" portion of the tag ">" or "/>".
            // The -2 accounts for both the start and end tags.
            var attributeChildren = tagBlock.Children.Skip(1).Take(tagBlockChildCount - 2);

            foreach (var child in attributeChildren)
            {
                KeyValuePair<string, SyntaxTreeNode> attribute;

                if (child.IsBlock)
                {
                    attribute = ParseBlock((Block)child);
                }
                else
                {
                    attribute = ParseSpan((Span)child);
                }

                attributes.Add(attribute.Key, attribute.Value);
            }

            return attributes;
        }

        // This method handles cases when the attribute is a "simple" span attribute such as
        // class="something moresomething".  This does not handle complex attributes such as
        // class="@myclass". Therefore the span.Content is equivalent to the entire attribute.
        private static KeyValuePair<string, SyntaxTreeNode> ParseSpan(Span span)
        {
            var afterEquals = false;
            var builder = new SpanBuilder
            {
                CodeGenerator = span.CodeGenerator,
                EditHandler = span.EditHandler,
                Kind = span.Kind
            };
            var htmlSymbols = ((IEnumerable<HtmlSymbol>)span.Symbols).ToArray();
            var symbolOffset = 1;
            string name = null;

            // Iterate down through the symbols to find the name and the start of the value.
            // -symbolOffset is so we don't accept an ending quote of a span.
            for (var i = 0; i < htmlSymbols.Length - symbolOffset; i++)
            {
                var symbol = htmlSymbols[i];

                if (name == null && symbol.Type == HtmlSymbolType.Text)
                {
                    name = symbol.Content;
                }
                else if (symbol.Type == HtmlSymbolType.Equals)
                {
                    // We've found an '=' symbol, this means that the coming symbols will either be whitespace, quote
                    // or value (in the case that the value is unquoted).

                    // TODO: Handle malformed tags, if there's an '=' then there MUST be a value.
                    // https://github.com/aspnet/Razor/issues/104

                    // Check for attribute start values, aka single or double quote
                    if (IsQuote(htmlSymbols[i + 1]))
                    {
                        // Move past the attribute start so we can accept the true value.
                        i++;
                    }
                    else
                    {
                        // Set the symbol offset to 0 so we don't attemp to skip an end quote that doesn't exist.
                        symbolOffset = 0;
                    }

                    afterEquals = true;
                }
                else if (afterEquals)
                {
                    builder.Accept(symbol);
                }
            }

            return new KeyValuePair<string, SyntaxTreeNode>(name, builder.Build());
        }

        private static KeyValuePair<string, SyntaxTreeNode> ParseBlock(Block block)
        {
            // TODO: Accept more than just spans: https://github.com/aspnet/Razor/issues/96.
            // The first child will only ever NOT be a Span if a user is doing something like:
            // <input type="checkbox" @checked />
            Debug.Assert(block.Children.First() is Span);

            var builder = new BlockBuilder(block);

            // If there's only 1 child it means that it's plain text inside of the attribute.
            if (builder.Children.Count == 1)
            {
                return ParseSpan((Span)builder.Children[0]);
            }

            var childSpan = (Span)block.Children.First();
            var name = childSpan.Symbols.FirstHtmlSymbolAs(HtmlSymbolType.Text)?.Content;

            if (name == null)
            {
                throw new InvalidOperationException(RazorResources.TagHelpers_AttributesMustHaveAName);
            }

            // Remove first child i.e. foo="
            builder.Children.RemoveAt(0);

            // Grabbing last child to check if the attribute value is quoted.
            var endNode = block.Children.Last();
            if (!endNode.IsBlock)
            {
                var endSpan = (Span)endNode;
                var endSymbol = (HtmlSymbol)endSpan.Symbols.Last();

                // Checking to see if it's not a quoteless attribute, if so we should remove end quote
                if (IsQuote(endSymbol))
                {
                    builder.Children.RemoveAt(builder.Children.Count - 1);
                }
            }

            // We need to rebuild the code generators of the builder and its children (this is needed to
            // ensure we don't do special attribute code generation since this is a tag helper).
            block = RebuildCodeGenerators(builder.Build());

            return new KeyValuePair<string, SyntaxTreeNode>(name, block);
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
                    // The child is a block, recurse down into the block to rebuild its children
                    builder.Children[i] = RebuildCodeGenerators((Block)child);
                }
                else
                {
                    var childSpan = (Span)child;
                    ISpanCodeGenerator newCodeGenerator = null;
                    var literalGenerator = childSpan.CodeGenerator as LiteralAttributeCodeGenerator;

                    if (literalGenerator != null)
                    {
                        if (literalGenerator.ValueGenerator == null || literalGenerator.ValueGenerator.Value == null)
                        {
                            newCodeGenerator = new MarkupCodeGenerator();
                        }
                        else
                        {
                            newCodeGenerator = literalGenerator.ValueGenerator.Value;
                        }
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

        private static bool IsQuote(HtmlSymbol htmlSymbol)
        {
            return htmlSymbol.Type == HtmlSymbolType.DoubleQuote ||
                   htmlSymbol.Type == HtmlSymbolType.SingleQuote;
        }
    }
}