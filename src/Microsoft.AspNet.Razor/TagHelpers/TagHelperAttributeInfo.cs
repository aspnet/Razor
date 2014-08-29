// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// A metadata class that defines the functionality of tag helper html attributes.
    /// </summary>
    public class TagHelperAttributeInfo
    {
        /// <summary>
        /// Instantiates a new <see cref="TagHelperAttributeInfo"/> class.
        /// </summary>
        /// <param name="attributeName">The HTML attribute name.</param>
        /// <param name="attributePropertyName">The property name that corresponds to the HTML 
        /// attribute name.</param>
        /// <param name="acceptsRazorCode"><c>true</c> if the described HTML attribute accepts raw Razor 
        /// code. <c>false</c> otherwise.</param>
        /// <param name="codeGenerator">The <see cref="TagHelperAttributeCodeGenerator"/> that is used to
        /// generate the code to instantiate the <paramref name="attributePropertyName"/> property on the
        /// tag helper.</param>
        public TagHelperAttributeInfo(string attributeName,
                                      string attributePropertyName,
                                      bool acceptsRazorCode,
                                      TagHelperAttributeCodeGenerator codeGenerator)
        {
            AttributeName = attributeName;
            AttributePropertyName = attributePropertyName;
            AcceptsRazorCode = acceptsRazorCode;
            CodeGenerator = codeGenerator;
        }

        /// <summary>
        /// The HTML attribute name mapping.
        /// </summary>
        public string AttributeName { get; private set; }

        /// <summary>
        /// <c>true</c> if the described HTML attribute accepts raw Razor code. <c>false</c> otherwise.
        /// </summary>
        public string AttributePropertyName { get; private set; }

        /// <summary>
        /// Indicates whether or not the attribute accepts raw Razor code.
        /// </summary>
        public bool AcceptsRazorCode { get; private set; }

        /// <summary>
        /// The <see cref="TagHelperAttributeCodeGenerator"/> that is used to generate the code to instantiate 
        /// the <paramref name="attributePropertyName"/> property on the tag helper.
        /// </summary>
        public TagHelperAttributeCodeGenerator CodeGenerator { get; private set; }
    }
}