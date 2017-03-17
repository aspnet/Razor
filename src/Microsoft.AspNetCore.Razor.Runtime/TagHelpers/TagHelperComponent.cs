// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Razor.Runtime.TagHelpers
{
    public class TagHelperComponent : ITagHelperComponent
    {
        /// <inheritdoc />
        /// <remarks>Default order is <c>0</c>.</remarks>
        public virtual int Order => 0;

        /// <inheritdoc />
        public virtual void Init(TagHelperContext context)
        {
        }

        /// <inheritdoc />
        public virtual Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            return TaskCache.CompletedTask;
        }

        /// <summary>
        /// Returns <c>true</c> to indicate if <see cref="ITagHelperComponent"/> applies to the given <see cref="TagHelperContext"/>.
        /// </summary>
        /// <param name="context">Contains information associated with the current HTML tag.</param>
        public virtual bool AppliesTo(TagHelperContext context)
        {
            return false;
        }
    }
}
