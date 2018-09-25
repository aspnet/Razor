// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal class TagHelperAttributeInfo
    {
        public TagHelperAttributeInfo(
            string name,
            AttributeStructure attributeStructure)
        {
            Name = name;
            AttributeStructure = attributeStructure;
        }

        public string Name { get; }

        public AttributeStructure AttributeStructure { get; }
    }
}
