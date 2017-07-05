﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public sealed class IntermediateNodeCollection : IList<IntermediateNode>
    {
        public static readonly IntermediateNodeCollection ReadOnly = new IntermediateNodeCollection(null);

        private readonly IList<IntermediateNode> _inner;

        public IntermediateNodeCollection()
            : this(new List<IntermediateNode>())
        {
        }

        private IntermediateNodeCollection(IList<IntermediateNode> inner)
        {
            // Inner can be null when the collection is meant to be readonly.
            _inner = inner;
        }

        public  IntermediateNode this[int index]
        {
            get
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                return _inner[index];
            }
            set
            {
                if (index < 0 || index >= Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(index));
                }

                _inner[index] = value;
            }
        }

        public int Count => _inner == null ? 0 : _inner.Count;

        public bool IsReadOnly => _inner == null || _inner.IsReadOnly;

        public void Add(IntermediateNode item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_inner == null)
            {
                throw new NotSupportedException();
            }

            _inner.Add(item);
        }

        public void Clear()
        {
            if (_inner == null)
            {
                throw new NotSupportedException();
            }

            _inner.Clear();
        }

        public bool Contains(IntermediateNode item)
        {
            if (_inner == null)
            {
                return false;
            }

            return _inner.Contains(item);
        }

        public void CopyTo(IntermediateNode[] array, int arrayIndex)
        {
            if (array == null)
            {
                throw new ArgumentNullException(nameof(array));
            }

            if (arrayIndex < 0 || arrayIndex > array.Length)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }
            else if (array.Length - arrayIndex < Count)
            {
                throw new ArgumentOutOfRangeException(nameof(arrayIndex));
            }

            if (_inner == null)
            {
                return;
            }

            _inner.CopyTo(array, arrayIndex);
        }

        public Enumerator GetEnumerator()
        {
            return new Enumerator(this);
        }

        IEnumerator<IntermediateNode> IEnumerable<IntermediateNode>.GetEnumerator()
        {
            return GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public int IndexOf(IntermediateNode item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_inner == null)
            {
                return -1;
            }

            return _inner.IndexOf(item);
        }

        public void Insert(int index, IntermediateNode item)
        {
            if (index < 0 || index > Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_inner == null)
            {
                throw new NotSupportedException();
            }

            _inner.Insert(index, item);
        }

        public bool Remove(IntermediateNode item)
        {
            if (item == null)
            {
                throw new ArgumentNullException(nameof(item));
            }

            if (_inner == null)
            {
                throw new NotSupportedException();
            }

            return _inner.Remove(item);
        }

        public void RemoveAt(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index));
            }

            if (_inner == null)
            {
                throw new NotSupportedException();
            }

            _inner.RemoveAt(index);
        }

        public struct Enumerator : IEnumerator<IntermediateNode>
        {
            private readonly IList<IntermediateNode> _items;
            private int _index;

            public Enumerator(IntermediateNodeCollection collection)
            {
                if (collection == null)
                {
                    throw new ArgumentNullException(nameof(collection));
                }

                _items = collection._inner;
                _index = -1;
            }

            public IntermediateNode Current
            {
                get
                {
                    if (_index < 0 || _items == null || _index >= _items.Count)
                    {
                        return null;
                    }

                    return _items[_index];
                }
            }

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

            public bool MoveNext()
            {
                _index++;
                return _items != null && _index < _items.Count;
            }

            public void Reset()
            {
                _index = -1;
            }
        }
    }
}
