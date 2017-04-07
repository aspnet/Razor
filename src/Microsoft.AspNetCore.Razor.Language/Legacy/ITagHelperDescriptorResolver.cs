// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    /// <summary>
    /// Contract used to resolve <see cref="TagHelperDescriptor"/>s.
    /// </summary>
    public interface ITagHelperDescriptorResolver
    {
        IEnumerable<TagHelperDescriptor> Resolve(IList<RazorDiagnostic> errors);
    }
}