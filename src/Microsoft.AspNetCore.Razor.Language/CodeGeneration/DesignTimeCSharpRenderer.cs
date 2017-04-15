﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language.CodeGeneration
{
    internal class DesignTimeCSharpRenderer : PageStructureCSharpRenderer
    {
        public DesignTimeCSharpRenderer(RuntimeTarget target, CSharpRenderingContext context)
            : base(target, context)
        {
        }

        public override void VisitCSharpExpression(CSharpExpressionIRNode node)
        {
            // We can't remove this yet, because it's still used recursively in a few places.
            if (node.Children.Count == 0)
            {
                return;
            }

            if (node.Source != null)
            {
                using (Context.Writer.BuildLinePragma(node.Source.Value))
                {
                    var offset = RazorDesignTimeIRPass.DesignTimeVariable.Length + " = ".Length;
                    var padding = BuildOffsetPadding(offset, node.Source.Value, Context);

                    Context.Writer
                        .Write(padding)
                        .WriteStartAssignment(RazorDesignTimeIRPass.DesignTimeVariable);

                    for (var i = 0; i < node.Children.Count; i++)
                    {
                        var token = node.Children[i] as RazorIRToken;
                        if (token != null && token.IsCSharp)
                        {
                            Context.AddLineMappingFor(token);
                            Context.Writer.Write(token.Content);
                        }
                        else
                        {
                            // There may be something else inside the expression like a Template or another extension node.
                            Visit(node.Children[i]);
                        }
                    }

                    Context.Writer.WriteLine(";");
                }
            }
            else
            {
                Context.Writer.WriteStartAssignment(RazorDesignTimeIRPass.DesignTimeVariable);
                VisitDefault(node);
                Context.Writer.WriteLine(";");
            }
        }

        public override void VisitUsingStatement(UsingStatementIRNode node)
        {
            if (node.Source.HasValue)
            {
                using (Context.Writer.BuildLinePragma(node.Source.Value))
                {
                    Context.AddLineMappingFor(node);
                    Context.Writer.WriteUsing(node.Content);
                }
            }
            else
            {
                Context.Writer.WriteUsing(node.Content);
            }
        }

        public override void VisitCSharpStatement(CSharpStatementIRNode node)
        {
            // We can't remove this yet, because it's still used recursively in a few places.
            var isWhitespaceStatement = true;
            for (var i = 0; i < node.Children.Count; i++)
            {
                var token = node.Children[i] as RazorIRToken;
                if (token == null || !string.IsNullOrWhiteSpace(token.Content))
                {
                    isWhitespaceStatement = false;
                    break;
                }
            }

            IDisposable linePragmaScope = null;
            if (node.Source != null)
            {
                if (!isWhitespaceStatement)
                {
                    linePragmaScope = Context.Writer.BuildLinePragma(node.Source.Value);
                }

                var padding = BuildOffsetPadding(0, node.Source.Value, Context);
                Context.Writer.Write(padding);
            }
            else if (isWhitespaceStatement)
            {
                // Don't write whitespace if there is no line mapping for it.
                return;
            }

            for (var i = 0; i < node.Children.Count; i++)
            {
                if (node.Children[i] is RazorIRToken token && token.IsCSharp)
                {
                    Context.AddLineMappingFor(token);
                    Context.Writer.Write(token.Content);
                }
                else
                {
                    // There may be something else inside the statement like an extension node.
                    Visit(node.Children[i]);
                }
            }

            if (linePragmaScope != null)
            {
                linePragmaScope.Dispose();
            }
            else
            {
                Context.Writer.WriteLine();
            }
        }

        public override void VisitDirectiveToken(DirectiveTokenIRNode node)
        {
            const string TypeHelper = "__typeHelper";

            var tokenKind = node.Descriptor.Kind;
            if (!node.Source.HasValue ||
                !string.Equals(
                    Context.SourceDocument.FileName,
                    node.Source.Value.FilePath,
                    StringComparison.OrdinalIgnoreCase))
            {
                // We don't want to handle directives from imports.
                return;
            }

            // Wrap the directive token in a lambda to isolate variable names.
            Context.Writer
                .Write("((")
                .Write(typeof(Action).FullName)
                .Write(")(");
            using (Context.Writer.BuildLambda(endLine: false))
            {
                var originalIndent = Context.Writer.CurrentIndent;
                Context.Writer.ResetIndent();
                switch (tokenKind)
                {
                    case DirectiveTokenKind.Type:

                        // {node.Content} __typeHelper = null;

                        Context.AddLineMappingFor(node);
                        Context.Writer
                            .Write(node.Content)
                            .Write(" ")
                            .WriteStartAssignment(TypeHelper)
                            .WriteLine("null;");
                        break;

                    case DirectiveTokenKind.Member:

                        // global::System.Object {node.content} = null;

                        Context.Writer
                            .Write("global::")
                            .Write(typeof(object).FullName)
                            .Write(" ");

                        Context.AddLineMappingFor(node);
                        Context.Writer
                            .Write(node.Content)
                            .WriteLine(" = null;");
                        break;

                    case DirectiveTokenKind.Namespace:

                        // global::System.Object __typeHelper = nameof({node.Content});

                        Context.Writer
                            .Write("global::")
                            .Write(typeof(object).FullName)
                            .Write(" ")
                            .WriteStartAssignment(TypeHelper);

                        Context.Writer.Write("nameof(");

                        Context.AddLineMappingFor(node);
                        Context.Writer
                            .Write(node.Content)
                            .WriteLine(");");
                        break;

                    case DirectiveTokenKind.String:

                        // global::System.Object __typeHelper = "{node.Content}";

                        Context.Writer
                            .Write("global::")
                            .Write(typeof(object).FullName)
                            .Write(" ")
                            .WriteStartAssignment(TypeHelper);

                        if (node.Content.StartsWith("\"", StringComparison.Ordinal))
                        {
                            Context.AddLineMappingFor(node);
                            Context.Writer.Write(node.Content);
                        }
                        else
                        {
                            Context.Writer.Write("\"");
                            Context.AddLineMappingFor(node);
                            Context.Writer
                                .Write(node.Content)
                                .Write("\"");
                        }

                        Context.Writer.WriteLine(";");
                        break;
                }
                Context.Writer.SetIndent(originalIndent);
            }
            Context.Writer.WriteLine("))();");
        }
    }
}
