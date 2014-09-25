// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Defines a contract that is used to run <see cref="TagHelper"/>s.
    /// </summary>
    public interface ITagHelperRunner
    {
        /// <summary>
        /// Calls the <see cref="TagHelper.ProcessAsync(TagHelperOutput, TagHelperContext)"/> method on
        /// <see cref="TagHelper"/>s.
        /// </summary>
        /// <param name="context">Contains information associated with running <see cref="TagHelper"/>s</param>
        /// <returns>A the resulting <see cref="TagHelperOutput"/> from processing all of the 
        /// <paramref name="context"/>s <see cref="TagHelper"/>s.</returns>
        Task<TagHelperOutput> RunAsync(TagHelpersExecutionContext context);

        /// <summary>
        /// Calls the <see cref="TagHelper.ProcessAsync(TagHelperOutput, TagHelperContext)"/> method on
        /// <see cref="TagHelper"/>s with a <see cref="TagHelperOutput"/> whos <see cref="TagHelperOutput.Content"/>
        /// is set to the given <paramref name="bufferBody"/> <see cref="string"/> value.
        /// </summary>
        /// <param name="context">Contains information associated with running <see cref="TagHelper"/>s</param>
        /// <param name="bufferedBody">Contains the buffered content of the current HTML tag that is associated
        /// with the current set of <see cref="TagHelper"/>s provided by the <paramref name="context"/>.</param>
        /// <returns>A the resulting <see cref="TagHelperOutput"/> from processing all of the 
        /// <paramref name="context"/>s <see cref="TagHelper"/>s.</returns>
        Task<TagHelperOutput> RunAsync(TagHelpersExecutionContext context, TextWriter bufferedBody);
    }
}