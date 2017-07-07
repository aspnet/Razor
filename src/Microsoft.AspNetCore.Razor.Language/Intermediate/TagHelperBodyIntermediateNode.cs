// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public class TagHelperBodyIntermediateNode : IntermediateNode
    {
        public TagHelperBodyIntermediateNode()
        {
        }

        public TagHelperBodyIntermediateNode(TagHelperBodyIntermediateNode other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

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

        public override void Accept(IntermediateNodeVisitor visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            visitor.VisitTagHelperBody(this);
        }

        public virtual void WriteNode(CodeTarget target, CodeRenderingContext context)
        {
        }

        protected static void AcceptExtensionNode<TNode>(TNode node, IntermediateNodeVisitor visitor)
            where TNode : TagHelperBodyIntermediateNode
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
                visitor.VisitTagHelperBody(node);
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
