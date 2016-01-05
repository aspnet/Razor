// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.Razor.Compilation.TagHelpers;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;

namespace Microsoft.AspNet.Razor.Parser.TagHelpers.Internal
{
    public class TagHelperParseTreeRewriter : ISyntaxTreeRewriter
    {
        // From http://dev.w3.org/html5/spec/Overview.html#elements-0
        private static readonly HashSet<string> VoidElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "area",
            "base",
            "br",
            "col",
            "command",
            "embed",
            "hr",
            "img",
            "input",
            "keygen",
            "link",
            "meta",
            "param",
            "source",
            "track",
            "wbr"
        };

        private TagHelperDescriptorProvider _provider;
        private Stack<TagBlockTracker> _trackerStack;
        private TagHelperBlockTracker _currentTagHelperTracker;
        private Stack<BlockBuilder> _blockStack;
        private BlockBuilder _currentBlock;
        private string _currentParentTagName;

        public TagHelperParseTreeRewriter(TagHelperDescriptorProvider provider)
        {
            _provider = provider;
            _trackerStack = new Stack<TagBlockTracker>();
            _blockStack = new Stack<BlockBuilder>();
        }

        public void Rewrite(RewritingContext context)
        {
            RewriteTags(context.SyntaxTree, context);

            context.SyntaxTree = _currentBlock.Build();
        }

        private void RewriteTags(Block input, RewritingContext context)
        {
            // We want to start a new block without the children from existing (we rebuild them).
            TrackBlock(new BlockBuilder
            {
                Type = input.Type,
                ChunkGenerator = input.ChunkGenerator
            });

            var activeTrackers = _trackerStack.Count;

            foreach (var child in input.Children)
            {
                if (child.IsBlock)
                {
                    var childBlock = (Block)child;

                    if (childBlock.Type == BlockType.Tag)
                    {
                        if (TryRewriteTagHelper(childBlock, context))
                        {
                            continue;
                        }
                        else
                        {
                            // Non-TagHelper tag.
                            ValidateParentAllowsPlainTag(childBlock, context.ErrorSink);

                            TrackTagBlock(childBlock);
                        }

                        // If we get to here it means that we're a normal html tag.  No need to iterate any deeper into
                        // the children of it because they wont be tag helpers.
                    }
                    else
                    {
                        // We're not an Html tag so iterate through children recursively.
                        RewriteTags(childBlock, context);
                        continue;
                    }
                }
                else
                {
                    ValidateParentAllowsContent((Span)child, context.ErrorSink);
                }

                // At this point the child is a Span or Block with Type BlockType.Tag that doesn't happen to be a
                // tag helper.

                // Add the child to current block.
                _currentBlock.Children.Add(child);
            }

            // We captured the number of active tag helpers at the start of our logic, it should be the same. If not
            // it means that there are malformed tag helpers at the top of our stack.
            if (activeTrackers != _trackerStack.Count)
            {
                // Malformed tag helpers built here will be tag helpers that do not have end tags in the current block
                // scope. Block scopes are special cases in Razor such as @<p> would cause an error because there's no
                // matching end </p> tag in the template block scope and therefore doesn't make sense as a tag helper.
                BuildMalformedTagHelpers(_trackerStack.Count - activeTrackers, context);

                Debug.Assert(activeTrackers == _trackerStack.Count);
            }

            BuildCurrentlyTrackedBlock();
        }

        private void TrackTagBlock(Block childBlock)
        {
            var tagName = GetTagName(childBlock);

            // Don't want to track incomplete tags that have no tag name.
            if (string.IsNullOrWhiteSpace(tagName))
            {
                return;
            }

            if (IsEndTag(childBlock))
            {
                var parentTracker = _trackerStack.Count > 0 ? _trackerStack.Peek() : null;
                if (parentTracker != null &&
                    !parentTracker.IsTagHelper &&
                    string.Equals(parentTracker.TagName, tagName, StringComparison.OrdinalIgnoreCase))
                {
                    PopTrackerStack();
                }
            }
            else if (!VoidElements.Contains(tagName) && !IsSelfClosing(childBlock))
            {
                // If it's not a void element and it's not self-closing then we need to create a tag
                // tracker for it.
                var tracker = new TagBlockTracker(tagName, isTagHelper: false);
                PushTrackerStack(tracker);
            }
        }

        private bool TryRewriteTagHelper(Block tagBlock, RewritingContext context)
        {
            // Get tag name of the current block (doesn't matter if it's an end or start tag)
            var tagName = GetTagName(tagBlock);

            // Could not determine tag name, it can't be a TagHelper, continue on and track the element.
            if (tagName == null)
            {
                return false;
            }

            var descriptors = Enumerable.Empty<TagHelperDescriptor>();

            if (!IsPotentialTagHelper(tagName, tagBlock))
            {
                return false;
            }

            var tracker = _currentTagHelperTracker;
            var tagNameScope = tracker?.TagName ?? string.Empty;

            if (!IsEndTag(tagBlock))
            {
                // We're now in a start tag block, we first need to see if the tag block is a tag helper.
                var providedAttributes = GetAttributeNames(tagBlock);

                descriptors = _provider.GetDescriptors(tagName, providedAttributes, _currentParentTagName);

                // If there aren't any TagHelperDescriptors registered then we aren't a TagHelper
                if (!descriptors.Any())
                {
                    // If the current tag matches the current TagHelper scope it means the parent TagHelper matched
                    // all the required attributes but the current one did not; therefore, we need to increment the
                    // OpenMatchingTags counter for current the TagHelperBlock so we don't end it too early.
                    // ex: <myth req="..."><myth></myth></myth> We don't want the first myth to close on the inside
                    // tag.
                    if (string.Equals(tagNameScope, tagName, StringComparison.OrdinalIgnoreCase))
                    {
                        tracker.OpenMatchingTags++;
                    }

                    return false;
                }

                ValidateParentAllowsTagHelper(tagName, tagBlock, context.ErrorSink);
                ValidateDescriptors(descriptors, tagName, tagBlock, context.ErrorSink);

                // We're in a start TagHelper block.
                var validTagStructure = ValidateTagSyntax(tagName, tagBlock, context);

                var builder = TagHelperBlockRewriter.Rewrite(
                    tagName,
                    validTagStructure,
                    tagBlock,
                    descriptors,
                    context.ErrorSink);

                // Track the original start tag so the editor knows where each piece of the TagHelperBlock lies
                // for formatting.
                builder.SourceStartTag = tagBlock;

                // Found a new tag helper block
                TrackTagHelperBlock(builder);

                // If it's a non-content expecting block then we don't have to worry about nested children within the
                // tag. Complete it.
                if (builder.TagMode == TagMode.SelfClosing || builder.TagMode == TagMode.StartTagOnly)
                {
                    BuildCurrentlyTrackedTagHelperBlock(endTag: null);
                }
            }
            else
            {
                // Validate that our end tag matches the currently scoped tag, if not we may need to error.
                if (tagNameScope.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                {
                    // If there are additional end tags required before we can build our block it means we're in a
                    // situation like this: <myth req="..."><myth></myth></myth> where we're at the inside </myth>.
                    if (tracker.OpenMatchingTags > 0)
                    {
                        tracker.OpenMatchingTags--;

                        return false;
                    }

                    ValidateTagSyntax(tagName, tagBlock, context);

                    BuildCurrentlyTrackedTagHelperBlock(tagBlock);
                }
                else
                {
                    descriptors = _provider.GetDescriptors(
                        tagName,
                        attributeNames: Enumerable.Empty<string>(),
                        parentTagName: _currentParentTagName);

                    // If there are not TagHelperDescriptors associated with the end tag block that also have no
                    // required attributes then it means we can't be a TagHelper, bail out.
                    if (!descriptors.Any())
                    {
                        return false;
                    }

                    var invalidDescriptor = descriptors.FirstOrDefault(
                        descriptor => descriptor.TagStructure == TagStructure.WithoutEndTag);
                    if (invalidDescriptor != null)
                    {
                        // End tag TagHelper that states it shouldn't have an end tag.
                        context.ErrorSink.OnError(
                            SourceLocation.Advance(tagBlock.Start, "</"),
                            RazorResources.FormatTagHelperParseTreeRewriter_EndTagTagHelperMustNotHaveAnEndTag(
                                tagName,
                                invalidDescriptor.TypeName,
                                invalidDescriptor.TagStructure),
                            tagName.Length);

                        return false;
                    }

                    // Current tag helper scope does not match the end tag. Attempt to recover the tag
                    // helper by looking up the previous tag helper scopes for a matching tag. If we
                    // can't recover it means there was no corresponding tag helper start tag.
                    if (TryRecoverTagHelper(tagName, tagBlock, context))
                    {
                        ValidateParentAllowsTagHelper(tagName, tagBlock, context.ErrorSink);
                        ValidateTagSyntax(tagName, tagBlock, context);

                        // Successfully recovered, move onto the next element.
                    }
                    else
                    {
                        // Could not recover, the end tag helper has no corresponding start tag, create
                        // an error based on the current childBlock.
                        context.ErrorSink.OnError(
                            SourceLocation.Advance(tagBlock.Start, "</"),
                            RazorResources.FormatTagHelpersParseTreeRewriter_FoundMalformedTagHelper(tagName),
                            tagName.Length);

                        return false;
                    }
                }
            }

            return true;
        }

        private IEnumerable<string> GetAttributeNames(Block tagBlock)
        {
            // Need to calculate how many children we should take that represent the attributes.
            var childrenOffset = IsPartialTag(tagBlock) ? 1 : 2;
            var attributeChildren = tagBlock.Children.Skip(1).Take(tagBlock.Children.Count() - childrenOffset);
            var attributeNames = new List<string>();

            foreach (var child in attributeChildren)
            {
                Span childSpan;

                if (child.IsBlock)
                {
                    childSpan = ((Block)child).FindFirstDescendentSpan();

                    if (childSpan == null)
                    {
                        continue;
                    }
                }
                else
                {
                    childSpan = child as Span;
                }

                var attributeName = childSpan
                    .Content
                    .Split(separator: new[] { '=' }, count: 2)[0]
                    .TrimStart();

                attributeNames.Add(attributeName);
            }

            return attributeNames;
        }

        private bool HasAllowedChildren()
        {
            var currentTracker = _trackerStack.Count > 0 ? _trackerStack.Peek() : null;

            // If the current tracker is not a TagHelper then there's no AllowedChildren to enforce.
            if (currentTracker == null || !currentTracker.IsTagHelper)
            {
                return false;
            }

            return _currentTagHelperTracker.AllowedChildren != null;
        }

        private void ValidateParentAllowsContent(Span child, ErrorSink errorSink)
        {
            if (HasAllowedChildren())
            {
                var content = child.Content;
                if (!string.IsNullOrWhiteSpace(content))
                {
                    var trimmedStart = content.TrimStart();
                    var whitespace = content.Substring(0, content.Length - trimmedStart.Length);
                    var errorStart = SourceLocation.Advance(child.Start, whitespace);
                    var length = trimmedStart.TrimEnd().Length;
                    var allowedChildren = _currentTagHelperTracker.AllowedChildren;
                    var allowedChildrenString = string.Join(", ", allowedChildren);
                    errorSink.OnError(
                        errorStart,
                        RazorResources.FormatTagHelperParseTreeRewriter_CannotHaveNonTagContent(
                            _currentTagHelperTracker.TagName,
                            allowedChildrenString),
                        length);
                }
            }
        }

        private void ValidateParentAllowsPlainTag(Block tagBlock, ErrorSink errorSink)
        {
            var tagName = GetTagName(tagBlock);

            // Treat partial tags such as '</' which have no tag names as content.
            if (string.IsNullOrEmpty(tagName))
            {
                Debug.Assert(tagBlock.Children.First() is Span);

                ValidateParentAllowsContent((Span)tagBlock.Children.First(), errorSink);
                return;
            }

            var currentTracker = _trackerStack.Count > 0 ? _trackerStack.Peek() : null;

            if (HasAllowedChildren() &&
                !_currentTagHelperTracker.AllowedChildren.Contains(tagName, StringComparer.OrdinalIgnoreCase))
            {
                OnAllowedChildrenTagError(_currentTagHelperTracker, tagName, tagBlock, errorSink);
            }
        }

        private void ValidateParentAllowsTagHelper(string tagName, Block tagBlock, ErrorSink errorSink)
        {
            if (HasAllowedChildren() &&
                !_currentTagHelperTracker.PrefixedAllowedChildren.Contains(tagName, StringComparer.OrdinalIgnoreCase))
            {
                OnAllowedChildrenTagError(_currentTagHelperTracker, tagName, tagBlock, errorSink);
            }
        }

        private static void OnAllowedChildrenTagError(
            TagHelperBlockTracker tracker,
            string tagName,
            Block tagBlock,
            ErrorSink errorSink)
        {
            var allowedChildrenString = string.Join(", ", tracker.AllowedChildren);
            var errorMessage = RazorResources.FormatTagHelperParseTreeRewriter_InvalidNestedTag(
                tagName,
                tracker.TagName,
                allowedChildrenString);
            var errorStart = GetTagDeclarationErrorStart(tagBlock);

            errorSink.OnError(errorStart, errorMessage, tagName.Length);
        }

        private static void ValidateDescriptors(
            IEnumerable<TagHelperDescriptor> descriptors,
            string tagName,
            Block tagBlock,
            ErrorSink errorSink)
        {
            // Ensure that all descriptors associated with this tag have appropriate TagStructures. Cannot have
            // multiple descriptors that expect different TagStructures (other than TagStructure.Unspecified).
            TagHelperDescriptor baseDescriptor = null;
            foreach (var descriptor in descriptors)
            {
                if (descriptor.TagStructure != TagStructure.Unspecified)
                {
                    // Can't have a set of TagHelpers that expect different structures.
                    if (baseDescriptor != null && baseDescriptor.TagStructure != descriptor.TagStructure)
                    {
                        errorSink.OnError(
                            tagBlock.Start,
                            RazorResources.FormatTagHelperParseTreeRewriter_InconsistentTagStructure(
                                baseDescriptor.TypeName,
                                descriptor.TypeName,
                                tagName,
                                nameof(TagHelperDescriptor.TagStructure)),
                            tagBlock.Length);
                    }

                    baseDescriptor = descriptor;
                }
            }
        }

        private static bool ValidateTagSyntax(string tagName, Block tag, RewritingContext context)
        {
            // We assume an invalid syntax until we verify that the tag meets all of our "valid syntax" criteria.
            if (IsPartialTag(tag))
            {
                var errorStart = GetTagDeclarationErrorStart(tag);

                context.ErrorSink.OnError(
                    errorStart,
                    RazorResources.FormatTagHelpersParseTreeRewriter_MissingCloseAngle(tagName),
                    tagName.Length);

                return false;
            }

            return true;
        }

        private static SourceLocation GetTagDeclarationErrorStart(Block tagBlock)
        {
            var advanceBy = IsEndTag(tagBlock) ? "</" : "<";

            return SourceLocation.Advance(tagBlock.Start, advanceBy);
        }

        private static bool IsPartialTag(Block tagBlock)
        {
            // No need to validate the tag end because in order to be a tag block it must start with '<'.
            var tagEnd = tagBlock.Children.Last() as Span;

            // If our tag end is not a markup span it means it's some sort of code SyntaxTreeNode (not a valid format)
            if (tagEnd != null && tagEnd.Kind == SpanKind.Markup)
            {
                var endSymbol = tagEnd.Symbols.LastOrDefault() as HtmlSymbol;

                if (endSymbol != null && endSymbol.Type == HtmlSymbolType.CloseAngle)
                {
                    return false;
                }
            }

            return true;
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

        private void BuildCurrentlyTrackedTagHelperBlock(Block endTag)
        {
            Debug.Assert(_trackerStack.Any(tracker => tracker.IsTagHelper));

            // We need to pop all trackers until we reach our TagHelperBlock. We can throw away any non-TagHelper
            // trackers because they don't need to be well-formed.
            TagHelperBlockTracker tagHelperTracker;
            do
            {
                tagHelperTracker = PopTrackerStack() as TagHelperBlockTracker;
            }
            while (tagHelperTracker == null);

            // Track the original end tag so the editor knows where each piece of the TagHelperBlock lies
            // for formatting.
            tagHelperTracker.Builder.SourceEndTag = endTag;

            _currentTagHelperTracker =
                (TagHelperBlockTracker)_trackerStack.FirstOrDefault(tagBlockTracker => tagBlockTracker.IsTagHelper);

            BuildCurrentlyTrackedBlock();
        }

        private bool IsPotentialTagHelper(string tagName, Block childBlock)
        {
            var child = childBlock.Children.FirstOrDefault();
            Debug.Assert(child != null);

            var childSpan = (Span)child;

            // text tags that are labeled as transitions should be ignored aka they're not tag helpers.
            return !string.Equals(tagName, SyntaxConstants.TextTagName, StringComparison.OrdinalIgnoreCase) ||
                   childSpan.Kind != SpanKind.Transition;
        }

        private void TrackBlock(BlockBuilder builder)
        {
            _currentBlock = builder;

            _blockStack.Push(builder);
        }

        private void TrackTagHelperBlock(TagHelperBlockBuilder builder)
        {
            _currentTagHelperTracker = new TagHelperBlockTracker(builder);
            PushTrackerStack(_currentTagHelperTracker);

            TrackBlock(builder);
        }

        private bool TryRecoverTagHelper(string tagName, Block endTag, RewritingContext context)
        {
            var malformedTagHelperCount = 0;

            foreach (var tracker in _trackerStack)
            {
                if (tracker.IsTagHelper && tracker.TagName.Equals(tagName, StringComparison.OrdinalIgnoreCase))
                {
                    break;
                }

                malformedTagHelperCount++;
            }

            // If the malformedTagHelperCount == _tagStack.Count it means we couldn't find a start tag for the tag
            // helper, can't recover.
            if (malformedTagHelperCount != _trackerStack.Count)
            {
                BuildMalformedTagHelpers(malformedTagHelperCount, context);

                // One final build, this is the build that completes our target tag helper block which is not malformed.
                BuildCurrentlyTrackedTagHelperBlock(endTag);

                // We were able to recover
                return true;
            }

            // Could not recover tag helper. Aka we found a tag helper end tag without a corresponding start tag.
            return false;
        }

        private void BuildMalformedTagHelpers(int count, RewritingContext context)
        {
            for (var i = 0; i < count; i++)
            {
                var tracker = _trackerStack.Peek();

                // Skip all non-TagHelper entries. Non TagHelper trackers do not need to represent well-formed HTML.
                if (!tracker.IsTagHelper)
                {
                    PopTrackerStack();
                    continue;
                }

                var malformedTagHelper = ((TagHelperBlockTracker)tracker).Builder;

                context.ErrorSink.OnError(
                    SourceLocation.Advance(malformedTagHelper.Start, "<"),
                    RazorResources.FormatTagHelpersParseTreeRewriter_FoundMalformedTagHelper(
                        malformedTagHelper.TagName),
                    malformedTagHelper.TagName.Length);

                BuildCurrentlyTrackedTagHelperBlock(endTag: null);
            }
        }

        private static string GetTagName(Block tagBlock)
        {
            var child = tagBlock.Children.First();

            if (tagBlock.Type != BlockType.Tag || !tagBlock.Children.Any() || !(child is Span))
            {
                return null;
            }

            var childSpan = (Span)child;
            var textSymbol = childSpan.Symbols.FirstHtmlSymbolAs(HtmlSymbolType.WhiteSpace | HtmlSymbolType.Text);

            if (textSymbol == null)
            {
                return null;
            }

            return textSymbol.Type == HtmlSymbolType.WhiteSpace ? null : textSymbol.Content;
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

        private static bool IsSelfClosing(Block childBlock)
        {
            var childSpan = childBlock.FindLastDescendentSpan();

            return childSpan?.Content.EndsWith("/>", StringComparison.Ordinal) ?? false;
        }

        private void PushTrackerStack(TagBlockTracker tracker)
        {
            _currentParentTagName = tracker.TagName;
            _trackerStack.Push(tracker);
        }

        private TagBlockTracker PopTrackerStack()
        {
            var poppedTracker = _trackerStack.Pop();
            _currentParentTagName = _trackerStack.Count > 0 ? _trackerStack.Peek().TagName : null;

            return poppedTracker;
        }

        private class TagBlockTracker
        {
            public TagBlockTracker(string tagName, bool isTagHelper)
            {
                TagName = tagName;
                IsTagHelper = isTagHelper;
            }

            public string TagName { get; }

            public bool IsTagHelper { get; }
        }

        private class TagHelperBlockTracker : TagBlockTracker
        {
            private IEnumerable<string> _prefixedAllowedChildren;

            public TagHelperBlockTracker(TagHelperBlockBuilder builder)
                : base(builder.TagName, isTagHelper: true)
            {
                Builder = builder;

                if (Builder.Descriptors.Any(descriptor => descriptor.AllowedChildren != null))
                {
                    AllowedChildren = Builder.Descriptors
                        .Where(descriptor => descriptor.AllowedChildren != null)
                        .SelectMany(descriptor => descriptor.AllowedChildren)
                        .Distinct(StringComparer.OrdinalIgnoreCase);
                }
            }

            public TagHelperBlockBuilder Builder { get; }

            public uint OpenMatchingTags { get; set; }

            public IEnumerable<string> AllowedChildren { get; }

            public IEnumerable<string> PrefixedAllowedChildren
            {
                get
                {
                    if (AllowedChildren != null && _prefixedAllowedChildren == null)
                    {
                        Debug.Assert(Builder.Descriptors.Count() >= 1);

                        var prefix = Builder.Descriptors.First().Prefix;
                        _prefixedAllowedChildren = AllowedChildren.Select(allowedChild => prefix + allowedChild);
                    }

                    return _prefixedAllowedChildren;
                }
            }
        }
    }
}