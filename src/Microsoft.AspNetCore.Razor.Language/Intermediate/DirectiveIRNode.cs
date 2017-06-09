﻿// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public sealed class DirectiveIRNode : RazorIRNode
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

        public override RazorIRNodeCollection Children { get; } = new DefaultIRNodeCollection();

        public override SourceSpan? Source { get; set; }

        public string Name { get; set; }

        public IEnumerable<DirectiveTokenIRNode> Tokens => Children.OfType<DirectiveTokenIRNode>();

        public DirectiveDescriptor Descriptor { get; set; }

        public override void Accept(RazorIRNodeVisitor visitor)
        {
            visitor.VisitDirective(this);
        }
    }
}