// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal class TagHelperParseTreeRewriter
    {
        public static RazorSyntaxTree Rewrite(RazorSyntaxTree syntaxTree, string tagHelperPrefix, IEnumerable<TagHelperDescriptor> descriptors)
        {
            var errorSink = new ErrorSink();
            var rewriter = new Rewriter(
                syntaxTree.Source,
                tagHelperPrefix,
                descriptors,
                syntaxTree.Options.FeatureFlags,
                errorSink);

            var rewritten = rewriter.Visit(syntaxTree.Root);

            var errorList = new List<RazorDiagnostic>();
            errorList.AddRange(errorSink.Errors);
            errorList.AddRange(descriptors.SelectMany(d => d.GetAllDiagnostics()));

            var diagnostics = CombineErrors(syntaxTree.Diagnostics, errorList).OrderBy(error => error.Span.AbsoluteIndex);

            var newSyntaxTree = RazorSyntaxTree.Create(rewritten, syntaxTree.Source, diagnostics, syntaxTree.Options);

            return newSyntaxTree;
        }

        private static IReadOnlyList<RazorDiagnostic> CombineErrors(IReadOnlyList<RazorDiagnostic> errors1, IReadOnlyList<RazorDiagnostic> errors2)
        {
            var combinedErrors = new List<RazorDiagnostic>(errors1.Count + errors2.Count);
            combinedErrors.AddRange(errors1);
            combinedErrors.AddRange(errors2);

            return combinedErrors;
        }

        // Internal for testing.
        internal class Rewriter : SyntaxRewriter
        {
            // Internal for testing.
            // Null characters are invalid markup for HTML attribute values.
            internal static readonly string InvalidAttributeValueMarker = "\0";

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

            private readonly RazorSourceDocument _source;
            private readonly string _tagHelperPrefix;
            private readonly List<KeyValuePair<string, string>> _htmlAttributeTracker;
            private readonly StringBuilder _attributeValueBuilder;
            private readonly TagHelperBinder _tagHelperBinder;
            private readonly Stack<TagBlockTracker> _trackerStack;
            private TagHelperBlockTracker _currentTagHelperTracker;
            private readonly ErrorSink _errorSink;
            private RazorParserFeatureFlags _featureFlags;

            public Rewriter(
                RazorSourceDocument source,
                string tagHelperPrefix,
                IEnumerable<TagHelperDescriptor> descriptors,
                RazorParserFeatureFlags featureFlags,
                ErrorSink errorSink)
            {
                _source = source;
                _tagHelperPrefix = tagHelperPrefix;
                _tagHelperBinder = new TagHelperBinder(tagHelperPrefix, descriptors);
                _trackerStack = new Stack<TagBlockTracker>();
                _attributeValueBuilder = new StringBuilder();
                _htmlAttributeTracker = new List<KeyValuePair<string, string>>();
                _featureFlags = featureFlags;
                _errorSink = errorSink;
                _currentTagHelperTracker = null;
            }

            private TagBlockTracker CurrentTracker => _trackerStack.Count > 0 ? _trackerStack.Peek() : null;

            private string CurrentParentTagName => CurrentTracker?.TagName;

            private bool CurrentParentIsTagHelper => CurrentTracker?.IsTagHelper ?? false;

            public override SyntaxNode VisitMarkupTagBlock(MarkupTagBlockSyntax node)
            {
                if (TryRewriteTagHelper(node, out var rewritten))
                {
                    return Visit(rewritten);
                }

                // Non-TagHelper tag.
                ValidateParentAllowsPlainTag(node);
                return base.VisitMarkupTagBlock(node);
            }

            private bool TryRewriteTagHelper(MarkupTagBlockSyntax tagBlock, out SyntaxNode rewritten)
            {
                rewritten = tagBlock;

                // Get tag name of the current block (doesn't matter if it's an end or start tag)
                var tagName = GetTagName(tagBlock);

                // Could not determine tag name, it can't be a TagHelper, continue on and track the element.
                if (tagName == null)
                {
                    return false;
                }

                TagHelperBinding tagHelperBinding;

                if (!IsPotentialTagHelper(tagName, tagBlock))
                {
                    return false;
                }

                var tracker = _currentTagHelperTracker;
                var tagNameScope = tracker?.TagName ?? string.Empty;

                if (!IsEndTag(tagBlock))
                {
                    // We're now in a start tag block, we first need to see if the tag block is a tag helper.
                    var elementAttributes = GetAttributeNameValuePairs(tagBlock);

                    tagHelperBinding = _tagHelperBinder.GetBinding(
                        tagName,
                        elementAttributes,
                        CurrentParentTagName,
                        CurrentParentIsTagHelper);

                    // If there aren't any TagHelperDescriptors registered then we aren't a TagHelper
                    if (tagHelperBinding == null)
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

                    ValidateParentAllowsTagHelper(tagName, tagBlock);
                    ValidateBinding(tagHelperBinding, tagName, tagBlock);

                    // We're in a start TagHelper block.
                    var validTagStructure = ValidateTagSyntax(tagName, tagBlock);

                    var startTag = TagHelperBlockRewriter.Rewrite(
                        tagName,
                        validTagStructure,
                        _featureFlags,
                        tagBlock,
                        tagHelperBinding,
                        _errorSink,
                        _source);

                    var tagMode = TagHelperBlockRewriter.GetTagMode(tagBlock, tagHelperBinding, _errorSink);
                    var tagHelperInfo = new TagHelperInfo(tagName, tagMode, tagHelperBinding);

                    //// Track the original start tag so the editor knows where each piece of the TagHelperBlock lies
                    //// for formatting.
                    //builder.SourceStartTag = tagBlock;

                    // Found a new tag helper block
                    //TrackTagHelperBlock(startTag, tagMode, tagHelperBinding);

                    // If it's a non-content expecting block then we don't have to worry about nested children within the
                    // tag. Complete it.
                    if (tagMode == TagMode.SelfClosing || tagMode == TagMode.StartTagOnly)
                    {
                        //BuildCurrentlyTrackedTagHelperBlock(endTag: null);
                    }
                }
                else
                {
                    // TODO
                }

                return true;
            }

            // Internal for testing
            internal IReadOnlyList<KeyValuePair<string, string>> GetAttributeNameValuePairs(MarkupTagBlockSyntax tagBlock)
            {
                // Need to calculate how many children we should take that represent the attributes.
                var childrenOffset = IsPartialTag(tagBlock) ? 0 : 1;
                var childCount = tagBlock.Children.Count - childrenOffset;

                if (childCount <= 1)
                {
                    return Array.Empty<KeyValuePair<string, string>>();
                }

                _htmlAttributeTracker.Clear();

                var attributes = _htmlAttributeTracker;

                for (var i = 1; i < childCount; i++)
                {
                    if (tagBlock.Children[i] is CSharpCodeBlockSyntax)
                    {
                        // Code blocks in the attribute area of tags mangles following attributes.
                        // It's also not supported by TagHelpers, bail early to avoid creating bad attribute value pairs.
                        break;
                    }

                    if (tagBlock.Children[i] is MarkupMinimizedAttributeBlockSyntax minimizedAttributeBlock)
                    {
                        if (minimizedAttributeBlock.Name == null)
                        {
                            _attributeValueBuilder.Append(InvalidAttributeValueMarker);
                            continue;
                        }

                        var minimizedAttribute = new KeyValuePair<string, string>(minimizedAttributeBlock.Name.GetContent(), string.Empty);
                        attributes.Add(minimizedAttribute);
                        continue;
                    }

                    if (!(tagBlock.Children[i] is MarkupAttributeBlockSyntax attributeBlock))
                    {
                        // If the parser thought these aren't attributes, we don't care about them. Move on.
                        continue;
                    }

                    if (attributeBlock.Name == null)
                    {
                        _attributeValueBuilder.Append(InvalidAttributeValueMarker);
                        continue;
                    }

                    for (var j = 0; j < attributeBlock.Value.Children.Count; j++)
                    {
                        var child = attributeBlock.Value.Children[j];
                        if (child is MarkupLiteralAttributeValueSyntax literalValue)
                        {
                            _attributeValueBuilder.Append(literalValue.GetContent());
                        }
                        else
                        {
                            _attributeValueBuilder.Append(InvalidAttributeValueMarker);
                        }
                    }

                    var attributeName = attributeBlock.Name.GetContent();
                    var attributeValue = _attributeValueBuilder.ToString();
                    var attribute = new KeyValuePair<string, string>(attributeName, attributeValue);
                    attributes.Add(attribute);

                    _attributeValueBuilder.Clear();
                }

                return attributes;
            }

            private void ValidateParentAllowsTagHelper(string tagName, MarkupTagBlockSyntax tagBlock)
            {
                if (HasAllowedChildren() &&
                    !_currentTagHelperTracker.PrefixedAllowedChildren.Contains(tagName, StringComparer.OrdinalIgnoreCase))
                {
                    OnAllowedChildrenTagError(_currentTagHelperTracker, tagName, tagBlock, _errorSink, _source);
                }
            }

            private void ValidateBinding(
                TagHelperBinding bindingResult,
                string tagName,
                MarkupTagBlockSyntax tagBlock)
            {
                // Ensure that all descriptors associated with this tag have appropriate TagStructures. Cannot have
                // multiple descriptors that expect different TagStructures (other than TagStructure.Unspecified).
                TagHelperDescriptor baseDescriptor = null;
                TagStructure? baseStructure = null;
                foreach (var descriptor in bindingResult.Descriptors)
                {
                    var boundRules = bindingResult.GetBoundRules(descriptor);
                    foreach (var rule in boundRules)
                    {
                        if (rule.TagStructure != TagStructure.Unspecified)
                        {
                            // Can't have a set of TagHelpers that expect different structures.
                            if (baseStructure.HasValue && baseStructure != rule.TagStructure)
                            {
                                _errorSink.OnError(
                                    RazorDiagnosticFactory.CreateTagHelper_InconsistentTagStructure(
                                        new SourceSpan(tagBlock.GetSourceLocation(_source), tagBlock.FullWidth),
                                        baseDescriptor.DisplayName,
                                        descriptor.DisplayName,
                                        tagName));
                            }

                            baseDescriptor = descriptor;
                            baseStructure = rule.TagStructure;
                        }
                    }
                }
            }

            private bool ValidateTagSyntax(string tagName, MarkupTagBlockSyntax tag)
            {
                // We assume an invalid syntax until we verify that the tag meets all of our "valid syntax" criteria.
                if (IsPartialTag(tag))
                {
                    var errorStart = GetTagDeclarationErrorStart(tag);

                    _errorSink.OnError(
                        RazorDiagnosticFactory.CreateParsing_TagHelperMissingCloseAngle(
                            new SourceSpan(errorStart, tagName.Length), tagName));

                    return false;
                }

                return true;
            }

            private bool IsPotentialTagHelper(string tagName, MarkupTagBlockSyntax childBlock)
            {
                Debug.Assert(childBlock.Children.Count > 0);
                var child = childBlock.Children[0];

                return !string.Equals(tagName, SyntaxConstants.TextTagName, StringComparison.OrdinalIgnoreCase) ||
                       child.Kind != SyntaxKind.MarkupTransition;
            }

            private SourceLocation GetTagDeclarationErrorStart(MarkupTagBlockSyntax tagBlock)
            {
                var advanceBy = IsEndTag(tagBlock) ? "</" : "<";

                return SourceLocationTracker.Advance(tagBlock.GetSourceLocation(_source), advanceBy);
            }

            private static bool IsPartialTag(MarkupTagBlockSyntax tagBlock)
            {
                // No need to validate the tag end because in order to be a tag block it must start with '<'.
                var tagEnd = tagBlock.Children[tagBlock.Children.Count - 1];

                // If our tag end is not a markup span it means it's some sort of code SyntaxTreeNode (not a valid format)
                if (tagEnd != null && tagEnd is MarkupTextLiteralSyntax tagEndLiteral)
                {
                    var endToken = tagEndLiteral.LiteralTokens.Count > 0 ?
                        tagEndLiteral.LiteralTokens[tagEndLiteral.LiteralTokens.Count - 1] :
                        null;

                    if (endToken != null && endToken.Kind == SyntaxKind.CloseAngle)
                    {
                        return false;
                    }
                }

                return true;
            }

            private void ValidateParentAllowsContent(SyntaxNode child)
            {
                if (HasAllowedChildren())
                {
                    var isDisallowedContent = true;
                    if (_featureFlags.AllowHtmlCommentsInTagHelpers)
                    {
                        // TODO: Questionable logic. Need to revisit
                        isDisallowedContent = !IsComment(child) &&
                            child.Kind != SyntaxKind.MarkupTransition &&
                            child.Kind != SyntaxKind.CSharpTransition &&
                            child.Kind != SyntaxKind.CSharpStatementLiteral &&
                            child.Kind != SyntaxKind.CSharpExpressionLiteral;
                    }

                    if (isDisallowedContent)
                    {
                        var content = child.GetContent();
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            var trimmedStart = content.TrimStart();
                            var whitespace = content.Substring(0, content.Length - trimmedStart.Length);
                            var errorStart = SourceLocationTracker.Advance(child.GetSourceLocation(_source), whitespace);
                            var length = trimmedStart.TrimEnd().Length;
                            var allowedChildren = _currentTagHelperTracker.AllowedChildren;
                            var allowedChildrenString = string.Join(", ", allowedChildren);
                            _errorSink.OnError(
                                RazorDiagnosticFactory.CreateTagHelper_CannotHaveNonTagContent(
                                    new SourceSpan(errorStart, length),
                                    _currentTagHelperTracker.TagName,
                                    allowedChildrenString));
                        }
                    }
                }
            }

            private void ValidateParentAllowsPlainTag(MarkupTagBlockSyntax tagBlock)
            {
                var tagName = GetTagName(tagBlock);

                // Treat partial tags such as '</' which have no tag names as content.
                if (string.IsNullOrEmpty(tagName))
                {
                    Debug.Assert(tagBlock.Children.First() is MarkupTextLiteralSyntax);

                    ValidateParentAllowsContent(tagBlock.Children.First());
                    return;
                }

                if (!HasAllowedChildren())
                {
                    return;
                }

                var tagHelperBinding = _tagHelperBinder.GetBinding(
                    tagName,
                    attributes: Array.Empty<KeyValuePair<string, string>>(),
                    parentTagName: CurrentParentTagName,
                    parentIsTagHelper: CurrentParentIsTagHelper);

                // If we found a binding for the current tag, then it is a tag helper. Use the prefixed allowed children to compare.
                var allowedChildren = tagHelperBinding != null ? _currentTagHelperTracker.PrefixedAllowedChildren : _currentTagHelperTracker.AllowedChildren;
                if (!allowedChildren.Contains(tagName, StringComparer.OrdinalIgnoreCase))
                {
                    OnAllowedChildrenTagError(_currentTagHelperTracker, tagName, tagBlock, _errorSink, _source);
                }
            }

            private bool HasAllowedChildren()
            {
                // TODO: Questionable logic. Need to revisit
                var currentTracker = _trackerStack.Count > 0 ? _trackerStack.Peek() : null;

                // If the current tracker is not a TagHelper then there's no AllowedChildren to enforce.
                if (currentTracker == null || !currentTracker.IsTagHelper)
                {
                    return false;
                }

                return _currentTagHelperTracker.AllowedChildren != null && _currentTagHelperTracker.AllowedChildren.Count > 0;
            }

            internal static bool IsComment(SyntaxNode node)
            {
                var commentParent = node.FirstAncestorOrSelf<SyntaxNode>(
                    n => n is RazorCommentBlockSyntax || n is MarkupCommentBlockSyntax);

                return commentParent != null;
            }

            private static string GetTagName(MarkupTagBlockSyntax tagBlock)
            {
                var child = tagBlock.Children[0];

                if (tagBlock.Children.Count == 0 || !(child is MarkupTextLiteralSyntax))
                {
                    return null;
                }

                var childLiteral = (MarkupTextLiteralSyntax)child;
                SyntaxToken textToken = null;
                for (var i = 0; i < childLiteral.LiteralTokens.Count; i++)
                {
                    var token = childLiteral.LiteralTokens[i];

                    if (token != null &&
                        (token.Kind == SyntaxKind.Whitespace || token.Kind == SyntaxKind.Text))
                    {
                        textToken = token;
                        break;
                    }
                }

                if (textToken == null)
                {
                    return null;
                }

                return textToken.Kind == SyntaxKind.Whitespace ? null : textToken.Content;
            }

            private static void OnAllowedChildrenTagError(
                TagHelperBlockTracker tracker,
                string tagName,
                MarkupTagBlockSyntax tagBlock,
                ErrorSink errorSink,
                RazorSourceDocument source)
            {
                var allowedChildrenString = string.Join(", ", tracker.AllowedChildren);
                var errorStart = GetTagDeclarationErrorStart(tagBlock, source);

                errorSink.OnError(
                    RazorDiagnosticFactory.CreateTagHelper_InvalidNestedTag(
                        new SourceSpan(errorStart, tagName.Length),
                        tagName,
                        tracker.TagName,
                        allowedChildrenString));
            }

            private static SourceLocation GetTagDeclarationErrorStart(MarkupTagBlockSyntax tagBlock, RazorSourceDocument source)
            {
                var advanceBy = IsEndTag(tagBlock) ? "</" : "<";

                return SourceLocationTracker.Advance(tagBlock.GetSourceLocation(source), advanceBy);
            }

            private static bool IsEndTag(MarkupTagBlockSyntax tagBlock)
            {
                var childSpan = (MarkupTextLiteralSyntax)tagBlock.Children.First();

                // We grab the token that could be forward slash
                var relevantToken = childSpan.LiteralTokens[childSpan.LiteralTokens.Count == 1 ? 0 : 1];

                return relevantToken.Kind == SyntaxKind.ForwardSlash;
            }

            private class TagBlockTracker
            {
                public TagBlockTracker(string tagName, bool isTagHelper, int depth)
                {
                    TagName = tagName;
                    IsTagHelper = isTagHelper;
                    Depth = depth;
                }

                public string TagName { get; }

                public bool IsTagHelper { get; }

                public int Depth { get; }
            }

            private class TagHelperBlockTracker : TagBlockTracker
            {
                private IReadOnlyList<string> _prefixedAllowedChildren;
                private readonly string _tagHelperPrefix;

                public TagHelperBlockTracker(string tagHelperPrefix, TagHelperBlockBuilder builder)
                    : base(builder.TagName, isTagHelper: true, depth: 0)
                {
                    _tagHelperPrefix = tagHelperPrefix;
                    Builder = builder;

                    if (Builder.BindingResult.Descriptors.Any(descriptor => descriptor.AllowedChildTags != null))
                    {
                        AllowedChildren = Builder.BindingResult.Descriptors
                            .Where(descriptor => descriptor.AllowedChildTags != null)
                            .SelectMany(descriptor => descriptor.AllowedChildTags.Select(childTag => childTag.Name))
                            .Distinct(StringComparer.OrdinalIgnoreCase)
                            .ToList();
                    }
                }

                public TagHelperBlockBuilder Builder { get; }

                public uint OpenMatchingTags { get; set; }

                public IReadOnlyList<string> AllowedChildren { get; }

                public IReadOnlyList<string> PrefixedAllowedChildren
                {
                    get
                    {
                        if (AllowedChildren != null && _prefixedAllowedChildren == null)
                        {
                            Debug.Assert(Builder.BindingResult.Descriptors.Count() >= 1);

                            _prefixedAllowedChildren = AllowedChildren.Select(allowedChild => _tagHelperPrefix + allowedChild).ToList();
                        }

                        return _prefixedAllowedChildren;
                    }
                }
            }
        }
    }
}
