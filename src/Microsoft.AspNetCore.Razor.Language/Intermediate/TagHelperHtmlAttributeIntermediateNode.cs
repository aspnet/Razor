// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public class TagHelperHtmlAttributeIntermediateNode : IntermediateNode
    {
        public TagHelperHtmlAttributeIntermediateNode()
        {
        }

        public TagHelperHtmlAttributeIntermediateNode(TagHelperHtmlAttributeIntermediateNode other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            AttributeName = other.AttributeName;
            AttributeStructure = other.AttributeStructure;
            Source = other.Source;

            for (var i = 0; i < other.Children.Count; i++)
            {
                Children.Add(other.Children[i]);
            }

            for (var i = 0; i < other.Diagnostics.Count; i++)
            {
                Diagnostics.Add(other.Diagnostics[i]);
            }
        }

        public override IntermediateNodeCollection Children { get; } = new IntermediateNodeCollection();

        public string AttributeName { get; set; }

        public AttributeStructure AttributeStructure { get; set; }

        public override void Accept(IntermediateNodeVisitor visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            visitor.VisitTagHelperHtmlAttribute(this);
        }

        public virtual void WriteNode(CodeTarget target, CodeRenderingContext context)
        {
        }

        protected static void AcceptExtensionNode<TNode>(TNode node, IntermediateNodeVisitor visitor)
            where TNode : TagHelperHtmlAttributeIntermediateNode
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            if (node == null)
            {
                throw new ArgumentNullException(nameof(node));
            }

            if (visitor is IExtensionIntermediateNodeVisitor<TNode> typedVisitor)
            {
                typedVisitor.VisitExtension(node);
            }
            else
            {
                visitor.VisitTagHelperHtmlAttribute(node);
            }
        }

        protected static void ReportMissingCodeTargetExtension<TDependency>(CodeRenderingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            ExtensionIntermediateNode.ReportMissingCodeTargetExtension<TDependency>(context);
        }
    }
}
