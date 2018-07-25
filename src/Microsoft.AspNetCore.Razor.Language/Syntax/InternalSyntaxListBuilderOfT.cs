// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language
{
    internal readonly struct InternalSyntaxListBuilder<TNode> where TNode : GreenNode
    {
        private readonly InternalSyntaxListBuilder _builder;

        public InternalSyntaxListBuilder(int size)
            : this(new InternalSyntaxListBuilder(size))
        {
        }

        public static InternalSyntaxListBuilder<TNode> Create()
        {
            return new InternalSyntaxListBuilder<TNode>(8);
        }

        internal InternalSyntaxListBuilder(InternalSyntaxListBuilder builder)
        {
            _builder = builder;
        }

        public bool IsNull
        {
            get
            {
                return _builder == null;
            }
        }

        public int Count
        {
            get
            {
                return _builder.Count;
            }
        }

        public TNode this[int index]
        {
            get
            {
                return (TNode)_builder[index];
            }

            set
            {
                _builder[index] = value;
            }
        }

        public void Clear()
        {
            _builder.Clear();
        }

        public InternalSyntaxListBuilder<TNode> Add(TNode node)
        {
            _builder.Add(node);
            return this;
        }

        public void AddRange(TNode[] items, int offset, int length)
        {
            _builder.AddRange(items, offset, length);
        }

        public void AddRange(InternalSyntaxList<TNode> nodes)
        {
            _builder.AddRange(nodes);
        }

        public void AddRange(InternalSyntaxList<TNode> nodes, int offset, int length)
        {
            _builder.AddRange(nodes, offset, length);
        }

        public bool Any(SyntaxKind kind)
        {
            return _builder.Any(kind);
        }

        public InternalSyntaxList<TNode> ToList()
        {
            return _builder.ToList<TNode>();
        }

        public GreenNode ToListNode()
        {
            return _builder.ToListNode();
        }

        public static implicit operator InternalSyntaxListBuilder(InternalSyntaxListBuilder<TNode> builder)
        {
            return builder._builder;
        }

        public static implicit operator InternalSyntaxList<TNode>(InternalSyntaxListBuilder<TNode> builder)
        {
            if (builder._builder != null)
            {
                return builder.ToList();
            }

            return default(InternalSyntaxList<TNode>);
        }

        public InternalSyntaxList<TDerived> ToList<TDerived>() where TDerived : GreenNode
        {
            return new InternalSyntaxList<TDerived>(ToListNode());
        }
    }
}
