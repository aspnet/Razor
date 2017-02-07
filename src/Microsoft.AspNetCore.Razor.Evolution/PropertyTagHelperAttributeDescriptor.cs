// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public class PropertyTagHelperAttributeDescriptor : TagHelperAttributeDescriptor
    {
        private string _propertyName;

        public PropertyTagHelperAttributeDescriptor()
        {
        }

        public PropertyTagHelperAttributeDescriptor(PropertyTagHelperAttributeDescriptor descriptor) : base(descriptor)
        {
            PropertyName = descriptor.PropertyName;
        }

        /// <summary>
        /// The name of the CLR property that corresponds to the HTML attribute.
        /// </summary>
        public string PropertyName
        {
            get
            {
                return _propertyName;
            }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException(nameof(value));
                }

                _propertyName = value;
            }
        }
    }
}