// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNetCore.Razor.Language
{
    public abstract class RazorProjectEngine
    {
        public abstract RazorProjectFileSystem ProjectFileSystem { get; }

        public abstract RazorEngine Engine { get; }

        public abstract IReadOnlyList<IRazorProjectEngineFeature> Features { get; }

        public abstract RazorCodeDocument Process(string filePath);

        public abstract RazorCodeDocument Process(RazorSourceDocument sourceDocument);

        public static RazorProjectEngine Create(RazorProjectFileSystem projectFileSystem) => Create(projectFileSystem, configure: null);

        public static RazorProjectEngine Create(RazorProjectFileSystem projectFileSystem, Action<RazorProjectEngineBuilder> configure)
        {
            if (projectFileSystem == null)
            {
                throw new ArgumentNullException(nameof(projectFileSystem));
            }

            var builder = new DefaultRazorProjectEngineBuilder(designTime: false, projectFileSystem: projectFileSystem);

            AddDefaults(builder);
            AddRuntimeDefaults(builder);
            configure?.Invoke(builder);

            return builder.Build();
        }

        public static RazorProjectEngine CreateDesignTime(RazorProjectFileSystem projectFileSystem) => CreateDesignTime(projectFileSystem, configure: null);

        public static RazorProjectEngine CreateDesignTime(RazorProjectFileSystem projectFileSystem, Action<RazorProjectEngineBuilder> configure)
        {
            if (projectFileSystem == null)
            {
                throw new ArgumentNullException(nameof(projectFileSystem));
            }

            var builder = new DefaultRazorProjectEngineBuilder(designTime: true, projectFileSystem: projectFileSystem);

            AddDefaults(builder);
            AddDesignTimeDefaults(builder);
            configure?.Invoke(builder);

            return builder.Build();
        }

        public static RazorProjectEngine CreateEmpty(RazorProjectFileSystem projectFileSystem, Action<RazorProjectEngineBuilder> configure)
        {
            if (projectFileSystem == null)
            {
                throw new ArgumentNullException(nameof(projectFileSystem));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = new DefaultRazorProjectEngineBuilder(designTime: false, projectFileSystem: projectFileSystem);

            configure(builder);

            return builder.Build();
        }

        public static RazorProjectEngine CreateDesignTimeEmpty(RazorProjectFileSystem projectFileSystem, Action<RazorProjectEngineBuilder> configure)
        {
            if (projectFileSystem == null)
            {
                throw new ArgumentNullException(nameof(projectFileSystem));
            }

            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            var builder = new DefaultRazorProjectEngineBuilder(designTime: true, projectFileSystem: projectFileSystem);

            configure(builder);

            return builder.Build();
        }

        private static void AddDefaults(RazorProjectEngineBuilder builder)
        {
            builder.Features.Add(new DefaultImportFeature());
        }

        private static void AddDesignTimeDefaults(RazorProjectEngineBuilder builder)
        {
            var defaultEngine = RazorEngine.CreateDesignTime();

            AddEngineFeaturesAndPhases(builder, defaultEngine);
        }

        private static void AddRuntimeDefaults(RazorProjectEngineBuilder builder)
        {
            var defaultEngine = RazorEngine.Create();

            AddEngineFeaturesAndPhases(builder, defaultEngine);
        }

        private static void AddEngineFeaturesAndPhases(RazorProjectEngineBuilder builder, RazorEngine defaultEngine)
        {
            // Lift default features from the engine into the project engine builder
            for (var i = 0; i < defaultEngine.Features.Count; i++)
            {
                var engineFeature = defaultEngine.Features[i];
                builder.Features.Add(engineFeature);
            }

            // Lift default phases from the engine into the project engine builder
            for (var i = 0; i < defaultEngine.Phases.Count; i++)
            {
                var enginePhase = defaultEngine.Phases[i];
                builder.Phases.Add(enginePhase);
            }
        }
    }
}
