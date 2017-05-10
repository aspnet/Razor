﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public abstract class ExtensionIRNode : RazorIRNode
    {
        private ItemCollection _annotations;

        public override ItemCollection Annotations
        {
            get
            {
                if (_annotations == null)
                {
                    _annotations = new DefaultItemCollection();
                }

                return _annotations;
            }
        }

        public abstract void WriteNode(RuntimeTarget target, CSharpRenderingContext context);

        protected static void AcceptExtensionNode<TNode>(TNode node, RazorIRNodeVisitor visitor) 
            where TNode : ExtensionIRNode
        {
            var typedVisitor = visitor as IExtensionIRNodeVisitor<TNode>;
            if (typedVisitor == null)
            {
                visitor.VisitExtension(node);
            }
            else
            {
                typedVisitor.VisitExtension(node);
            }
        }
    }
}
