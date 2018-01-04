// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultRazorProjectEngineBuilder : RazorProjectEngineBuilder
    {
        private readonly RazorEngine _engine;

        public DefaultRazorProjectEngineBuilder(RazorEngine engine, RazorProject project)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            _engine = engine;
            Project = project;
            Features = new List<IRazorProjectEngineFeature>();
        }

        public override RazorProject Project { get; }

        public override ICollection<IRazorProjectEngineFeature> Features { get; }

        public override RazorProjectEngine Build()
        {
            var features = new IRazorProjectEngineFeature[Features.Count];
            Features.CopyTo(features, arrayIndex: 0);

            return new DefaultRazorProjectEngine(_engine, Project, features);
        }
    }
}
