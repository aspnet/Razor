﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public static class RazorCodeDocumentExtensions
    {
        private static object TagHelperPrefixKey = new object();

        public static string GetTagHelperPrefix(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.Items[TagHelperPrefixKey] as string;
        }

        public static void SetTagHelperPrefix(this RazorCodeDocument document, string tagHelperPrefix)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.Items[TagHelperPrefixKey] = tagHelperPrefix;
        }

        public static RazorSyntaxTree GetSyntaxTree(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.Items[typeof(RazorSyntaxTree)] as RazorSyntaxTree;
        }

        public static void SetSyntaxTree(this RazorCodeDocument document, RazorSyntaxTree syntaxTree)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.Items[typeof(RazorSyntaxTree)] = syntaxTree;
        }

        public static IReadOnlyList<RazorSyntaxTree> GetImportSyntaxTrees(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (document.Items[typeof(ImportSyntaxTreesHolder)] as ImportSyntaxTreesHolder)?.SyntaxTrees;
        }

        public static void SetImportSyntaxTrees(this RazorCodeDocument document, IReadOnlyList<RazorSyntaxTree> syntaxTrees)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.Items[typeof(ImportSyntaxTreesHolder)] = new ImportSyntaxTreesHolder(syntaxTrees);
        }

        public static DocumentIRNode GetIRDocument(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return document.Items[typeof(DocumentIRNode)] as DocumentIRNode;
        }

        public static void SetIRDocument(this RazorCodeDocument document, DocumentIRNode irDocument)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.Items[typeof(DocumentIRNode)] = irDocument;
        }

        public static RazorCSharpDocument GetCSharpDocument(this RazorCodeDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            return (RazorCSharpDocument)document.Items[typeof(RazorCSharpDocument)];
        }

        public static void SetCSharpDocument(this RazorCodeDocument document, RazorCSharpDocument csharp)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            document.Items[typeof(RazorCSharpDocument)] = csharp;
        }

        private class ImportSyntaxTreesHolder
        {
            public ImportSyntaxTreesHolder(IReadOnlyList<RazorSyntaxTree> syntaxTrees)
            {
                SyntaxTrees = syntaxTrees;
            }

            public IReadOnlyList<RazorSyntaxTree> SyntaxTrees { get; }
        }

        private class IncludeSyntaxTreesHolder
        {
            public IncludeSyntaxTreesHolder(IReadOnlyList<RazorSyntaxTree> syntaxTrees)
            {
                SyntaxTrees = syntaxTrees;
            }

            public IReadOnlyList<RazorSyntaxTree> SyntaxTrees { get; }
        }
    }
}
