// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    /// <summary>
    /// A metadata class describing a tag helper attribute.
    /// </summary>
    public abstract class TagHelperAttributeDescriptor
    {
        protected TagHelperAttributeDescriptor(string kind)
        {
            Kind = kind;
        }

        public string Kind { get; }

        /// <summary>
        /// Gets or sets an indication whether this property is of type <see cref="string"/> or, if
        /// <see cref="IsDictionary"/> is <c>true</c>, whether the indexer's value is of type <see cref="string"/>.
        /// </summary>
        /// <value>
        /// If <c>true</c> the <see cref="TypeName"/> is for <see cref="string"/>. This causes the Razor parser
        /// to allow empty values for HTML attributes matching this <see cref="TagHelperAttributeDescriptor"/>. If
        /// <c>false</c> empty values for such matching attributes lead to errors.
        /// </value>
        public bool IsKeyValueStringProperty { get; protected set; }

        /// <summary>
        /// Gets or sets an indication whether this property is an <see cref="Enum"/>.
        /// </summary>
        public bool IsEnum { get; protected set; }

        /// <summary>
        /// Gets or sets an indication whether this property is of type <see cref="string"/> or, if
        /// <see cref="IsDictionary"/> is <c>true</c>, whether the indexer's value is of type <see cref="string"/>.
        /// </summary>
        /// <value>
        /// If <c>true</c> the <see cref="TypeName"/> is for <see cref="string"/>. This causes the Razor parser
        /// to allow empty values for HTML attributes matching this <see cref="TagHelperAttributeDescriptor"/>. If
        /// <c>false</c> empty values for such matching attributes lead to errors.
        /// </value>
        public bool IsStringProperty { get; protected set; }

        /// <summary>
        /// The HTML attribute name or, if <see cref="IsDictionary"/> is <c>true</c>, the prefix for matching attribute
        /// names.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// The full name of the named (see <see name="PropertyName"/>) property's <see cref="Type"/> or, if
        /// <see cref="IsDictionary"/> is <c>true</c>, the full name of the indexer's value <see cref="Type"/>.
        /// </summary>
        public string TypeName { get; protected set; }

        public string KeyValueTypeName { get; protected set; }

        public IReadOnlyDictionary<string, string> PropertyBag { get; protected set; }

        /// <summary>
        /// A summary of how to use a tag helper.
        /// </summary>
        public string Documentation { get; protected set; }

        public string DisplayName { get; protected set; }

        public IEnumerable<RazorDiagnostic> Diagnostics { get; protected set; }

        public bool HasAnyErrors
        {
            get
            {
                var anyErrors = Diagnostics.Any(diagnostic => diagnostic.Severity == RazorDiagnosticSeverity.Error);

                return anyErrors;
            }
        }
    }
}