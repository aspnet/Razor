// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultRazorProjectEngineBuilder : RazorProjectEngineBuilder
    {
        public DefaultRazorProjectEngineBuilder(bool designTime, RazorProjectFileSystem projectFileSystem)
        {
            if (projectFileSystem == null)
            {
                throw new ArgumentNullException(nameof(projectFileSystem));
            }

            DesignTime = designTime;
            ProjectFileSystem = projectFileSystem;
            Features = new List<IRazorFeature>();
            Phases = new List<IRazorEnginePhase>();
        }

        public override RazorProjectFileSystem ProjectFileSystem { get; }

        public override ICollection<IRazorFeature> Features { get; }

        public override IList<IRazorEnginePhase> Phases { get; }

        public override bool DesignTime { get; }

        public override RazorProjectEngine Build()
        {
            RazorEngine engine = null;

            if (DesignTime)
            {
                engine = RazorEngine.CreateDesignTimeEmpty(ConfigureRazorEngine);
            }
            else
            {
                engine = RazorEngine.CreateEmpty(ConfigureRazorEngine);
            }

            var projectEngineFeatures = Features.OfType<IRazorProjectEngineFeature>().ToArray();
            var projectEngine = new DefaultRazorProjectEngine(engine, ProjectFileSystem, projectEngineFeatures);

            return projectEngine;
        }

        private void ConfigureRazorEngine(IRazorEngineBuilder engineBuilder)
        {
            var engineFeatures = Features.OfType<IRazorEngineFeature>();
            foreach (var engineFeature in engineFeatures)
            {
                engineBuilder.Features.Add(engineFeature);
            }

            for (var i = 0; i < Phases.Count; i++)
            {
                engineBuilder.Phases.Add(Phases[i]);
            }
        }
    }
}
