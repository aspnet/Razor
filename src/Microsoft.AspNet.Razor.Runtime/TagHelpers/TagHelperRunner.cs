// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// A class used to run <see cref="ITagHelper"/>s during runtime.
    /// </summary>
    public class TagHelperRunner
    {
        /// <summary>
        /// Calls the <see cref="ITagHelper.ProcessAsync(TagHelperOutput, TagHelperContext)"/> method on
        /// <see cref="ITagHelper"/>s.
        /// </summary>
        /// <param name="context">Contains information associated with running <see cref="ITagHelper"/>s.</param>
        /// <returns>A the resulting <see cref="TagHelperOutput"/> from processing all of the 
        /// <paramref name="context"/>s <see cref="ITagHelper"/>s.</returns>
        public async Task<TagHelperOutput> RunAsync(TagHelpersExecutionContext context)
        {
            return await RunAsyncCore(context, string.Empty);
        }

        /// <summary>
        /// Calls the <see cref="ITagHelper.ProcessAsync(TagHelperOutput, TagHelperContext)"/> method on
        /// <see cref="ITagHelper"/>s with a <see cref="TagHelperOutput"/> whos <see cref="TagHelperOutput.Content"/>
        /// is set to the given <paramref name="bufferBody"/> <see cref="string"/> value.
        /// </summary>
        /// <param name="context">Contains information associated with running <see cref="ITagHelper"/>s.</param>
        /// <param name="bufferedBody">Contains the buffered content of the current HTML tag that is associated
        /// with the current set of <see cref="ITagHelper"/>s provided by the <paramref name="context"/>.</param>
        /// <returns>A the resulting <see cref="TagHelperOutput"/> from processing all of the 
        /// <paramref name="context"/>s <see cref="ITagHelper"/>s.</returns>
        public async Task<TagHelperOutput> RunAsync(TagHelpersExecutionContext context, TextWriter bufferedBody)
        {
            return await RunAsyncCore(context, bufferedBody.ToString());
        }

        private async Task<TagHelperOutput> RunAsyncCore(TagHelpersExecutionContext executionContext, string outputContent)
        {
            var tagHelperContext = new TagHelperContext(executionContext.AllAttributes);
            var tagHelperOutput = new TagHelperOutput(executionContext.TagName, executionContext.HTMLAttributes)
            {
                Content = outputContent
            };

            foreach (var tagHelper in executionContext.TagHelpers)
            {
                await tagHelper.ProcessAsync(tagHelperOutput, tagHelperContext);
            }

            return tagHelperOutput;
        }
    }
}