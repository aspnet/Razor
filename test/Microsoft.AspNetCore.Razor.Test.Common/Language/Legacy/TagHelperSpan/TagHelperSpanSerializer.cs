// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal class TagHelperSpanSerializer
    {
        internal static string Serialize(SyntaxTreeNode node, string filePath = null)
        {
            if (!(node is Block block))
            {
                return string.Empty;
            }

            using (var writer = new StringWriter())
            {
                var syntaxTree = GetSyntaxTree(block, filePath);
                var visitor = new TagHelperSpanWriter(writer, syntaxTree);
                visitor.Visit();

                return writer.ToString();
            }
        }

        internal static string Serialize(RazorSyntaxTree syntaxTree)
        {
            using (var writer = new StringWriter())
            {
                var visitor = new TagHelperSpanWriter(writer, syntaxTree);
                visitor.Visit();

                return writer.ToString();
            }
        }

        private static RazorSyntaxTree GetSyntaxTree(Block root, string filePath)
        {
            return RazorSyntaxTree.Create(
                root,
                TestRazorSourceDocument.Create(filePath: filePath),
                Array.Empty<RazorDiagnostic>(),
                RazorParserOptions.CreateDefault());
        }
    }
}
