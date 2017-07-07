﻿// Copyright(c) .NET Foundation.All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public sealed class DirectiveIntermediateNode : IntermediateNode
    {
        public override IntermediateNodeCollection Children { get; } = new IntermediateNodeCollection();

        public string DirectiveName { get; set; }

        public IEnumerable<DirectiveTokenIntermediateNode> Tokens => Children.OfType<DirectiveTokenIntermediateNode>();

        public DirectiveDescriptor Directive { get; set; }

        public override void Accept(IntermediateNodeVisitor visitor)
        {
            visitor.VisitDirective(this);
        }
    }
}