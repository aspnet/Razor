// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// Defines a class that is used to resolve tag helper <see cref="Type"/>s.
    /// </summary>
    public interface ITagHelperTypeResolver
    {
        /// <summary>
        /// Resolves tag helper <see cref="Type"/>s.
        /// </summary>
        /// <param name="lookupText">
        /// A <see cref="string"/> location on where to find tag helper <see cref="Type"/>s.
        /// </param>
        /// <returns>An <see cref="IEnumerable{Type}"/> of <see cref="Type"/>s that represent tag helpers.</returns>
        IEnumerable<Type> Resolve(string lookupText);
    }
}