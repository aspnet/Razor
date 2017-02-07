// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    /// <summary>
    /// A metadata class describing a tag helper attribute.
    /// </summary>
    public class TagHelperAttributeDescriptor
    {
        private string _typeName;
        private string _name;

        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperAttributeDescriptor"/> class.
        /// </summary>
        public TagHelperAttributeDescriptor()
        {
        }

        public TagHelperAttributeDescriptor(TagHelperAttributeDescriptor descriptor)
        {
            Name = descriptor.Name;
            TypeName = descriptor.TypeName;
            IsEnum = descriptor.IsEnum;
            IsDictionary = descriptor.IsDictionary;
            Summary = descriptor.Summary;
        }

        // Internal for testing i.e. for easy TagHelperAttributeDescriptor creation when PropertyInfo is available.
        internal TagHelperAttributeDescriptor(string name, PropertyInfo propertyInfo)
        {
            Name = name;
            TypeName = propertyInfo.PropertyType.FullName;
            IsEnum = propertyInfo.PropertyType.GetTypeInfo().IsEnum;
        }

        /// <summary>
        /// Gets an indication whether this <see cref="TagHelperAttributeDescriptor"/> is used for dictionary indexer
        /// assignments.
        /// </summary>
        /// <value>
        /// If <c>true</c> this <see cref="TagHelperAttributeDescriptor"/> should be associated with all HTML
        /// attributes that have names starting with <see cref="Name"/>. Otherwise this
        /// <see cref="TagHelperAttributeDescriptor"/> is used for property assignment and is only associated with an
        /// HTML attribute that has the exact <see cref="Name"/>.
        /// </value>
        /// <remarks>
        /// HTML attribute names are matched case-insensitively, regardless of <see cref="IsDictionary"/>.
        /// </remarks>
        public bool IsDictionary { get; set; }

        /// <summary>
        /// Gets or sets an indication whether this property is an <see cref="Enum"/>.
        /// </summary>
        public bool IsEnum { get; set; }

        /// <summary>
        /// Gets or sets an indication whether this property is of type <see cref="string"/> or, if
        /// <see cref="IsDictionary"/> is <c>true</c>, whether the indexer's value is of type <see cref="string"/>.
        /// </summary>
        /// <value>
        /// If <c>true</c> the <see cref="TypeName"/> is for <see cref="string"/>. This causes the Razor parser
        /// to allow empty values for HTML attributes matching this <see cref="TagHelperAttributeDescriptor"/>. If
        /// <c>false</c> empty values for such matching attributes lead to errors.
        /// </value>
        public bool IsStringProperty { get; set; }

        /// <summary>
        /// The HTML attribute name or, if <see cref="IsDictionary"/> is <c>true</c>, the prefix for matching attribute
        /// names.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _name = value;
            }
        }

        /// <summary>
        /// The full name of the named (see <see name="PropertyName"/>) property's <see cref="Type"/> or, if
        /// <see cref="IsDictionary"/> is <c>true</c>, the full name of the indexer's value <see cref="Type"/>.
        /// </summary>
        public string TypeName
        {
            get
            {
                return _typeName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _typeName = value;
                IsStringProperty = string.Equals(TypeName, typeof(string).FullName, StringComparison.Ordinal);
            }
        }

        /// <summary>
        /// A summary of how to use a tag helper.
        /// </summary>
        public string Summary { get; set; }
    }
}