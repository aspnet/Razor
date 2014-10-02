// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Defines a contract that is used to control rendering of HTML elements.
    /// </summary>
    public interface ITagHelper
    {
        /// <summary>
        /// Synchronously executes the <see cref="ITagHelper"/> with the given <paramref name="output"/> and
        /// <paramref name="context"/>.
        /// </summary>
        /// <param name="output">The stateful HTML element used to represent a tag</param>
        /// <param name="context">Contains information associated with the current HTML tags execution.</param>
        /// <returns>A task that represents when the method has completed.</returns>
        Task ProcessAsync(TagHelperOutput output, TagHelperContext context);
    }
}