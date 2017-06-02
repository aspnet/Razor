// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions
{
    public class PageAttributeInjectionPass : RazorIRPassBase, IRazorIROptimizationPass
    {
        private const string RazorViewAttribute = "Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute";
        private const string RazorPageAttribute = "Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.RazorPageAttribute";

        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIRNode irDocument)
        {
            var visitor = new Visitor();
            visitor.Visit(irDocument);

            var @namespace = visitor.FirstNamespace;
            if (@namespace == null || string.IsNullOrEmpty(@namespace.Content))
            {
                // No namespace node or it's incomplete. Skip.
                return;
            }

            var @class = visitor.FirstClass;
            if (@class == null || string.IsNullOrEmpty(@class.Name))
            {
                // No class node or it's incomplete. Skip.
                return;
            }

            var generatedTypeName = $"{@namespace.Content}.{@class.Name}";
            var path = codeDocument.GetRelativePath();
            var escapedPath = EscapeAsVerbatimLiteral(path);

            string attribute;
            if (irDocument.DocumentKind == MvcViewDocumentClassifierPass.MvcViewDocumentKind)
            {
                attribute = $"[assembly:{RazorViewAttribute}({escapedPath}, typeof({generatedTypeName}))]";
            }
            else if (irDocument.DocumentKind == RazorPageDocumentClassifierPass.RazorPageDocumentKind &&
                PageDirective.TryGetPageDirective(irDocument, out var pageDirective))
            {
                var escapedRoutePrefix = EscapeAsVerbatimLiteral(pageDirective.RouteTemplate);
                attribute = $"[assembly:{RazorPageAttribute}({escapedPath}, typeof({generatedTypeName}), {escapedRoutePrefix})]";
            }
            else
            {
                return;
            }

            var index = irDocument.Children.IndexOf(visitor.FirstNamespace);
            Debug.Assert(index >= 0);

            var pageAttribute = new CSharpStatementIRNode();
            RazorIRBuilder.Create(pageAttribute)
                .Add(new RazorIRToken()
                {
                    Kind = RazorIRToken.TokenKind.CSharp,
                    Content = attribute,
                });

            irDocument.Children.Insert(index, pageAttribute);
        }

        private static string EscapeAsVerbatimLiteral(string value)
        {
            if (value == null)
            {
                return "null";
            }

            value = value.Replace("\"", "\"\"");
            return $"@\"{value}\"";
        }

        private class Visitor : RazorIRNodeWalker
        {
            public ClassDeclarationIRNode FirstClass { get; private set; }

            public NamespaceDeclarationIRNode FirstNamespace { get; private set; }

            public override void VisitNamespaceDeclaration(NamespaceDeclarationIRNode node)
            {
                if (FirstNamespace == null)
                {
                    FirstNamespace = node;
                }

                base.VisitNamespaceDeclaration(node);
            }

            public override void VisitClassDeclaration(ClassDeclarationIRNode node)
            {
                if (FirstClass == null)
                {
                    FirstClass = node;
                }

                base.VisitClassDeclaration(node);
            }
        }
    }
}
