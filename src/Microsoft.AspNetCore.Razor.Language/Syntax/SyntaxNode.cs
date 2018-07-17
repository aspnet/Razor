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

        public virtual string ToFullString()
        {
            return GreenNode.ToFullString();
        }
    }
}
