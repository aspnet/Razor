// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace Microsoft.AspNetCore.Razor.Language
{
    public abstract class RazorSyntaxTree
    {
        internal static RazorSyntaxTree Create(
            Block root,
            RazorSourceDocument source,
            IEnumerable<RazorDiagnostic> diagnostics,
            RazorParserOptions options)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new LegacyRazorSyntaxTree(root, source, new List<RazorDiagnostic>(diagnostics), options);
        }

        internal static RazorSyntaxTree Create(
            SyntaxNode root,
            RazorSourceDocument source,
            IEnumerable<RazorDiagnostic> diagnostics,
            RazorParserOptions options)
        {
            if (root == null)
            {
                throw new ArgumentNullException(nameof(root));
            }

            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            if (diagnostics == null)
            {
                throw new ArgumentNullException(nameof(diagnostics));
            }

            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            return new DefaultRazorSyntaxTree(root, source, new List<RazorDiagnostic>(diagnostics), options);
        }

        public static RazorSyntaxTree Parse(RazorSourceDocument source, bool legacy = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return Parse(source, options: null, legacy: legacy);
        }

        public static RazorSyntaxTree Parse(RazorSourceDocument source, RazorParserOptions options, bool legacy = false)
        {
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }
            
            var parser = new RazorParser(options ?? RazorParserOptions.CreateDefault());
            return parser.Parse(source, legacy);
        }

        public abstract IReadOnlyList<RazorDiagnostic> Diagnostics { get; }

        public abstract RazorParserOptions Options { get; }

        internal abstract Block LegacyRoot { get; }

        internal virtual SyntaxNode Root { get; }

        public abstract RazorSourceDocument Source { get; }
    }
}
