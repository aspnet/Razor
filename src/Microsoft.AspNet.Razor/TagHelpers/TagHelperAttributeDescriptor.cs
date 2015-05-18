// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// A metadata class describing a tag helper attribute.
    /// </summary>
    public class TagHelperAttributeDescriptor
    {
        // Internal for testing i.e. for easy TagHelperAttributeDescriptor creation when PropertyInfo is available.
        internal TagHelperAttributeDescriptor([NotNull] string name, [NotNull] PropertyInfo propertyInfo)
            : this(
                  name,
                  propertyInfo.Name,
                  propertyInfo.PropertyType.FullName,
                  isStringProperty: propertyInfo.PropertyType == typeof(string),
                  prefix: null,
                  objectCreationExpression: null,
                  prefixedValueTypeName: null,
                  areStringPrefixedValues: false)
        {
        }

        // Internal for testing i.e. for easy TagHelperAttributeDescriptor creation without Prefix information.
        internal TagHelperAttributeDescriptor(
            [NotNull] string name,
            [NotNull] string propertyName,
            [NotNull] string typeName)
            : this(
                  name,
                  propertyName,
                  typeName,
                  prefix: null,
                  objectCreationExpression: null,
                  prefixedValueTypeName: null)
        {
        }

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperAttributeDescriptor"/> class.
        /// </summary>
        /// <param name="name">The HTML attribute name.</param>
        /// <param name="propertyName">The name of the CLR property that corresponds to the HTML attribute.</param>
        /// <param name="typeName">
        /// The full name of the named (see <paramref name="propertyName"/>) property's <see cref="System.Type"/>.
        /// </param>
        /// <param name="prefix">
        /// The prefix used to match HTML attribute names. Matching attributes are added to the associated property
        /// (an <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>).
        /// </param>
        /// <param name="objectCreationExpression">
        /// The C# object creation expression (<c>new</c> operator invocation) used to initialize the property.
        /// </param>
        /// <param name="prefixedValueTypeName">
        /// The full name of <c>TValue</c> in the <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>
        /// the named (see <paramref name="propertyName"/>) property implements.
        /// </param>
        /// <remarks>
        /// <paramref name="objectCreationExpression"/> and <paramref name="prefixedValueTypeName"/> are
        /// ignored if <paramref name="prefix"/> is <c>null</c>. In turn <paramref name="prefix"/> is expected to be
        /// non-<c>null</c> only if <paramref name="typeName"/> implements
        /// <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> where <c>TKey</c> is
        /// <see cref="string"/>.
        /// </remarks>
        public TagHelperAttributeDescriptor(
            [NotNull] string name,
            [NotNull] string propertyName,
            [NotNull] string typeName,
            string prefix,
            string objectCreationExpression,
            string prefixedValueTypeName)
            : this(
                  name,
                  propertyName,
                  typeName,
                  isStringProperty: string.Equals(typeName, typeof(string).FullName, StringComparison.Ordinal),
                  prefix: prefix,
                  objectCreationExpression: objectCreationExpression,
                  prefixedValueTypeName: prefixedValueTypeName,
                  areStringPrefixedValues: string.Equals(
                      prefixedValueTypeName,
                      typeof(string).FullName,
                      StringComparison.Ordinal))
        {
        }

        // Internal for testing i.e. for confirming above constructor sets IsStringProperty and
        // AreStringPrefixedValues as expected.
        internal TagHelperAttributeDescriptor(
            [NotNull] string name,
            [NotNull] string propertyName,
            [NotNull] string typeName,
            bool isStringProperty,
            string prefix,
            string objectCreationExpression,
            string prefixedValueTypeName,
            bool areStringPrefixedValues)
        {
            Name = name;
            PropertyName = propertyName;
            TypeName = typeName;
            IsStringProperty = isStringProperty;

            Prefix = prefix;
            if (prefix != null)
            {
                ObjectCreationExpression = objectCreationExpression;
                PrefixedValueTypeName = prefixedValueTypeName;
                AreStringPrefixedValues = areStringPrefixedValues;
            }
        }

        /// <summary>
        /// Gets an indication whether this property type implements
        /// <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> where <c>TValue</c> is
        /// <see cref="string"/>.
        /// </summary>
        /// <value>
        /// If <c>true</c> the <see cref="TypeName"/> is for an
        /// <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> where both <c>TKey</c> and
        /// <c>TValue</c> are <see cref="string"/>. This causes the razor parser to allow empty values for attributes
        /// that have names starting with <see cref="Prefix"/>. If <c>false</c> empty values for such matching
        /// attributes lead to errors.
        /// </value>
        public bool AreStringPrefixedValues { get; }

        /// <summary>
        /// Gets an indication whether this property is of type <see cref="string"/>.
        /// </summary>
        /// <value>
        /// If <c>true</c> the <see cref="TypeName"/> is for <see cref="string"/>. This causes the Razor parser
        /// to allow empty values for attributes that have names matching <see cref="Name"/>. If <c>false</c>
        /// empty values for such matching attributes lead to errors.
        /// </value>
        public bool IsStringProperty { get; }

        /// <summary>
        /// The HTML attribute name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the C# object creation expression (<c>new</c> operator invocation) used to initialize the property.
        /// Ignored if <paramref name="prefix"/> is <c>null</c>.
        /// </summary>
        public string ObjectCreationExpression { get; }

        /// <summary>
        /// Gets the prefix used to match HTML attribute names. Matching attributes are added to the associated
        /// property (an <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/>).
        /// </summary>
        public string Prefix { get; }

        /// <summary>
        /// Gets the full name of <c>TValue</c> in the
        /// <see cref="System.Collections.Generic.IDictionary{TKey, TValue}"/> the named (see
        /// <see cref="PropertyName"/>) property implements.
        /// </summary>
        public string PrefixedValueTypeName { get; }

        /// <summary>
        /// The name of the CLR property that corresponds to the HTML attribute.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The full name of the named (see <see name="PropertyName"/>) property's <see cref="System.Type"/>.
        /// </summary>
        public string TypeName { get; }
    }
}