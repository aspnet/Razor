// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// A <see cref="TagHelperExpression"/> that accepts Razor code in it's corresponding HTML attribute
    /// and provides the resulting value via the <see cref="Value"/> property.
    /// </summary>
    public class TagHelperRazorExpression : TagHelperExpression
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="TagHelperRazorExpression"/> class.
        /// </summary>
        /// <param name="value">The resulting value of the the HTML element's attribute.</param>
        public TagHelperRazorExpression(string value)
        {
            Value = value;
        }

        /// <summary>
        /// The result of the Razor code that was provided in this expressions HTML attribute.
        /// </summary>
        public string Value { get; private set; }
    }
}