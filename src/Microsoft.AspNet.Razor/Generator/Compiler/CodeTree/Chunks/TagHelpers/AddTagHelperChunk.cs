// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.Generator.Compiler
{
    /// <summary>
    /// A <see cref="Chunk"/> that is used to lookup <see cref="TagHelpers.TagHelperDescriptor"/>s.
    /// </summary>
    public class AddTagHelperChunk : Chunk
    {
        /// <summary>
        /// Arbitrary text used to lookup <see cref="TagHelpers.TagHelperDescriptor"/>s.
        /// </summary>
        public string LookupText { get; set; }
    }
}