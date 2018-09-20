// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultDirectiveSyntaxTreePass : RazorEngineFeatureBase, IRazorSyntaxTreePass
    {
        public int Order => 75;

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
                var legacySectionVerifier = new LegacyNestedSectionVerifier();
                legacySectionVerifier.Verify(syntaxTree);
                return syntaxTree;
            }
            
            var sectionVerifier = new NestedSectionVerifier(syntaxTree);
            return sectionVerifier.Verify();
        }

        private class NestedSectionVerifier : SyntaxRewriter
        {
            private int _nestedLevel;
            private RazorSyntaxTree _syntaxTree;

            public NestedSectionVerifier(RazorSyntaxTree syntaxTree)
            {
                _syntaxTree = syntaxTree;
            }

            public RazorSyntaxTree Verify()
            {
                var root = Visit(_syntaxTree.Root);
                var rewrittenTree = new DefaultRazorSyntaxTree(root, _syntaxTree.Source, _syntaxTree.Diagnostics, _syntaxTree.Options);
                return rewrittenTree;
            }

            public override SyntaxNode VisitRazorDirective(RazorDirectiveSyntax node)
            {
                if (_nestedLevel > 0)
                {
                    var directiveStart = node.Transition.GetSourceLocation(_syntaxTree.Source);
                    var errorLength = /* @ */ 1 + SectionDirective.Directive.Directive.Length;
                    var error = RazorDiagnosticFactory.CreateParsing_SectionsCannotBeNested(new SourceSpan(directiveStart, errorLength));
                    node = node.AppendDiagnostic(error);
                }
                _nestedLevel++;
                var result = base.VisitRazorDirective(node);
                _nestedLevel--;

                return result;
            }
        }

        private class LegacyNestedSectionVerifier : ParserVisitor
        {
            private int _nestedLevel;

            public void Verify(RazorSyntaxTree tree)
            {
                tree.LegacyRoot.Accept(this);
            }

            public override void VisitDirectiveBlock(DirectiveChunkGenerator chunkGenerator, Block block)
            {
                if (_nestedLevel > 0)
                {
                    var directiveStart = block.Children.First(child => !child.IsBlock && ((Span)child).Kind == SpanKindInternal.Transition).Start;
                    var errorLength = /* @ */ 1 + SectionDirective.Directive.Directive.Length;
                    var error = RazorDiagnosticFactory.CreateParsing_SectionsCannotBeNested(new SourceSpan(directiveStart, errorLength));
                    chunkGenerator.Diagnostics.Add(error);
                }

                _nestedLevel++;

                VisitDefault(block);

                _nestedLevel--;
            }
        }
    }
}
