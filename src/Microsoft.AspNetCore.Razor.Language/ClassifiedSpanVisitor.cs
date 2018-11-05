// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
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
            return WriteBlock(node, BlockKindInternal.Comment, razorCommentSyntax =>
            {
                WriteSpan(razorCommentSyntax.StartCommentTransition, SpanKindInternal.Transition, AcceptedCharactersInternal.None);
                WriteSpan(razorCommentSyntax.StartCommentStar, SpanKindInternal.MetaCode, AcceptedCharactersInternal.None);

                var comment = razorCommentSyntax.Comment;
                if (comment.IsMissing)
                {
                    // We need to generate a classified span at this position. So insert a marker in its place.
                    comment = (SyntaxToken)SyntaxFactory.Token(SyntaxKind.Marker, string.Empty).Green.CreateRed(razorCommentSyntax, razorCommentSyntax.StartCommentStar.EndPosition);
                }
                WriteSpan(comment, SpanKindInternal.Comment, AcceptedCharactersInternal.Any);

                WriteSpan(razorCommentSyntax.EndCommentStar, SpanKindInternal.MetaCode, AcceptedCharactersInternal.None);
                WriteSpan(razorCommentSyntax.EndCommentTransition, SpanKindInternal.Transition, AcceptedCharactersInternal.None);

                return razorCommentSyntax;
            });
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

        public override SyntaxNode VisitGenericBlock(GenericBlockSyntax node)
        {
            if (!(node.Parent is MarkupTagHelperAttributeValueSyntax) &&
                node.FirstAncestorOrSelf<SyntaxNode>(n => n is MarkupTagHelperAttributeValueSyntax) != null)
            {
                return WriteBlock(node, BlockKindInternal.Expression, base.VisitGenericBlock);
            }

            return base.VisitGenericBlock(node);
        }

        public override SyntaxNode VisitMarkupTagHelperAttributeValue(MarkupTagHelperAttributeValueSyntax node)
        {
            // We don't generate a classified span when the attribute value is a simple literal value.
            if (node.Children.Count > 1 ||
                (node.Children.Count == 1 && node.Children[0] is MarkupDynamicAttributeValueSyntax))
            {
                return WriteBlock(node, BlockKindInternal.Markup, base.VisitMarkupTagHelperAttributeValue);
            }

            return base.VisitMarkupTagHelperAttributeValue(node);
        }

        public override SyntaxNode VisitMarkupTagBlock(MarkupTagBlockSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Tag, base.VisitMarkupTagBlock);
        }

        public override SyntaxNode VisitMarkupTagHelperElement(MarkupTagHelperElementSyntax node)
        {
            return WriteBlock(node, BlockKindInternal.Tag, base.VisitMarkupTagHelperElement);
        }

        public override SyntaxNode VisitMarkupTagHelperStartTag(MarkupTagHelperStartTagSyntax node)
        {
            var rewritten = new List<RazorSyntaxNode>();
            foreach (var child in node.Children)
            {
                if (child is MarkupTagHelperAttributeSyntax attribute)
                {
                    var rewrittenAttribute = (MarkupTagHelperAttributeSyntax)Visit(attribute);
                    rewritten.Add(rewrittenAttribute);
                }
                else
                {
                    rewritten.Add(child);
                }
            }

            return node.Update(new SyntaxList<RazorSyntaxNode>(rewritten));
        }

        public override SyntaxNode VisitMarkupTagHelperEndTag(MarkupTagHelperEndTagSyntax node)
        {
            return node;
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

        public override SyntaxNode VisitMarkupTagHelperAttribute(MarkupTagHelperAttributeSyntax node)
        {
            var rewrittenValue = (MarkupTagHelperAttributeValueSyntax)Visit(node.Value);

            return node.Update(node.NamePrefix, node.Name, node.NameSuffix, node.EqualsToken, node.ValuePrefix, rewrittenValue, node.ValueSuffix);
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

            var spanSource = node.GetSourceSpan(_source);
            var blockSource = _currentBlock.GetSourceSpan(_source);
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
    }
}
