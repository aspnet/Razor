﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Globalization;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    internal class DefaultInstrumentationPass : RazorIRPassBase
    {
        public override int Order => RazorIRPass.DefaultLoweringOrder;

        public override DocumentIRNode ExecuteCore(RazorCodeDocument codeDocument, DocumentIRNode irDocument)
        {
            var walker = new Visitor();
            walker.VisitDocument(irDocument);

            for (var i = 0; i < walker.Items.Count; i++)
            {
                var node = walker.Items[i];
     
                AddInstrumentation(node);
            }

            return irDocument;
        }

        private static void AddInstrumentation(InstrumentationItem item)
        {
            var beginContextMethodName = "Instrumentation.BeginContext"; /* ORIGINAL: BeginContextMethodName */
            var endContextMethodName = "Instrumentation.EndContext"; /* ORIGINAL: EndContextMethodName */

            var beginNode = new CSharpStatementIRNode()
            {
                Content = string.Format("{0}({1}, {2}, {3});",
                    beginContextMethodName,
                    item.Source.AbsoluteIndex.ToString(CultureInfo.InvariantCulture),
                    item.Source.Length.ToString(CultureInfo.InvariantCulture),
                    item.IsLiteral ? "true" : "false"),
                Parent = item.Node.Parent
            };

            var endNode = new CSharpStatementIRNode()
            {
                Content = string.Format("{0}();", endContextMethodName),
                Parent = item.Node.Parent
            };

            var nodeIndex = item.Node.Parent.Children.IndexOf(item.Node);
            item.Node.Parent.Children.Insert(nodeIndex, beginNode);
            item.Node.Parent.Children.Insert(nodeIndex + 2, endNode);
        }

        private struct InstrumentationItem
        {
            public InstrumentationItem(RazorIRNode node, bool isLiteral, SourceSpan source)
            {
                Node = node;
                IsLiteral = isLiteral;
                Source = source;
            }

            public RazorIRNode Node { get; }

            public bool IsLiteral { get; }

            public SourceSpan Source { get; }
        }

        private class Visitor : RazorIRNodeWalker
        {
            public List<InstrumentationItem> Items { get; } = new List<InstrumentationItem>();

            public override void VisitHtml(HtmlContentIRNode node)
            {
                if (node.Source != null)
                {
                    Items.Add(new InstrumentationItem(node, isLiteral: true, source: node.Source.Value));
                }

                VisitDefault(node);
            }

            public override void VisitCSharpExpression(CSharpExpressionIRNode node)
            {
                if (node.Source != null && !(node.Parent is CSharpAttributeValueIRNode))
                {
                    Items.Add(new InstrumentationItem(node, isLiteral: false, source: node.Source.Value));
                }

                VisitDefault(node);
            }

            internal override void VisitExecuteTagHelpers(ExecuteTagHelpersIRNode node)
            {
                // As a special case the TagHelperIRNode (which must be the parent) is the one that carries
                // the location. The execute node won't have one, but the instrumentation goes around the call
                // to execute.
                if (node.Parent is TagHelperIRNode && node.Parent.Source != null)
                {
                    Items.Add(new InstrumentationItem(node, isLiteral: false, source: node.Parent.Source.Value));
                }

                VisitDefault(node);
            }
        }
    }
}
