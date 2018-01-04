// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultRazorProjectEngine : RazorProjectEngine
    {
        private readonly RazorEngine _engine;

        public DefaultRazorProjectEngine(
            RazorEngine engine,
            RazorProject project,
            IReadOnlyList<IRazorProjectEngineFeature> features)
        {
            if (engine == null)
            {
                throw new ArgumentNullException(nameof(engine));
            }

            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            if (features == null)
            {
                throw new ArgumentNullException(nameof(features));
            }

            _engine = engine;
            Project = project;
            Features = features;

            for (var i = 0; i < features.Count; i++)
            {
                features[i].Engine = this;
            }
        }

        public override RazorProject Project { get; }

        public override IReadOnlyList<IRazorProjectEngineFeature> Features { get; }

        public override RazorProjectEngineResult Process(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException(Resources.ArgumentCannotBeNullOrEmpty, nameof(filePath));
            }

            var projectItem = Project.GetItem(filePath);
            var sourceDocument = RazorSourceDocument.ReadFrom(projectItem);
            var csharpDocument = Process(sourceDocument);

            return csharpDocument;
        }

        public override RazorProjectEngineResult Process(RazorSourceDocument sourceDocument)
        {
            if (sourceDocument == null)
            {
                throw new ArgumentNullException(nameof(sourceDocument));
            }

            var importFeature = GetRequiredFeature<IRazorImportFeature>();
            var imports = importFeature.GetImports(sourceDocument);

            var codeDocumentFeature = GetRequiredFeature<IRazorCodeDocumentFeature>();
            var codeDocument = codeDocumentFeature.GetCodeDocument(sourceDocument, imports);

            _engine.Process(codeDocument);

            var engineResult = RazorProjectEngineResult.Create(codeDocument);
            return engineResult;
        }

        private TFeature GetRequiredFeature<TFeature>() where TFeature : IRazorProjectEngineFeature
        {
            var feature = Features.OfType<TFeature>().FirstOrDefault();
            if (feature == null)
            {
                throw new InvalidOperationException(
                    Resources.FormatMissingFeatureDependency(
                        typeof(RazorProjectEngine).FullName,
                        typeof(TFeature).FullName));
            }

            return feature;
        }
    }
}
