// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Language
{
    public abstract class RazorProjectEngineBuilder
    {
        public abstract RazorProject Project { get; }

        public abstract ICollection<IRazorProjectEngineFeature> Features { get; }

        public abstract RazorProjectEngine Build();
    }
}
