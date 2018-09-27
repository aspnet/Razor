// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class ClassifiedSpanVisitor : SyntaxRewriter
    {
        private RazorSourceDocument _source;
        private List<ClassifiedSpanInternal> _spans;
        private BlockKindInternal _currentBlockKind;
        private SyntaxNode _currentBlock;

        public ClassifiedSpanVisitor(RazorSourceDocument source)
        {
            _source = source;
            _spans = new List<ClassifiedSpanInternal>();
            _currentBlockKind = BlockKindInternal.Markup;
        }

        public IReadOnlyList<ClassifiedSpanInternal> ClassifiedSpans => _spans;

        public override SyntaxNode VisitRazorCommentBlock(RazorCommentBlockSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Comment, base.VisitRazorCommentBlock);
        }

        public override SyntaxNode VisitToken(SyntaxToken token)
        {
            if (token.Parent is RazorCommentBlockSyntax)
            {
                if (token.Kind == SyntaxKind.RazorCommentTransition)
                {
                    WriteSpan(token, SpanKindInternal.Transition, AcceptedCharactersInternal.None);
                }
                else if (token.Kind == SyntaxKind.RazorCommentStar)
                {
                    WriteSpan(token, SpanKindInternal.MetaCode, AcceptedCharactersInternal.None);
                }
                else if (token.Kind == SyntaxKind.RazorCommentLiteral)
                {
                    WriteSpan(token, SpanKindInternal.Comment, AcceptedCharactersInternal.Any);
                }
            }

            return base.VisitToken(token);
        }

        public override SyntaxNode VisitCSharpCodeBlock(CSharpCodeBlockSyntax node)
        {
            if (node.Parent is CSharpStatementBodySyntax ||
                node.Parent is CSharpExplicitExpressionBodySyntax ||
                node.Parent is CSharpImplicitExpressionBodySyntax ||
                node.Parent is RazorDirectiveBodySyntax ||
                (_currentBlockKind == BlockKindInternal.Directive &&
                node.Children.Count == 1 &&
                node.Children[0] is CSharpStatementLiteralSyntax))
            {
                return base.VisitCSharpCodeBlock(node);
            }

            return WriteBlock(node, BlockKindInternal.Statement, base.VisitCSharpCodeBlock);
        }

        public override SyntaxNode VisitCSharpStatement(CSharpStatementSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Statement, base.VisitCSharpStatement);
        }

        public override SyntaxNode VisitCSharpExplicitExpression(CSharpExplicitExpressionSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Expression, base.VisitCSharpExplicitExpression);
        }

        public override SyntaxNode VisitCSharpImplicitExpression(CSharpImplicitExpressionSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Expression, base.VisitCSharpImplicitExpression);
        }

        public override SyntaxNode VisitRazorDirective(RazorDirectiveSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Directive, base.VisitRazorDirective);
        }

        public override SyntaxNode VisitCSharpTemplateBlock(CSharpTemplateBlockSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Template, base.VisitCSharpTemplateBlock);
        }

        public override SyntaxNode VisitMarkupBlock(MarkupBlockSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Markup, base.VisitMarkupBlock);
        }

        public override SyntaxNode VisitMarkupTagBlock(MarkupTagBlockSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Tag, base.VisitMarkupTagBlock);
        }

        public override SyntaxNode VisitMarkupAttributeBlock(MarkupAttributeBlockSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Markup, n =>
            {
                var equalsSyntax = SyntaxFactory.MarkupTextLiteral(new SyntaxList<SyntaxToken>(node.EqualsToken));
                var mergedAttributePrefix = MergeTextLiteralSpans(node.NamePrefix, node.Name, node.NameSuffix, equalsSyntax, node.ValuePrefix);
                Visit(mergedAttributePrefix);
                Visit(node.Value);
                Visit(node.ValueSuffix);

                return n;
            });
        }

        public override SyntaxNode VisitMarkupMinimizedAttributeBlock(MarkupMinimizedAttributeBlockSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Markup, n =>
            {
                var mergedAttributePrefix = MergeTextLiteralSpans(node.NamePrefix, node.Name);
                Visit(mergedAttributePrefix);

                return n;
            });
        }

        public override SyntaxNode VisitMarkupCommentBlock(MarkupCommentBlockSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.HtmlComment, base.VisitMarkupCommentBlock);
        }

        public override SyntaxNode VisitMarkupDynamicAttributeValue(MarkupDynamicAttributeValueSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Markup, base.VisitMarkupDynamicAttributeValue);
        }

        public override SyntaxNode VisitRazorMetaCode(RazorMetaCodeSyntax node)
        {
            WriteSpan(node, SpanKindInternal.MetaCode);
            return base.VisitRazorMetaCode(node);
        }

        public override SyntaxNode VisitCSharpTransition(CSharpTransitionSyntax node)
        {
            WriteSpan(node, SpanKindInternal.Transition);
            return base.VisitCSharpTransition(node);
        }

        public override SyntaxNode VisitMarkupTransition(MarkupTransitionSyntax node)
        {
            WriteSpan(node, SpanKindInternal.Transition);
            return base.VisitMarkupTransition(node);
        }

        public override SyntaxNode VisitCSharpStatementLiteral(CSharpStatementLiteralSyntax node)
        {
            WriteSpan(node, SpanKindInternal.Code);
            return base.VisitCSharpStatementLiteral(node);
        }

        public override SyntaxNode VisitCSharpExpressionLiteral(CSharpExpressionLiteralSyntax node)
        {
            WriteSpan(node, SpanKindInternal.Code);
            return base.VisitCSharpExpressionLiteral(node);
        }

        public override SyntaxNode VisitCSharpEphemeralTextLiteral(CSharpEphemeralTextLiteralSyntax node)
        {
            WriteSpan(node, SpanKindInternal.Code);
            return base.VisitCSharpEphemeralTextLiteral(node);
        }

        public override SyntaxNode VisitUnclassifiedTextLiteral(UnclassifiedTextLiteralSyntax node)
        {
            WriteSpan(node, SpanKindInternal.None);
            return base.VisitUnclassifiedTextLiteral(node);
        }

        public override SyntaxNode VisitMarkupLiteralAttributeValue(MarkupLiteralAttributeValueSyntax node)
        {
            WriteSpan(node, SpanKindInternal.Markup);
            return base.VisitMarkupLiteralAttributeValue(node);
        }

        public override SyntaxNode VisitMarkupTextLiteral(MarkupTextLiteralSyntax node)
        {
            if (node.Parent is MarkupLiteralAttributeValueSyntax)
            {
                return base.VisitMarkupTextLiteral(node);
            }

            WriteSpan(node, SpanKindInternal.Markup);
            return base.VisitMarkupTextLiteral(node);
        }

        public override SyntaxNode VisitMarkupEphemeralTextLiteral(MarkupEphemeralTextLiteralSyntax node)
        {
            WriteSpan(node, SpanKindInternal.Markup);
            return base.VisitMarkupEphemeralTextLiteral(node);
        }

        private SyntaxNode WriteBlock<TNode>(TNode node, BlockKindInternal kind, Func<TNode, SyntaxNode> handler) where TNode : SyntaxNode
        {
            var previousBlock = _currentBlock;
            var previousKind = _currentBlockKind;

            _currentBlock = node;
            _currentBlockKind = kind;

            var result = handler(node);

            _currentBlock = previousBlock;
            _currentBlockKind = previousKind;

            return result;
        }

        private void WriteSpan(SyntaxNode node, SpanKindInternal kind, AcceptedCharactersInternal? acceptedCharacters = null)
        {
            if (node.IsMissing)
            {
                return;
            }

            var spanSource = GetSourceSpanForNode(node);
            var blockSource = GetSourceSpanForNode(_currentBlock);
            if (!acceptedCharacters.HasValue)
            {
                acceptedCharacters = AcceptedCharactersInternal.Any;
                var context = node.GetSpanContext();
                if (context != null)
                {
                    acceptedCharacters = context.EditHandler.AcceptedCharacters;
                }
            }

            var span = new ClassifiedSpanInternal(spanSource, blockSource, kind, _currentBlockKind, acceptedCharacters.Value);
            _spans.Add(span);
        }

        private MarkupTextLiteralSyntax MergeTextLiteralSpans(params MarkupTextLiteralSyntax[] literalSyntaxes)
        {
            if (literalSyntaxes == null || literalSyntaxes.Length == 0)
            {
                return null;
            }

            SyntaxNode parent = null;
            var position = 0;
            var seenFirstLiteral = false;
            var builder = Syntax.InternalSyntax.SyntaxListBuilder.Create();

            foreach (var syntax in literalSyntaxes)
            {
                if (syntax == null)
                {
                    continue;
                }
                else if (!seenFirstLiteral)
                {
                    // Set the parent and position of the merged literal to the value of the first non-null literal.
                    parent = syntax.Parent;
                    position = syntax.Position;
                    seenFirstLiteral = true;
                }

                foreach (var token in syntax.LiteralTokens)
                {
                    builder.Add(token.Green);
                }
            }

            var mergedLiteralSyntax = Syntax.InternalSyntax.SyntaxFactory.MarkupTextLiteral(
                builder.ToList<Syntax.InternalSyntax.SyntaxToken>());

            return (MarkupTextLiteralSyntax)mergedLiteralSyntax.CreateRed(parent, position);
        }

        private SourceSpan GetSourceSpanForNode(SyntaxNode node)
        {
            try
            {
                if (_source.Length == 0)
                {
                    // Just a marker symbol
                    return new SourceSpan(_source.FilePath, 0, 0, 0, node.FullWidth);
                }
                if (node.Position >= _source.Length)
                {
                    // E.g. Marker symbol at the end of the document
                    var lastLocation = _source.Lines.GetLocation(_source.Length - 1);
                    return new SourceSpan(
                        lastLocation.FilePath,
                        lastLocation.AbsoluteIndex + 1,
                        lastLocation.LineIndex,
                        lastLocation.CharacterIndex + 1,
                        node.FullWidth);
                }

                return node.GetSourceSpan(_source);
            }
            catch (IndexOutOfRangeException)
            {
                Debug.Assert(false, "Node position should stay within document length.");
                return new SourceSpan(_source.FilePath, node.Position, 0, 0, node.FullWidth);
            }
        }
    }
}
