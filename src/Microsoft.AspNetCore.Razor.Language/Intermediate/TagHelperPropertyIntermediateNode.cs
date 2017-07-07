// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public class TagHelperPropertyIntermediateNode : IntermediateNode
    {
        public TagHelperPropertyIntermediateNode()
        {
        }

        public TagHelperPropertyIntermediateNode(TagHelperPropertyIntermediateNode other)
        {
            if (other == null)
            {
                throw new ArgumentNullException(nameof(other));
            }

            AttributeName = other.AttributeName;
            AttributeStructure = other.AttributeStructure;
            BoundAttribute = other.BoundAttribute;
            IsIndexerNameMatch = other.IsIndexerNameMatch;
            Source = other.Source;
            TagHelper = other.TagHelper;

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

        public BoundAttributeDescriptor BoundAttribute { get; set; }

        public TagHelperDescriptor TagHelper { get; set; }

        public bool IsIndexerNameMatch { get; set; }

        public override void Accept(IntermediateNodeVisitor visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            visitor.VisitTagHelperProperty(this);
        }

        public virtual void WriteNode(CodeTarget target, CodeRenderingContext context)
        {
        }

        protected static void AcceptExtensionNode<TNode>(TNode node, IntermediateNodeVisitor visitor)
            where TNode : TagHelperPropertyIntermediateNode
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
                visitor.VisitTagHelperProperty(node);
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
