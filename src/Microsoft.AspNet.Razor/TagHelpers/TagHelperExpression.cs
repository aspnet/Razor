// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// An expression abstract expression used to represent tag helper HTML attribute's.
    /// </summary>
    public abstract class TagHelperExpression
    {
        // Internal for testing purposes.
        internal static string IsSetPropertyName = "IsSet";

        /// <summary>
        /// Indicates whether or not the tag helper expression was set in the HTML.
        /// </summary>
        public virtual bool IsSet { get; set; }
    }
}