﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Evolution.Intermediate
{
    public abstract class IRNodeWalker : IRNodeVisitor
    {
        public override void VisitDefault(IRNode node)
        {
            var children = node.Children;
            for (var i = 0; i < node.Children.Count; i++)
            {
                var child = children[i];
                Visit(child);
            }
        }
    }
}