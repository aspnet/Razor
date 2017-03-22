// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.TagHelpers;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// An abstract base class for <see cref="ITagHelperComponent"/>.
    /// </summary>
    public abstract class TagHelperComponent : ITagHelperComponent
    {
        /// <inheritdoc />
        /// <remarks>Default order is <c>0</c>.</remarks>
        public virtual int Order => 0;

        /// <inheritdoc />
        public virtual void Init(TagHelperContext context)
        {
        }

        /// <inheritdoc />
        public virtual void Process(TagHelperContext context, TagHelperOutput output)
        {
        }

        /// <inheritdoc />
        public virtual Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            Process(context, output);
            return TaskCache.CompletedTask;
        }
    }
}
