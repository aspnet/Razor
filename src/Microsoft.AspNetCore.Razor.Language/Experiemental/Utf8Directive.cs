// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
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
            public override IntermediateNodeCollection Children { get; } = IntermediateNodeCollection.ReadOnly;

            public string FieldName { get; set; }

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

        private class Utf8FieldNode : ExtensionIntermediateNode
        {
            public override IntermediateNodeCollection Children { get; } = IntermediateNodeCollection.ReadOnly;

            public string Content { get; set; }

            public string FieldName { get; set; }

            public override void Accept(IntermediateNodeVisitor visitor)
            {
                if (visitor == null)
                {
                    throw new ArgumentNullException(nameof(visitor));
                }

                AcceptExtensionNode<Utf8FieldNode>(this, visitor);
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

                extension.WriteUtf8Field(context, this);
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

                var @class = documentNode.FindPrimaryClass();
                if (@class == null)
                {
                    return;
                }

                // OK, rewrite all of the static content
                var visitor = new Visitor(@class);
                visitor.Visit(documentNode);
            }
        }

        private class Visitor : IntermediateNodeWalker
        {
            private readonly ClassDeclarationIntermediateNode _class;
            private readonly Dictionary<string, Utf8FieldNode> _fields;

            public Visitor(ClassDeclarationIntermediateNode @class)
            {
                _class = @class;

                _fields = new Dictionary<string, Utf8FieldNode>();
            }

            public override void VisitHtml(HtmlContentIntermediateNode node)
            {
                var content = string.Join(string.Empty, node.Children.Cast<IntermediateToken>().Select(t => t.Content));
                if (!_fields.TryGetValue(content, out var field))
                {
                    field = new Utf8FieldNode()
                    {
                        Content = content,
                        FieldName = $"__bytes{_fields.Count}",
                    };

                    _fields.Add(content, field);

                    // Just adding these at the end for now. #YOLO
                    _class.Children.Add(field);
                }

                var index = Parent.Children.IndexOf(node);
                Parent.Children[index] = new Utf8Node()
                {
                    FieldName = field.FieldName,
                };
            }
        }

        private class TargetExtension : ICodeTargetExtension
        {
            public void WriteUtf8(CodeRenderingContext context, Utf8Node node)
            {
                context.CodeWriter.WriteMethodInvocation("WriteLiteral", node.FieldName);
            }

            public void WriteUtf8Field(CodeRenderingContext context, Utf8FieldNode node)
            {
                context.CodeWriter.Write("private static readonly global::System.ReadOnlyMemory<byte> ");
                context.CodeWriter.Write(node.FieldName);
                context.CodeWriter.Write(" ");
                context.CodeWriter.Write(" = new byte[] { ");

                var bytes = Encoding.UTF8.GetBytes(node.Content);
                for (var i = 0; i < bytes.Length; i++)
                {
                    context.CodeWriter.Write(bytes[i].ToString());
                    context.CodeWriter.Write(", ");
                }

                context.CodeWriter.Write(" };");
                context.CodeWriter.WriteLine();
            }
        }
    }
}
