// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Threading;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal abstract class SyntaxNode
    {
        public SyntaxNode(GreenNode green, SyntaxNode parent, int position)
        {
            GreenNode = green;
            Parent = parent;
            Start = position;
        }

        internal GreenNode GreenNode { get; }

        public SyntaxNode Parent { get; }

        public int Start { get; }

        public SyntaxKind Kind => GreenNode.Kind;

        public int FullWidth => GreenNode.FullWidth;

        public int Width => GreenNode.Width;

        public int SpanStart => Start + GreenNode.GetLeadingTriviaWidth();

        public TextSpan FullSpan => new TextSpan(Start, GreenNode.FullWidth);

        public int End => Start + FullWidth;

        public TextSpan Span
        {
            get
            {
                // Start with the full span.
                var start = Start;
                var width = GreenNode.FullWidth;

                // adjust for preceding trivia (avoid calling this twice, do not call Green.Width)
                var precedingWidth = GreenNode.GetLeadingTriviaWidth();
                start += precedingWidth;
                width -= precedingWidth;

                // adjust for following trivia width
                width -= GreenNode.GetTrailingTriviaWidth();

                Debug.Assert(width >= 0);
                return new TextSpan(start, width);
            }
        }

        internal int SlotCount => GreenNode.SlotCount;

        public bool IsList => GreenNode.IsList;

        protected virtual int GetTextWidth()
        {
            return 0;
        }

        internal abstract SyntaxNode Accept(SyntaxVisitor visitor);

        internal abstract SyntaxNode GetNodeSlot(int index);

        /// <summary>
        /// Gets a node at given node index without forcing its creation.
        /// If node was not created it would return null.
        /// </summary>
        internal abstract SyntaxNode GetCachedSlot(int index);

        internal SyntaxNode GetRed(ref SyntaxNode field, int slot)
        {
            var result = field;

            if (result == null)
            {
                var green = GreenNode.GetSlot(slot);
                if (green != null)
                {
                    Interlocked.CompareExchange(ref field, green.CreateRed(this, GetChildPosition(slot)), null);
                    result = field;
                }
            }

            return result;
        }

        protected T GetRed<T>(ref T field, int slot) where T : SyntaxNode
        {
            var result = field;

            if (result == null)
            {
                var green = this.GreenNode.GetSlot(slot);
                if (green != null)
                {
                    Interlocked.CompareExchange(ref field, (T)green.CreateRed(this, this.GetChildPosition(slot)), null);
                    result = field;
                }
            }

            return result;
        }

        internal SyntaxNode GetRedElement(ref SyntaxNode element, int slot)
        {
            Debug.Assert(IsList);

            var result = element;

            if (result == null)
            {
                var green = GreenNode.GetSlot(slot);
                // passing list's parent
                Interlocked.CompareExchange(ref element, green.CreateRed(Parent, GetChildPosition(slot)), null);
                result = element;
            }

            return result;
        }

        internal virtual int GetChildPosition(int index)
        {
            var offset = 0;
            var green = GreenNode;
            while (index > 0)
            {
                index--;
                var prevSibling = GetCachedSlot(index);
                if (prevSibling != null)
                {
                    return prevSibling.End + offset;
                }
                var greenChild = green.GetSlot(index);
                if (greenChild != null)
                {
                    offset += greenChild.FullWidth;
                }
            }

            return Start + offset;
        }

        // Get the leading trivia a green array, recursively to first token.
        public virtual SyntaxTriviaList GetLeadingTrivia()
        {
            var firstToken = GetFirstToken();
            return firstToken == null ? default(SyntaxTriviaList) : firstToken.GetLeadingTrivia();
        }

        // Get the trailing trivia a green array, recursively to first token.
        public virtual SyntaxTriviaList GetTrailingTrivia()
        {
            var lastToken = GetLastToken();
            return lastToken == null ? default(SyntaxTriviaList) : lastToken.GetTrailingTrivia();
        }

        internal SyntaxToken GetFirstToken()
        {
            return ((SyntaxToken)GetFirstTerminal());
        }

        internal SyntaxToken GetLastToken()
        {
            return ((SyntaxToken)GetLastTerminal());
        }

        public SyntaxNode GetFirstTerminal()
        {
            var node = this;

            do
            {
                bool foundChild = false;
                for (int i = 0, n = node.SlotCount; i < n; i++)
                {
                    var child = node.GetNodeSlot(i);
                    if (child != null)
                    {
                        node = child;
                        foundChild = true;
                        break;
                    }
                }

                if (!foundChild)
                {
                    return null;
                }
            }
            while (node.SlotCount != 0);

            return node == this ? this : node;
        }

        public SyntaxNode GetLastTerminal()
        {
            var node = this;

            do
            {
                for (int i = node.SlotCount - 1; i >= 0; i--)
                {
                    var child = node.GetNodeSlot(i);
                    if (child != null)
                    {
                        node = child;
                        break;
                    }
                }
            } while (node.SlotCount != 0);

            return node == this ? this : node;
        }

        public virtual string ToFullString()
        {
            return GreenNode.ToFullString();
        }
    }
}
