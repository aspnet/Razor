// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Specifies the contract for copying to a <see cref="TextWriter"/>.
    /// </summary>
    public interface ITextWriterCopyable
    {
        /// <summary>
        /// Method which needs to be implemented to copy the particular type of a <paramref name="writer"/>..
        /// </summary>
        /// <param name="writer">The <see cref="TextWriter"/> target.</param>
        void CopyTo(TextWriter writer);
    }
}