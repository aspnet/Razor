// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public abstract class ExtensionIntermediateNode : IntermediateNode
    {
        public abstract void WriteNode(CodeTarget target, CodeRenderingContext context);

        protected static void AcceptExtensionNode<TNode>(TNode node, IntermediateNodeVisitor visitor)
            where TNode : ExtensionIntermediateNode
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
                visitor.VisitExtension(node);
            }
        }

        protected internal static void ReportMissingCodeTargetExtension<TDependency>(CodeRenderingContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var documentKind = context.DocumentKind ?? string.Empty;
            context.Diagnostics.Add(
                RazorDiagnosticFactory.CreateCodeTarget_UnsupportedExtension(
                    documentKind, 
                    typeof(TDependency)));
        }
    }
}
