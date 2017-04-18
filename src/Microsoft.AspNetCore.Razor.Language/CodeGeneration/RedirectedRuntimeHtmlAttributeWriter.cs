﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Globalization;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Razor.Language.CodeGeneration
{
    internal class RedirectedRuntimeHtmlAttributeWriter : RuntimeHtmlAttributeWriter
    {
        private readonly string _textWriter;

        public RedirectedRuntimeHtmlAttributeWriter(string textWriter)
        {
            _textWriter = textWriter;
        }

        public override string WriteAttributeValueMethod { get; set; } = "WriteAttributeValueTo";

        public override void WriteHtmlAttributeValue(CSharpRenderingContext context, HtmlAttributeValueIRNode node)
        {
            var prefixLocation = node.Source.Value.AbsoluteIndex;
            var valueLocation = node.Source.Value.AbsoluteIndex + node.Prefix.Length;
            var valueLength = node.Source.Value.Length;
            context.Writer
                .WriteStartMethodInvocation(WriteAttributeValueMethod)
                .Write(_textWriter)
                .WriteParameterSeparator()
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

        public override void WriteCSharpAttributeValue(CSharpRenderingContext context, CSharpAttributeValueIRNode node)
        {
            const string ValueWriterName = "__razor_attribute_value_writer";

            var expressionValue = node.Children.FirstOrDefault() as CSharpExpressionIRNode;
            var linePragma = expressionValue != null ? context.Writer.BuildLinePragma(node.Source.Value) : null;
            var prefixLocation = node.Source.Value.AbsoluteIndex;
            var valueLocation = node.Source.Value.AbsoluteIndex + node.Prefix.Length;
            var valueLength = node.Source.Value.Length - node.Prefix.Length;
            context.Writer
                .WriteStartMethodInvocation(WriteAttributeValueMethod)
                .WriteStringLiteral(node.Prefix)
                .WriteParameterSeparator()
                .Write(prefixLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator();

            if (expressionValue != null)
            {
                Debug.Assert(node.Children.Count == 1);

                RenderExpressionInline(context, expressionValue);
            }
            else
            {
                // Not an expression; need to buffer the result.
                context.Writer.WriteStartNewObject(TemplateTypeName);

                using (context.Push(new RedirectedRuntimeBasicWriter(ValueWriterName)))
                using (context.Writer.BuildAsyncLambda(endLine: false, parameterNames: ValueWriterName))
                {
                    context.RenderChildren(node);
                }

                context.Writer.WriteEndMethodInvocation(false);
            }

            context.Writer
                .WriteParameterSeparator()
                .Write(valueLocation.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .Write(valueLength.ToString(CultureInfo.InvariantCulture))
                .WriteParameterSeparator()
                .WriteBooleanLiteral(false)
                .WriteEndMethodInvocation();

            linePragma?.Dispose();
        }
    }
}
