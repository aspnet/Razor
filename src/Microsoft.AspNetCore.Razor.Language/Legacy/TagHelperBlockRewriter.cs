// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal static class TagHelperBlockRewriter
    {
        private static readonly string StringTypeName = typeof(string).FullName;

        public static TagHelperBlockBuilder Rewrite(
            string tagName,
            bool validStructure,
            RazorParserFeatureFlags featureFlags,
            MarkupTagBlockSyntax tag,
            TagHelperBinding bindingResult,
            ErrorSink errorSink,
            RazorSourceDocument source)
        {
            // There will always be at least one child for the '<'.
            var start = tag.Children[0].GetSourceLocation(source);
            var attributes = GetTagAttributes(tagName, validStructure, tag, bindingResult, featureFlags, errorSink, source);
            var tagMode = GetTagMode(tag, bindingResult, errorSink);

            return null;
        }

        private static IList<TagHelperAttributeNode> GetTagAttributes(
            string tagName,
            bool validStructure,
            MarkupTagBlockSyntax tagBlock,
            TagHelperBinding bindingResult,
            RazorParserFeatureFlags featureFlags,
            ErrorSink errorSink,
            RazorSourceDocument source)
        {
            var attributes = new List<TagHelperAttributeNode>();
            return attributes;
        }

        private static TagMode GetTagMode(
            MarkupTagBlockSyntax tagBlock,
            TagHelperBinding bindingResult,
            ErrorSink errorSink)
        {
            var childSpan = tagBlock.GetLastToken()?.Parent;

            // Self-closing tags are always valid despite descriptors[X].TagStructure.
            if (childSpan?.GetContent().EndsWith("/>", StringComparison.Ordinal) ?? false)
            {
                return TagMode.SelfClosing;
            }

            foreach (var descriptor in bindingResult.Descriptors)
            {
                var boundRules = bindingResult.GetBoundRules(descriptor);
                var nonDefaultRule = boundRules.FirstOrDefault(rule => rule.TagStructure != TagStructure.Unspecified);

                if (nonDefaultRule?.TagStructure == TagStructure.WithoutEndTag)
                {
                    return TagMode.StartTagOnly;
                }
            }

            return TagMode.StartTagAndEndTag;
        }
    }
}
