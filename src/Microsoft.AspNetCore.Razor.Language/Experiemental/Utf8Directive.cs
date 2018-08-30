// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Razor.Language.Experiemental
{
    public static class Utf8Directive
    {
        public static readonly DirectiveDescriptor Directive = DirectiveDescriptor.CreateDirective(
            "utfeight",
            DirectiveKind.SingleLine,
            builder =>
            {
                builder.Usage = DirectiveUsage.FileScopedSinglyOccurring;
                builder.Description = "does utf8 (duh)";
            });

        public static RazorProjectEngineBuilder Register(RazorProjectEngineBuilder builder)
        {
            if (builder == null)
            {
                throw new ArgumentNullException(nameof(builder));
            }

            builder.AddDirective(Directive);
            builder.Features.Add(new Pass());
            builder.AddTargetExtension(new TargetExtension());
            return builder;
        }

        private class Utf8Node : ExtensionIntermediateNode
        {
            public Utf8Node(HtmlContentIntermediateNode node)
            {
                for (var i = 0; i < node.Children.Count; i++)
                {
                    Children.Add(node.Children[i]);
                }
            }

            public override IntermediateNodeCollection Children { get; } = new IntermediateNodeCollection();

            public override void Accept(IntermediateNodeVisitor visitor)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                AcceptExtensionNode<Utf8Node>(this, visitor);
            }

            public override void WriteNode(CodeTarget target, CodeRenderingContext context)
            {
                if (target == null)
                {
                    throw new ArgumentNullException(nameof(target));
                }

                if (context == null)
                {
                    throw new ArgumentNullException(nameof(context));
                }

                var extension = target.GetExtension<TargetExtension>();
                if (extension == null)
                {
                    ReportMissingCodeTargetExtension<TargetExtension>(context);
                    return;
                }

                extension.WriteUtf8(context, this);
            }
        }

        private class Pass : IntermediateNodePassBase, IRazorOptimizationPass
        {
            // Runs before directive removal
            public override int Order => DefaultFeatureOrder + 49;

            protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIntermediateNode documentNode)
            {
                if (documentNode.Options.DesignTime)
                {
                    return;
                }

                var directives = documentNode.FindDirectiveReferences(Directive);
                if (directives.Count == 0)
                {
                    // Not utf8-ized
                    return;
                }

                // OK, rewrite all of the static content
                var visitor = new Visitor();
                visitor.Visit(documentNode);
            }
        }

        private class Visitor : IntermediateNodeWalker
        {
            public override void VisitHtml(HtmlContentIntermediateNode node)
            {
                var index = Parent.Children.IndexOf(node);
                Parent.Children[index] = new Utf8Node(node);
            }
        }

        private class TargetExtension : ICodeTargetExtension
        {
            private readonly StringBuilder _builder = new StringBuilder();
            private int _count;

            public void WriteUtf8(CodeRenderingContext context, Utf8Node node)
            {
                _builder.Clear();

                for (var i = 0; i < node.Children.Count; i++)
                {
                    var token = (IntermediateToken)node.Children[i];
                    _builder.Append(token.Content);
                }

                var text = _builder.ToString();
                var bytes = Encoding.UTF8.GetBytes(text);

                var index = Interlocked.Increment(ref _count);

                context.CodeWriter.WriteVariableDeclaration(
                    "System.ReadOnlySpan<byte>",
                    $"__bytes{index}",
                    $"new byte[]{{{string.Join(", ", bytes)}}}");
                context.CodeWriter.WriteMethodInvocation("WriteLiteral", $"__bytes{index}");
            }
        }
    }
}
