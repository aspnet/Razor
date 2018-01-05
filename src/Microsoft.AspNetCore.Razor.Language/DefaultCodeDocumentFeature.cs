// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultCodeDocumentFeature : RazorProjectEngineFeatureBase, IRazorCodeDocumentFeature
    {
        private IEnumerable<IRazorCodeDocumentProcessor> _processors;

        public RazorCodeDocument GetCodeDocument(RazorSourceDocument sourceDocument, IEnumerable<RazorSourceDocument> imports)
        {
            if (sourceDocument == null)
            {
                throw new ArgumentNullException(nameof(sourceDocument));
            }

            if (imports == null)
            {
                throw new ArgumentNullException(nameof(imports));
            }

            var codeDocument = RazorCodeDocument.Create(sourceDocument, imports);
            foreach (var processor in _processors)
            {
                processor.Process(codeDocument);
            }

            return codeDocument;
        }

        protected override void OnInitialized()
        {
            _processors = Engine.Features.OfType<IRazorCodeDocumentProcessor>();
        }
    }
}
