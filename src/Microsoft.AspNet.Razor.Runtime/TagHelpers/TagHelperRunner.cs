// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// A class used to run <see cref="TagHelper"/>s during runtime.
    /// </summary>
    public class TagHelperRunner : ITagHelperRunner
    {
        /// <inheritdoc />
        public async Task<TagHelperOutput> RunAsync(TagHelpersExecutionContext context)
        {
            return await RunAsyncCore(context, string.Empty);
        }

        /// <inheritdoc />
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