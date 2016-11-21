// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    internal class Block : SyntaxTreeNode
    {
        public Block(BlockBuilder source)
            : this(source.Type, source.Children, source.ChunkGenerator)
        {
            source.Reset();
        }

        protected Block(BlockType? type, IReadOnlyList<SyntaxTreeNode> children, IParentChunkGenerator generator)
        {
            if (type == null)
            {
                throw new InvalidOperationException(LegacyResources.Block_Type_Not_Specified);
            }

            Type = type.Value;
            Children = children;
            ChunkGenerator = generator;

            // Perf: Avoid allocating an enumerator.
            for (var i = 0; i < Children.Count; i++)
            {
                Children[i].Parent = this;
            }
        }
        public IParentChunkGenerator ChunkGenerator { get; }

        public BlockType Type { get; }

        public IReadOnlyList<SyntaxTreeNode> Children { get; }

        public override bool IsBlock => true;

        public override SourceLocation Start
        {
            get
            {
                var child = Children.FirstOrDefault();
                if (child == null)
                {
                    return SourceLocation.Zero;
                }
                else
                {
                    return child.Start;
                }
            }
        }

        public override int Length => Children.Sum(child => child.Length);

        public virtual IEnumerable<Span> Flatten()
        {
            // Perf: Avoid allocating an enumerator.
            for (var i = 0; i < Children.Count; i++)
            {
                var element = Children[i];
                var span = element as Span;
                if (span != null)
                {
                    yield return span;
                }
                else
                {
                    var block = element as Block;
                    foreach (Span childSpan in block.Flatten())
                    {
                        yield return childSpan;
                    }
                }
            }
        }

        public Span FindFirstDescendentSpan()
        {
            SyntaxTreeNode current = this;
            while (current != null && current.IsBlock)
            {
                current = ((Block)current).Children.FirstOrDefault();
            }
            return current as Span;
        }

        public Span FindLastDescendentSpan()
        {
            SyntaxTreeNode current = this;
            while (current != null && current.IsBlock)
            {
                current = ((Block)current).Children.LastOrDefault();
            }
            return current as Span;
        }

        public override string ToString()
        {
            return string.Format(
                CultureInfo.CurrentCulture,
                "{0} Block at {1}::{2} (Gen:{3})",
                Type,
                Start,
                Length,
                ChunkGenerator);
        }

        public override bool Equals(object obj)
        {
            var other = obj as Block;
            return other != null &&
                Type == other.Type &&
                Equals(ChunkGenerator, other.ChunkGenerator) &&
                ChildrenEqual(Children, other.Children);
        }

        public override int GetHashCode()
        {
            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(Type);
            hashCodeCombiner.Add(ChunkGenerator);
            hashCodeCombiner.Add(Children);

            return hashCodeCombiner;
        }

        private static bool ChildrenEqual(IEnumerable<SyntaxTreeNode> left, IEnumerable<SyntaxTreeNode> right)
        {
            IEnumerator<SyntaxTreeNode> leftEnum = left.GetEnumerator();
            IEnumerator<SyntaxTreeNode> rightEnum = right.GetEnumerator();
            while (leftEnum.MoveNext())
            {
                if (!rightEnum.MoveNext() || // More items in left than in right
                    !Equals(leftEnum.Current, rightEnum.Current))
                {
                    // Nodes are not equal
                    return false;
                }
            }
            if (rightEnum.MoveNext())
            {
                // More items in right than left
                return false;
            }
            return true;
        }

        public override bool EquivalentTo(SyntaxTreeNode node)
        {
            var other = node as Block;
            if (other == null || other.Type != Type)
            {
                return false;
            }

            return Enumerable.SequenceEqual(Children, other.Children, EquivalenceComparer.Default);
        }

        public override int GetEquivalenceHash()
        {
            var hashCodeCombiner = HashCodeCombiner.Start();
            hashCodeCombiner.Add(Type);
            foreach (var child in Children)
            {
                hashCodeCombiner.Add(child.GetEquivalenceHash());
            }

            return hashCodeCombiner.CombinedHash;
        }

        private class EquivalenceComparer : IEqualityComparer<SyntaxTreeNode>
        {
            public static readonly EquivalenceComparer Default = new EquivalenceComparer();

            private EquivalenceComparer()
            {
            }

            public bool Equals(SyntaxTreeNode nodeX, SyntaxTreeNode nodeY)
            {
                if (nodeX == nodeY)
                {
                    return true;
                }

                return nodeX != null && nodeX.EquivalentTo(nodeY);
            }

            public int GetHashCode(SyntaxTreeNode node)
            {
                if (node == null)
                {
                    throw new ArgumentNullException(nameof(node));
                }

                return node.GetEquivalenceHash();
            }
        }
    }
}
