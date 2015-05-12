// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// A metadata class describing a tag helper attribute.
    /// </summary>
    public class TagHelperAttributeDescriptor
    {
        // Internal for testing
        internal TagHelperAttributeDescriptor([NotNull] string name, [NotNull] PropertyInfo propertyInfo)
            : this(
                  name,
                  propertyInfo.Name,
                  propertyInfo.PropertyType.FullName,
                  isStringProperty: propertyInfo.PropertyType == typeof(string))
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
        /// <param name="isStringProperty">
        /// An indication whether this property is of type <see cref="string"/>.
        /// </param>
        public TagHelperAttributeDescriptor(
            [NotNull] string name,
            [NotNull] string propertyName,
            [NotNull] string typeName,
            bool isStringProperty)
        {
            Name = name;
            PropertyName = propertyName;
            TypeName = typeName;
            IsStringProperty = isStringProperty;
        }

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
        /// The name of the CLR property that corresponds to the HTML attribute.
        /// </summary>
        public string PropertyName { get; }

        /// <summary>
        /// The full name of the named (see <see name="PropertyName"/>) property's <see cref="System.Type"/>.
        /// </summary>
        public string TypeName { get; }
    }
}