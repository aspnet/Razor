// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// A generic tag helper expression that is used to generate property values.
    /// </summary>
    /// <typeparam name="TBuildType">The value type to generate.</typeparam>
    public abstract class TagHelperExpression<TBuildType> : TagHelperExpression
    {
        /// <summary>
        /// Builds the current value of the expression.
        /// </summary>
        /// <param name="context">The tag helper's process context.</param>
        /// <returns>The typed value of the expression.</returns>
        public abstract TBuildType Build(TagHelperContext context);
    }
}