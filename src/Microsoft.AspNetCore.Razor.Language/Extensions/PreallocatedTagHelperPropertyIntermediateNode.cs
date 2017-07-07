// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Razor.Language.Extensions
{
    internal sealed class PreallocatedTagHelperPropertyIntermediateNode : TagHelperPropertyIntermediateNode
    {
        public PreallocatedTagHelperPropertyIntermediateNode()
        {
        }

        public PreallocatedTagHelperPropertyIntermediateNode(DefaultTagHelperPropertyIntermediateNode other)
            : base(other)
        {
            // We don't want to add any of the children of the node we were created from. The contents of the attribute
            // are represented elsewhere.
            Children.Clear();
        }

        public string FieldName { get; set; }

        public string PropertyName { get; set; }

        public string VariableName { get; set; }

        public override void Accept(IntermediateNodeVisitor visitor)
        {
            if (visitor == null)
            {
                throw new ArgumentNullException(nameof(visitor));
            }

            AcceptExtensionNode<PreallocatedTagHelperPropertyIntermediateNode>(this, visitor);
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

            var extension = target.GetExtension<IPreallocatedAttributeTargetExtension>();
            if (extension == null)
            {
                ReportMissingCodeTargetExtension<IPreallocatedAttributeTargetExtension>(context);
                return;
            }

            extension.WriteTagHelperProperty(context, this);
        }
    }
}
