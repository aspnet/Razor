// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language.Syntax
{
    internal static class SyntaxNodeExtensions
    {
        public static TNode WithAnnotations<TNode>(this TNode node, params SyntaxAnnotation[] annotations) where TNode : SyntaxNode
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return (TNode)node.Green.SetAnnotations(annotations).CreateRed(node.Parent, node.Position);
        }

        public static object GetAnnotationValue<TNode>(this TNode node, string key) where TNode : SyntaxNode
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var annotation = node.GetAnnotations().FirstOrDefault(n => n.Kind == key);
            return annotation?.Data;
        }

        public static TNode WithDiagnostics<TNode>(this TNode node, params RazorDiagnostic[] diagnostics) where TNode : SyntaxNode
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            return (TNode)node.Green.SetDiagnostics(diagnostics).CreateRed(node.Parent, node.Position);
        }

        public static TNode AppendDiagnostic<TNode>(this TNode node, params RazorDiagnostic[] diagnostics) where TNode : SyntaxNode
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            var existingDiagnostics = node.GetDiagnostics();
            var allDiagnostics = existingDiagnostics.Concat(diagnostics).ToArray();

            return (TNode)node.WithDiagnostics(allDiagnostics);
        }

        public static SourceLocation GetSourceLocation(this SyntaxNode node, RazorSourceDocument source)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            return source.Lines.GetLocation(node.Position);
        }

        public static SourceSpan GetSourceSpan(this SyntaxNode node, RazorSourceDocument source)
        {
            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }
            if (source == null)
            {
                throw new ArgumentNullException(nameof(source));
            }

            var location = node.GetSourceLocation(source);

            return new SourceSpan(location, node.FullWidth);
        }
    }
}
