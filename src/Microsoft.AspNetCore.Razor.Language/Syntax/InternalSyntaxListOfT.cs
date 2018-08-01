// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal readonly struct InternalSyntaxList<TNode>
        where TNode : GreenNode
    {
        private readonly GreenNode _node;

        public InternalSyntaxList(GreenNode node)
        {
            _node = node;
        }

        public GreenNode Node
        {
            get
            {
                return ((GreenNode)_node);
            }
        }

        public int Count
        {
            get
            {
                return (_node == null) ? 0 : _node.IsList ? _node.SlotCount : 1;
            }
        }

        public TNode Last
        {
            get
            {
                var node = _node;
                if (node.IsList)
                {
                    return ((TNode)node.GetSlot(node.SlotCount - 1));
                }

                return ((TNode)node);
            }
        }

        /* Not Implemented: Default */
        public TNode this[int index]
        {
            get
            {
                var node = _node;
                if (node.IsList)
                {
                    return ((TNode)node.GetSlot(index));
                }

                Debug.Assert(index == 0);
                return ((TNode)node);
            }
        }

        public GreenNode ItemUntyped(int index)
        {
            var node = _node;
            if (node.IsList)
            {
                return node.GetSlot(index);
            }

            Debug.Assert(index == 0);
            return node;
        }

        public bool Any()
        {
            return _node != null;
        }

        public bool Any(SyntaxKind kind)
        {
            for (var i = 0; i < this.Count; i++)
            {
                var element = this.ItemUntyped(i);
                if ((element.Kind == kind))
                {
                    return true;
                }
            }

            return false;
        }

        public TNode[] Nodes
        {
            get
            {
                var arr = new TNode[this.Count];
                for (var i = 0; i < this.Count; i++)
                {
                    arr[i] = this[i];
                }

                return arr;
            }
        }

        public static bool operator ==(InternalSyntaxList<TNode> left, InternalSyntaxList<TNode> right)
        {
            return (left._node == right._node);
        }

        public static bool operator !=(InternalSyntaxList<TNode> left, InternalSyntaxList<TNode> right)
        {
            return !(left._node == right._node);
        }

        public override bool Equals(object obj)
        {
            return (obj is InternalSyntaxList<TNode> && (_node == ((InternalSyntaxList<TNode>)obj)._node));
        }

        public override int GetHashCode()
        {
            return _node != null ? _node.GetHashCode() : 0;
        }

        /*public SeparatedSyntaxList<TOther> AsSeparatedList<TOther>() where TOther : GreenNode
        {
            return new SeparatedSyntaxList<TOther>(new SyntaxList<TOther>(_node));
        }*/

        public static implicit operator InternalSyntaxList<TNode>(TNode node)
        {
            return new InternalSyntaxList<TNode>(node);
        }

        public static implicit operator InternalSyntaxList<GreenNode>(InternalSyntaxList<TNode> nodes)
        {
            return new InternalSyntaxList<GreenNode>(nodes._node);
        }
    }
}
