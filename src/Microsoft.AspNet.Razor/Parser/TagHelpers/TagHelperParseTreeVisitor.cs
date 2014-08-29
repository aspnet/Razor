// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.TagHelpers.Internal;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;

namespace Microsoft.AspNet.Razor.Parser.TagHelpers.Internal
{
    public class TagHelperParseTreeVisitor : ISyntaxTreeRewriter
    {
        private TagHelperProvider _provider;
        private Stack<TagHelperBlockBuilder> _tagStack;
        private Stack<BlockBuilder> _blockStack;
        private BlockBuilder _currentBlock;

        public TagHelperParseTreeVisitor(TagHelperProvider provider)
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
            // We want to start a new block without the children from existing (we re-build them).
            StartBlock(new BlockBuilder
            {
                Type = input.Type,
                CodeGenerator = input.CodeGenerator
            });

            foreach (var child in input.Children)
            {
                if (child.IsBlock)
                {
                    var childBlock = child as Block;

                    if (childBlock.Type == BlockType.Tag)
                    {
                        var currentTagHelper = _tagStack.Any() ? _tagStack.Peek() : null;

                        // TODO: Fully handle malformed tags: https://github.com/aspnet/Razor/issues/104

                        // Get tag name of the current block (doesn't matter if it's end or begin tag)
                        var tagName = GetTagName(childBlock);

                        if (tagName == null)
                        {
                            continue;
                        }

                        if (IsEndTag(childBlock))
                        {
                            // Check if it's an "end" tag helper that matches our current tag helper
                            if (currentTagHelper != null && currentTagHelper.TagName == tagName)
                            {
                                CompleteTagHelperBlock();
                                continue;
                            }

                            // We're in an end tag, there wont be anymore tag helpers nested.
                        }
                        else
                        {
                            // We're in a begin tag block

                            if (IsTagHelper(tagName) && IsValidTagHelper(tagName, childBlock))
                            {
                                // Found a new tag helper block
                                StartTagHelperBlock(new TagHelperBlockBuilder(tagName, childBlock));

                                // If it's a self closing block then we don't have any children... complete it.
                                if (IsSelfClosing(childBlock))
                                {
                                    CompleteTagHelperBlock();
                                }

                                continue;
                            }
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

                // Add the child to "current" block. We need to rebuild the block so we have a chance
                // at finding tag helpers.
                _currentBlock.Children.Add(child);
            }

            CompleteBlock();
        }

        private bool IsValidTagHelper(string tagName, Block childBlock)
        {
            Debug.Assert(childBlock.Children.Any());

            var child = childBlock.Children.First() as Span;

            // text tags that are labeled as transitions should be ignored aka they're not tag helpers.
            if (tagName == SyntaxConstants.TextTagName && child.Kind == SpanKind.Transition)
            {
                return false;
            }

            return true;
        }

        private bool IsTagHelper(string tagName)
        {
            return _provider.GetTagHelpers(tagName).Any();
        }

        private void StartBlock(BlockBuilder builder)
        {
            _currentBlock = builder;

            _blockStack.Push(builder);
        }

        private void StartTagHelperBlock(TagHelperBlockBuilder builder)
        {
            _tagStack.Push(builder);

            StartBlock(builder);
        }

        private void CompleteBlock()
        {
            var currentBlock = _blockStack.Pop();

            if (_blockStack.Any())
            {
                _currentBlock = _blockStack.Peek();
                _currentBlock.Children.Add(currentBlock.Build());
            }
            else
            {
                _currentBlock = currentBlock;
            }
        }

        private void CompleteTagHelperBlock()
        {
            _tagStack.Pop();

            CompleteBlock();
        }

        private static string GetTagName(Block tagBlock)
        {
            Debug.Assert(tagBlock.Type == BlockType.Tag);
            Debug.Assert(tagBlock.Children.Any());
            Debug.Assert(tagBlock.Children.First() is Span);

            var childSpan = tagBlock.Children.First() as Span;

            return childSpan.Symbols.FirstHtmlSymbolAs(HtmlSymbolType.Text)?.Content;
        }

        private static bool IsSelfClosing(Block beginTagBlock)
        {
            EnsureTagBlock(beginTagBlock);

            var childSpan = beginTagBlock.Children.Last() as Span;

            return childSpan.Content.EndsWith("/>");
        }

        private static bool IsEndTag(Block tagBlock)
        {
            EnsureTagBlock(tagBlock);

            var childSpan = tagBlock.Children.First() as Span;
            // We grab the symbol that should be forward slash
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