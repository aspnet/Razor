// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal abstract class GreenNode
    {
        private int _fullWidth;
        private NodeFlags _flags;
        private Byte _slotCount;

        protected GreenNode(SyntaxKind kind)
        {
            Kind = kind;
        }

        protected GreenNode(SyntaxKind kind, int fullWidth)
            : this(kind)
        {
            if (fullWidth == -1)
            {
                throw new InvalidOperationException($"Can't create {typeof(GreenNode).Name} with {nameof(fullWidth)} {fullWidth}.");
            }

            _fullWidth = fullWidth;
        }

        protected GreenNode(SyntaxKind kind, RazorDiagnostic[] diagnostics, SyntaxAnnotation[] annotations)
            : this(kind, 0, diagnostics, annotations)
        {
        }

        protected GreenNode(SyntaxKind kind, int fullWidth, RazorDiagnostic[] diagnostics, SyntaxAnnotation[] annotations)
            : this(kind, fullWidth)
        {
            if (diagnostics?.Length > 0)
            {
                _flags |= NodeFlags.ContainsDiagnostics;
                //diagnosticsTable.Add(this, diagnostics);
            }
            if (annotations?.Length > 0)
            {
                foreach (var annotation in annotations)
                    if (annotation == null)
                        throw new ArgumentException(paramName: nameof(annotations), message: "Annotation cannot be null");
                _flags |= NodeFlags.ContainsAnnotations;
                //annotationsTable.Add(this, annotations);
            }
        }

        public virtual int Width
        {
            get
            {
                return FullWidth - (GetLeadingTriviaWidth() + GetTrailingTriviaWidth());
            }
        }

        internal virtual bool IsList => false;

        internal virtual bool IsToken => false;

        internal virtual bool IsMissing => (_flags & NodeFlags.IsMissing) != 0;

        internal int FullWidth => _fullWidth;

        internal SyntaxKind Kind { get; }

        protected void AdjustWidth(GreenNode node)
        {
            _fullWidth += node == null ? 0 : node.FullWidth;
        }

        internal virtual GreenNode GetLeadingTrivia()
        {
            // TODO
            return null;
        }

        public virtual int GetLeadingTriviaWidth()
        {
            // TODO
            return 0;
        }

        internal virtual GreenNode GetTrailingTrivia()
        {
            // TODO
            return null;
        }

        public virtual int GetTrailingTriviaWidth()
        {
            // TODO
            return 0;
        }

        public int SlotCount
        {
            get
            {
                int count = _slotCount;
                if (count == byte.MaxValue)
                {
                    count = GetSlotCount();
                }

                return count;
            }

            protected set
            {
                _slotCount = (byte)value;
            }
        }

        internal abstract GreenNode GetSlot(int index);

        // for slot counts >= byte.MaxValue
        protected virtual int GetSlotCount()
        {
            return _slotCount;
        }

        public virtual int GetSlotOffset(int index)
        {
            var offset = 0;
            for (var i = 0; i < index; i++)
            {
                var child = GetSlot(i);
                if (child != null)
                    offset += child.FullWidth;
            }

            return offset;
        }

        public virtual int FindSlotIndexContainingOffset(int offset)
        {
            Debug.Assert(0 <= offset && offset < FullWidth);

            int i;
            var accumulatedWidth = 0;
            for (i = 0; ; i++)
            {
                Debug.Assert(i < SlotCount);
                var child = GetSlot(i);
                if (child != null)
                {
                    accumulatedWidth += child.FullWidth;
                    if (offset < accumulatedWidth)
                    {
                        break;
                    }
                }
            }

            return i;
        }

        public virtual GreenNode CreateList(IEnumerable<GreenNode> nodes, bool alwaysCreateListNode = false)
        {
            if (nodes == null)
            {
                return null;
            }

            var list = nodes.ToArray();

            switch (list.Length)
            {
                case 0:
                    return null;
                case 1:
                    if (alwaysCreateListNode)
                    {
                        goto default;
                    }
                    else
                    {
                        return list[0];
                    }
                case 2:
                    return SyntaxList.List(list[0], list[1]);
                case 3:
                    return SyntaxList.List(list[0], list[1], list[2]);
                default:
                    return SyntaxList.List(list);
            }
        }

        public SyntaxNode CreateRed()
        {
            return CreateRed(null, 0);
        }

        internal abstract SyntaxNode CreateRed(SyntaxNode parent, int position);

        internal RazorDiagnostic[] GetDiagnostics()
        {
            // TODO
            return Array.Empty<RazorDiagnostic>();
        }

        internal abstract GreenNode SetDiagnostics(RazorDiagnostic[] diagnostics);

        internal SyntaxAnnotation[] GetAnnotations()
        {
            // TODO
            return Array.Empty<SyntaxAnnotation>();
        }

        internal abstract GreenNode SetAnnotations(SyntaxAnnotation[] annotations);

        public virtual string ToFullString()
        {
            var builder = new StringBuilder();
            var writer = new StringWriter(builder, System.Globalization.CultureInfo.InvariantCulture);
            WriteTo(writer);
            return builder.ToString();
        }

        public virtual void WriteTo(TextWriter writer)
        {
            var stack = new Stack<GreenNode>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                stack.Pop().WriteToOrFlatten(writer, stack);
            }
        }

        /*  <summary>
        ''' NOTE: the method should write OR push children, but never do both
        ''' </summary>
        */
        internal virtual void WriteToOrFlatten(TextWriter writer, Stack<GreenNode> stack)
        {
            // By default just push children to the stack
            for (var i = SlotCount - 1; i >= 0; i--)
            {
                var node = GetSlot(i);
                if (node != null)
                {
                    stack.Push(GetSlot(i));
                }
            }
        }

        internal virtual GreenNode Accept(InternalSyntaxVisitor visitor)
        {
            return visitor.Visit(this);
        }
    }
}
