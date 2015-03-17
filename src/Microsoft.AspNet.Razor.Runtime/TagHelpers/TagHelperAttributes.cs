// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public class TagHelperAttributes<TAttributeValue> : IList<TagHelperAttribute<TAttributeValue>>
    {
        private readonly IList<TagHelperAttribute<TAttributeValue>> _attributes;

        public TagHelperAttributes()
        {
            _attributes = new List<TagHelperAttribute<TAttributeValue>>();
        }

        public int Count => _attributes.Count;

        public bool IsReadOnly => false;

        public IEnumerable<string> Keys => _attributes.Select(attribute => attribute.Key);

        public IEnumerable<TAttributeValue> Values => _attributes.Select(attribute => attribute.Value);

        public TagHelperAttribute<TAttributeValue> this[[NotNull] string key] =>
            _attributes.LastOrDefault(attribute => MatchesAttribute(key, attribute));

        public TagHelperAttribute<TAttributeValue> this[int index]
        {
            get
            {
                return _attributes[index];
            }
            set
            {
                _attributes[index] = value;
            }
        }

        public bool TryGetAttribute([NotNull] string key, out TagHelperAttribute<TAttributeValue> attribute)
        {
            attribute = this[key];

            return attribute != null;
        }

        public bool TryGetAttributes([NotNull] string key, out IEnumerable<TagHelperAttribute<TAttributeValue>> attributes)
        {
            attributes = _attributes.Where(attribute => MatchesAttribute(key, attribute));

            return attributes.Any();
        }

        public void Add([NotNull] string key)
        {
            _attributes.Add(new TagHelperAttribute<TAttributeValue>(key)
            {
                Minimized = true
            });
        }

        public void Add([NotNull] string key, TAttributeValue value)
        {
            _attributes.Add(new TagHelperAttribute<TAttributeValue>(key, value));
        }

        public void Add([NotNull] TagHelperAttribute<TAttributeValue> attribute)
        {
            _attributes.Add(attribute);
        }

        public void Insert(int index, TagHelperAttribute<TAttributeValue> item)
        {
            _attributes.Insert(index, item);
        }

        public void CopyTo([NotNull] TagHelperAttribute<TAttributeValue>[] attributes, int index)
        {
            _attributes.CopyTo(attributes, index);
        }

        public bool Remove([NotNull] string key)
        {
            IEnumerable<TagHelperAttribute<TAttributeValue>> attributes;
            if (TryGetAttributes(key, out attributes))
            {
                foreach (var attribute in attributes)
                {
                    Remove(attribute);
                }

                return true;
            }

            return false;
        }

        public bool Remove([NotNull] TagHelperAttribute<TAttributeValue> item)
        {
            // REVIEW: Do we want this to be a comparer remove? If so it'd remove more than one.
            return _attributes.Remove(item);
        }

        public void RemoveAt(int index)
        {
            _attributes.RemoveAt(index);
        }

        public void Clear()
        {
            _attributes.Clear();
        }

        public bool Contains([NotNull] string key)
        {
            return _attributes.Any(attribute => MatchesAttribute(key, attribute));
        }

        public bool Contains([NotNull] TagHelperAttribute<TAttributeValue> item)
        {
            // REVIEW: Do we want this to be a comparer check?
            return _attributes.Contains(item);
        }

        public int IndexOf([NotNull] TagHelperAttribute<TAttributeValue> item)
        {
            return _attributes.IndexOf(item);
        }

        public IEnumerator<TagHelperAttribute<TAttributeValue>> GetEnumerator()
        {
            return _attributes.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static bool MatchesAttribute(string key, TagHelperAttribute<TAttributeValue> attribute)
        {
            return string.Equals(key, attribute.Key, StringComparison.OrdinalIgnoreCase);
        }
    }
}