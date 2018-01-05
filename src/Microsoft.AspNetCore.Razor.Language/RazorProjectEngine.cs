// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Language
{
    public abstract class RazorProjectEngine
    {
        // REVIEWERS: Technically could do away with this because a user could construct each of the various features with the project that
        // was passed to the RazorProjectEngine. However, it's super nice to have this available given the name of this class and the common
        // need to have the project. Thoughts?
        public abstract RazorProject Project { get; }

        public abstract IReadOnlyList<IRazorProjectEngineFeature> Features { get; }

        public abstract RazorProjectEngineResult Process(string filePath);

        public abstract RazorProjectEngineResult Process(RazorSourceDocument sourceDocument);

        public static RazorProjectEngine Create(RazorEngine engine, RazorProject project) => Create(engine, project, configure: null);

        public static RazorProjectEngine Create(
            RazorEngine engine,
            RazorProject project,
            Action<RazorProjectEngineBuilder> configure)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            var builder = new DefaultRazorProjectEngineBuilder(engine, project);

            AddDefaults(builder);
            configure?.Invoke(builder);

            return builder.Build();
        }

        private static void AddDefaults(RazorProjectEngineBuilder builder)
        {
            builder.Features.Add(new DefaultRazorImportFeature());
            builder.Features.Add(new DefaultImportDiscoverer());
            builder.Features.Add(new DefaultCodeDocumentFeature());
            builder.Features.Add(new DefaultImportItemFeature());
        }
    }
}
