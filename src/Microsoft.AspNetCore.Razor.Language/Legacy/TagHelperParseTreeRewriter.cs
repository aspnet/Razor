// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal class TagHelperParseTreeRewriter : SyntaxRewriter
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

        private readonly string _tagHelperPrefix;
        private readonly List<KeyValuePair<string, string>> _htmlAttributeTracker;
        private readonly StringBuilder _attributeValueBuilder;
        private readonly TagHelperBinder _tagHelperBinder;
        private readonly Stack<TagBlockTracker> _trackerStack;
        private RazorParserFeatureFlags _featureFlags;
        private ErrorSink _errorSink;

        public TagHelperParseTreeRewriter(
            string tagHelperPrefix,
            IEnumerable<TagHelperDescriptor> descriptors,
            RazorParserFeatureFlags featureFlags)
        {
            _tagHelperPrefix = tagHelperPrefix;
            _tagHelperBinder = new TagHelperBinder(tagHelperPrefix, descriptors);
            _trackerStack = new Stack<TagBlockTracker>();
            _attributeValueBuilder = new StringBuilder();
            _htmlAttributeTracker = new List<KeyValuePair<string, string>>();
            _featureFlags = featureFlags;
        }

        public SyntaxNode Rewrite(SyntaxNode root, ErrorSink errorSink)
        {
            _errorSink = errorSink;

            var rewritten = Visit(root);

            return rewritten;
        }

        public override SyntaxNode VisitMarkupTagBlock(MarkupTagBlockSyntax node)
        {
            return base.VisitMarkupTagBlock(node);
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
    }
}
