// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language.CodeGeneration
{
    internal class RuntimeCSharpRenderer : PageStructureCSharpRenderer
    {
        public RuntimeCSharpRenderer(RuntimeTarget target, CSharpRenderingContext context)
            : base(target, context)
        {
        }

        public override void VisitHtml(HtmlContentIRNode node)
        {
            // We can't remove this yet, because it's still used recursively in a few places.
            const int MaxStringLiteralLength = 1024;

            var builder = new StringBuilder();
            for (var i = 0; i < node.Children.Count; i++)
            {
                if (node.Children[i] is RazorIRToken token && token.IsHtml)
                {
                    builder.Append(token.Content);
                }
            }

            var content = builder.ToString();

            var charactersConsumed = 0;

            // Render the string in pieces to avoid Roslyn OOM exceptions at compile time: https://github.com/aspnet/External/issues/54
            while (charactersConsumed < content.Length)
            {
                string textToRender;
                if (content.Length <= MaxStringLiteralLength)
                {
                    textToRender = content;
                }
                else
                {
                    var charactersToSubstring = Math.Min(MaxStringLiteralLength, content.Length - charactersConsumed);
                    textToRender = content.Substring(charactersConsumed, charactersToSubstring);
                }

                Context.Writer
                    .Write(Context.RenderingConventions.StartWriteLiteralMethod)
                    .WriteStringLiteral(textToRender)
                    .WriteEndMethodInvocation();

                charactersConsumed += textToRender.Length;
            }
        }

        public override void VisitCSharpExpression(CSharpExpressionIRNode node)
        {
            // We can't remove this yet, because it's still used recursively in a few places.
            IDisposable linePragmaScope = null;
            if (node.Source != null)
            {
                linePragmaScope = Context.Writer.BuildLinePragma(node.Source.Value);
                var padding = BuildOffsetPadding(Context.RenderingConventions.StartWriteMethod.Length, node.Source.Value, Context);
                Context.Writer.Write(padding);
            }

            Context.Writer.Write(Context.RenderingConventions.StartWriteMethod);

            for (var i = 0; i < node.Children.Count; i++)
            {
                if (node.Children[i] is RazorIRToken token && token.IsCSharp)
                {
                    Context.Writer.Write(token.Content);
                }
                else
                {
                    // There may be something else inside the expression like a Template or another extension node.
                    Visit(node.Children[i]);
                }
            }

            Context.Writer.WriteEndMethodInvocation();

            linePragmaScope?.Dispose();
        }

        public override void VisitHtmlAttribute(HtmlAttributeIRNode node)
        {
            var valuePieceCount = node
                .Children
                .Count(child => child is HtmlAttributeValueIRNode || child is CSharpAttributeValueIRNode);
            var prefixLocation = node.Source.Value.AbsoluteIndex;
            var suffixLocation = node.Source.Value.AbsoluteIndex + node.Source.Value.Length - node.Suffix.Length;
            Context.Writer
                .Write(Context.RenderingConventions.StartBeginWriteAttributeMethod)
                .WriteStringLiteral(node.Name)
                .WriteParameterSeparator()
                .WriteStringLiteral(node.Prefix)
                .WriteParameterSeparator()
                .Write(prefixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteStringLiteral(node.Suffix)
                .WriteParameterSeparator()
                .Write(suffixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(valuePieceCount.ToString(CultureInfo.InvariantCulture))
                .WriteEndMethodInvocation();

            VisitDefault(node);

            Context.Writer
                .Write(Context.RenderingConventions.StartEndWriteAttributeMethod)
                .WriteEndMethodInvocation();
        }

        public override void VisitHtmlAttributeValue(HtmlAttributeValueIRNode node)
        {
            var prefixLocation = node.Source.Value.AbsoluteIndex;
            var valueLocation = node.Source.Value.AbsoluteIndex + node.Prefix.Length;
            var valueLength = node.Source.Value.Length;
            Context.Writer
                .Write(Context.RenderingConventions.StartWriteAttributeValueMethod)
                .WriteStringLiteral(node.Prefix)
                .WriteParameterSeparator()
                .Write(prefixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteStringLiteral(node.Content)
                .WriteParameterSeparator()
                .Write(valueLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(valueLength.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteBooleanLiteral(true)
                .WriteEndMethodInvocation();
        }

        public override void VisitCSharpAttributeValue(CSharpAttributeValueIRNode node)
        {
            const string ValueWriterName = "__razor_attribute_value_writer";

            var expressionValue = node.Children.FirstOrDefault() as CSharpExpressionIRNode;
            var linePragma = expressionValue != null ? Context.Writer.BuildLinePragma(node.Source.Value) : null;
            var prefixLocation = node.Source.Value.AbsoluteIndex;
            var valueLocation = node.Source.Value.AbsoluteIndex + node.Prefix.Length;
            var valueLength = node.Source.Value.Length - node.Prefix.Length;
            Context.Writer
                .Write(Context.RenderingConventions.StartWriteAttributeValueMethod)
                .WriteStringLiteral(node.Prefix)
                .WriteParameterSeparator()
                .Write(prefixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator();

            if (expressionValue != null)
            {
                Debug.Assert(node.Children.Count == 1);

                RenderExpressionInline(expressionValue, Context);
            }
            else
            {
                // Not an expression; need to buffer the result.
                Context.Writer.WriteStartNewObject("Microsoft.AspNetCore.Mvc.Razor.HelperResult" /* ORIGINAL: TemplateTypeName */);

                var initialRenderingConventions = Context.RenderingConventions;
                Context.RenderingConventions = new CSharpRedirectRenderingConventions(ValueWriterName, Context.Writer);
                using (Context.Writer.BuildAsyncLambda(endLine: false, parameterNames: ValueWriterName))
                {
                    VisitDefault(node);
                }
                Context.RenderingConventions = initialRenderingConventions;

                Context.Writer.WriteEndMethodInvocation(false);
            }

            Context.Writer
                .WriteParameterSeparator()
                .Write(valueLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(valueLength.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteBooleanLiteral(false)
                .WriteEndMethodInvocation();

            linePragma?.Dispose();
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

            if (isWhitespaceStatement)
            {
                return;
            }

            IDisposable linePragmaScope = null;
            if (node.Source != null)
            {
                linePragmaScope = Context.Writer.BuildLinePragma(node.Source.Value);
                var padding = BuildOffsetPadding(0, node.Source.Value, Context);
                Context.Writer.Write(padding);
            }

            for (var i = 0; i < node.Children.Count; i++)
            {
                if (node.Children[i] is RazorIRToken token && token.IsCSharp)
                {
                    Context.Writer.Write(token.Content);
                }
                else
                {
                    // There may be something else inside the statement like an extension node.
                    Visit(node.Children[i]);
                }
            }

            if (linePragmaScope == null)
            {
                Context.Writer.WriteLine();
            }

            linePragmaScope?.Dispose();
        }

        public override void VisitAddPreallocatedTagHelperHtmlAttribute(AddPreallocatedTagHelperHtmlAttributeIRNode node)
        {
            Context.Writer
                .WriteStartInstanceMethodInvocation(
                    "__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */,
                    "AddHtmlAttribute" /* ORIGINAL: ExecutionContextAddHtmlAttributeMethodName */)
                .Write(node.VariableName)
                .WriteEndMethodInvocation();
        }

        public override void VisitSetPreallocatedTagHelperProperty(SetPreallocatedTagHelperPropertyIRNode node)
        {
            var tagHelperVariableName = GetTagHelperVariableName(node.TagHelperTypeName);
            var propertyValueAccessor = GetTagHelperPropertyAccessor(node.IsIndexerNameMatch, tagHelperVariableName, node.AttributeName, node.Descriptor);
            var attributeValueAccessor = $"{node.VariableName}.Value" /* ORIGINAL: TagHelperAttributeValuePropertyName */;
            Context.Writer
                .WriteStartAssignment(propertyValueAccessor)
                .Write("(string)")
                .Write(attributeValueAccessor)
                .WriteLine(";")
                .WriteStartInstanceMethodInvocation(
                    "__tagHelperExecutionContext" /* ORIGINAL: ExecutionContextVariableName */,
                    "AddTagHelperAttribute" /* ORIGINAL: ExecutionContextAddTagHelperAttributeMethodName */)
                .Write(node.VariableName)
                .WriteEndMethodInvocation();
        }

        public override void VisitDeclarePreallocatedTagHelperHtmlAttribute(DeclarePreallocatedTagHelperHtmlAttributeIRNode node)
        {
            Context.Writer
                .Write("private static readonly global::")
                .Write("Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute" /* ORIGINAL: TagHelperAttributeTypeName */)
                .Write(" ")
                .Write(node.VariableName)
                .Write(" = ")
                .WriteStartNewObject("global::" + "Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute" /* ORIGINAL: TagHelperAttributeTypeName */)
                .WriteStringLiteral(node.Name);

            if (node.ValueStyle == HtmlAttributeValueStyle.Minimized)
            {
                Context.Writer.WriteEndMethodInvocation();
            }
            else
            {
                Context.Writer
                    .WriteParameterSeparator()
                    .WriteStartNewObject("global::" + "Microsoft.AspNetCore.Html.HtmlString" /* ORIGINAL: EncodedHtmlStringTypeName */)
                    .WriteStringLiteral(node.Value)
                    .WriteEndMethodInvocation(endLine: false)
                    .WriteParameterSeparator()
                    .Write($"global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.{node.ValueStyle}")
                    .WriteEndMethodInvocation();
            }
        }

        public override void VisitDeclarePreallocatedTagHelperAttribute(DeclarePreallocatedTagHelperAttributeIRNode node)
        {
            Context.Writer
                .Write("private static readonly global::")
                .Write("Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute" /* ORIGINAL: TagHelperAttributeTypeName */)
                .Write(" ")
                .Write(node.VariableName)
                .Write(" = ")
                .WriteStartNewObject("global::" + "Microsoft.AspNetCore.Razor.TagHelpers.TagHelperAttribute" /* ORIGINAL: TagHelperAttributeTypeName */)
                .WriteStringLiteral(node.Name)
                .WriteParameterSeparator()
                .WriteStringLiteral(node.Value)
                .WriteParameterSeparator()
                .Write($"global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.{node.ValueStyle}")
                .WriteEndMethodInvocation();
        }
    }
}
