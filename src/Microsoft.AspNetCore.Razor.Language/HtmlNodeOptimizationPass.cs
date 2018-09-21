// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class HtmlNodeOptimizationPass : RazorEngineFeatureBase, IRazorSyntaxTreePass
    {
        public int Order => 100;

        public RazorSyntaxTree Execute(RazorCodeDocument codeDocument, RazorSyntaxTree syntaxTree)
        {
            if (codeDocument == null)
            {
                throw new ArgumentNullException(nameof(codeDocument));
            }

            if (syntaxTree == null)
            {
                throw new ArgumentNullException(nameof(syntaxTree));
            }

            if (syntaxTree is LegacyRazorSyntaxTree)
            {
                return LegacyExecute(codeDocument, syntaxTree);
            }

            var whitespaceRewriter = new WhitespaceRewriter();
            var rewritten = whitespaceRewriter.Visit(syntaxTree.Root);

            var rewrittenSyntaxTree = RazorSyntaxTree.Create(rewritten, syntaxTree.Source, syntaxTree.Diagnostics, syntaxTree.Options);
            return rewrittenSyntaxTree;
        }

        private RazorSyntaxTree LegacyExecute(RazorCodeDocument codeDocument, RazorSyntaxTree syntaxTree)
        {
            var conditionalAttributeCollapser = new LegacyConditionalAttributeCollapser();
            var rewritten = conditionalAttributeCollapser.Rewrite(syntaxTree.LegacyRoot);

            var whitespaceRewriter = new LegacyWhitespaceRewriter();
            rewritten = whitespaceRewriter.Rewrite(rewritten);

            var rewrittenSyntaxTree = RazorSyntaxTree.Create(rewritten, syntaxTree.Source, syntaxTree.Diagnostics, syntaxTree.Options);
            return rewrittenSyntaxTree;
        }
    }
}