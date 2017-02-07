// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public class ITagHelperDescriptor : TagHelperDescriptor
    {
        private string _typeName;

        public ITagHelperDescriptor()
        {
        }

        public ITagHelperDescriptor(ITagHelperDescriptor descriptor) : base(descriptor)
        {
            TypeName = descriptor.TypeName;
        }

        public new IEnumerable<PropertyTagHelperAttributeDescriptor> Attributes
        {
            get
            {
                return base.Attributes as IEnumerable<PropertyTagHelperAttributeDescriptor>;
            }
            set
            {
                base.Attributes = value;
            }
        }

        /// <summary>
        /// The full name of the tag helper class.
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
            }
        }
    }
}
