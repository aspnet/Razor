﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;

namespace Microsoft.AspNet.Razor.Parser.TagHelpers.Internal
{
    public class TagHelperParseTreeRewriter : ISyntaxTreeRewriter
    {
        private TagHelperDescriptorProvider _provider;
        private Stack<TagHelperBlockBuilder> _tagStack;
        private Stack<BlockBuilder> _blockStack;
        private BlockBuilder _currentBlock;

        public TagHelperParseTreeRewriter(TagHelperDescriptorProvider provider)
        {
            _provider = provider;
            _tagStack = new Stack<TagHelperBlockBuilder>();
            _blockStack = new Stack<BlockBuilder>();
        }

        public Block Rewrite(Block input)
        {
            RewriteTags(input);

            Debug.Assert(_blockStack.Count == 0);

            return _currentBlock.Build();
        }

        private void RewriteTags(Block input)
        {
            // We want to start a new block without the children from existing (we rebuild them).
            TrackBlock(new BlockBuilder
            {
                Type = input.Type,
                CodeGenerator = input.CodeGenerator
            });

            foreach (var child in input.Children)
            {
                if (child.IsBlock)
                {
                    var childBlock = (Block)child;

                    if (childBlock.Type == BlockType.Tag)
                    {
                        // TODO: Fully handle malformed tags: https://github.com/aspnet/Razor/issues/104

                        // Get tag name of the current block (doesn't matter if it's an end or start tag)
                        var tagName = GetTagName(childBlock);

                        if (tagName == null)
                        {
                            continue;
                        }

                        if (!IsEndTag(childBlock))
                        {
                            // We're in a begin tag block

                            if (IsValidTagHelper(tagName, childBlock) && IsRegisteredTagHelper(tagName))
                            {
                                // Found a new tag helper block
                                TrackTagHelperBlock(new TagHelperBlockBuilder(tagName, childBlock));

                                // If it's a self closing block then we don't have to worry about nested children 
                                // within the tag... complete it.
                                if (IsSelfClosing(childBlock))
                                {
                                    BuildCurrentlyTrackedTagHelperBlock();
                                }

                                continue;
                            }
                        }
                        else
                        {
                            var currentTagHelper = _tagStack.Count > 0 ? _tagStack.Peek() : null;

                            // Check if it's an "end" tag helper that matches our current tag helper
                            if (currentTagHelper != null &&
                                string.Equals(currentTagHelper.TagName, tagName, StringComparison.OrdinalIgnoreCase))
                            {
                                BuildCurrentlyTrackedTagHelperBlock();
                                continue;
                            }

                            // We're in an end tag, there won't be anymore tag helpers nested.
                        }

                        // If we get to here it means that we're a normal html tag.  No need to iterate
                        // any deeper into the children of it because they wont be tag helpers.
                    }
                    else
                    {
                        // We're not an Html tag so iterate through children recursively.
                        RewriteTags(childBlock);
                        continue;
                    }
                }

                // Add the child to current block. 
                // At this point the child is a Span (not a tag helper or block).
                _currentBlock.Children.Add(child);
            }

            BuildCurrentlyTrackedBlock();
        }

        private bool IsValidTagHelper(string tagName, Block childBlock)
        {
            var child = childBlock.Children.FirstOrDefault();
            Debug.Assert(child != null);

            var childSpan = (Span)child;

            // text tags that are labeled as transitions should be ignored aka they're not tag helpers.
            return !string.Equals(tagName, SyntaxConstants.TextTagName, StringComparison.OrdinalIgnoreCase) ||
                   childSpan.Kind != SpanKind.Transition;
        }

        private bool IsRegisteredTagHelper(string tagName)
        {
            return _provider.GetTagHelpers(tagName).Any();
        }

        private void TrackBlock(BlockBuilder builder)
        {
            _currentBlock = builder;

            _blockStack.Push(builder);
        }

        private void TrackTagHelperBlock(TagHelperBlockBuilder builder)
        {
            _tagStack.Push(builder);

            TrackBlock(builder);
        }

        private void BuildCurrentlyTrackedBlock()
        {
            // Going to remove the current BlockBuilder from the stack because it's complete.
            var currentBlock = _blockStack.Pop();

            // If there are block stacks left it means we're not at the root.
            if (_blockStack.Count > 0)
            {
                // Grab the next block in line so we can continue managing its children (it's not done).
                var previousBlock = _blockStack.Peek();

                // We've finished the currentBlock so build it and add it to its parent.
                previousBlock.Children.Add(currentBlock.Build());

                // Update the _currentBlock to point at the last tracked block because it's not complete.
                _currentBlock = previousBlock;
            }
            else
            {
                _currentBlock = currentBlock;
            }
        }

        private void BuildCurrentlyTrackedTagHelperBlock()
        {
            _tagStack.Pop();

            BuildCurrentlyTrackedBlock();
        }

        private static string GetTagName(Block tagBlock)
        {
            var child = tagBlock.Children.First();

            if (tagBlock.Type != BlockType.Tag || !tagBlock.Children.Any() || !(child is Span))
            {
                return null;
            }

            var childSpan = (Span)child;

            return childSpan.Symbols.FirstHtmlSymbolAs(HtmlSymbolType.Text)?.Content;
        }

        private static bool IsSelfClosing(Block beginTagBlock)
        {
            EnsureTagBlock(beginTagBlock);

            var childSpan = (Span)beginTagBlock.Children.Last();

            return childSpan.Content.EndsWith("/>");
        }

        private static bool IsEndTag(Block tagBlock)
        {
            EnsureTagBlock(tagBlock);

            var childSpan = (Span)tagBlock.Children.First();
            // We grab the symbol that could be forward slash
            var relevantSymbol = (HtmlSymbol)childSpan.Symbols.Take(2).Last();

            return relevantSymbol.Type == HtmlSymbolType.ForwardSlash;
        }

        private static void EnsureTagBlock(Block tagBlock)
        {
            Debug.Assert(tagBlock.Type == BlockType.Tag);
            Debug.Assert(tagBlock.Children.First() is Span);
        }
    }
}