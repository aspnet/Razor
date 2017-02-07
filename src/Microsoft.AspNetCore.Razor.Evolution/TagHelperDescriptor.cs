// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    /// <summary>
    /// A metadata class describing a tag helper.
    /// </summary>
    public class TagHelperDescriptor
    {
        private string _assemblyName;
        private IDictionary<string, string> _propertyBag;
        private IEnumerable<CorrelationRequirement> _requirements;
        private IEnumerable<TagHelperAttributeDescriptor> _attributes =
            Enumerable.Empty<TagHelperAttributeDescriptor>();

        /// <summary>
        /// Creates a new <see cref="TagHelperDescriptor"/>.
        /// </summary>
        public TagHelperDescriptor()
        {
        }

        /// <summary>
        /// Creates a shallow copy of the given <see cref="TagHelperDescriptor"/>.
        /// </summary>
        /// <param name="descriptor">The <see cref="TagHelperDescriptor"/> to copy.</param>
        public TagHelperDescriptor(TagHelperDescriptor descriptor)
        {
            CorrelationRequirements = descriptor.CorrelationRequirements;
            AssemblyName = descriptor.AssemblyName;
            Attributes = descriptor.Attributes;
            AllowedChildren = descriptor.AllowedChildren;
            Summary = descriptor.Summary;
            OutputElementHint = descriptor.OutputElementHint;

            foreach (var property in descriptor.PropertyBag)
            {
                PropertyBag.Add(property.Key, property.Value);
            }
        }

        public IEnumerable<CorrelationRequirement> CorrelationRequirements
        {
            get
            {
                return _requirements;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _requirements = value;
            }
        }

        /// <summary>
        /// The name of the assembly containing the tag helper class.
        /// </summary>
        public string AssemblyName
        {
            get
            {
                return _assemblyName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _assemblyName = value;
            }
        }

        /// <summary>
        /// The list of attributes the tag helper expects.
        /// </summary>
        public IEnumerable<TagHelperAttributeDescriptor> Attributes
        {
            get
            {
                return _attributes;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _attributes = value;
            }
        }

        /// <summary>
        /// Get the names of elements allowed as children.
        /// </summary>
        /// <remarks><c>null</c> indicates all children are allowed.</remarks>
        public IEnumerable<string> AllowedChildren { get; set; }

        /// <summary>
        /// A summary of how to use a tag helper.
        /// </summary>
        public string Summary { get; set; }

        /// <summary>
        /// The HTML element a tag helper may output.
        /// </summary>
        /// <remarks>
        /// In IDEs supporting IntelliSense, may override the HTML information provided at design time.
        /// </remarks>
        public string OutputElementHint { get; set; }

        /// <summary>
        /// A dictionary containing additional information about the <see cref="TagHelperDescriptor"/>.
        /// </summary>
        public IDictionary<string, string> PropertyBag
        {
            get
            {
                if (_propertyBag == null)
                {
                    _propertyBag = new Dictionary<string, string>();
                }

                return _propertyBag;
            }
        }
    }
}
