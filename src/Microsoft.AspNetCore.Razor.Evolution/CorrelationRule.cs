// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public abstract class CorrelationRule
    {
        private IEnumerable<RazorDiagnostic> _allDiagnostics;

        public string TagName { get; protected set; }

        public IEnumerable<TagHelperRequiredAttributeDescriptor> Attributes { get; protected set; }

        public string Parent { get; protected set; }

        public TagStructure TagStructure { get; protected set; }

        public IEnumerable<RazorDiagnostic> Diagnostics { get; protected set; }

        public bool HasAnyErrors
        {
            get
            {
                var allDiagnostics = GetAllDiagnostics();
                var anyErrors = allDiagnostics.Any(diagnostic => diagnostic.Severity == RazorDiagnosticSeverity.Error);

                return anyErrors;
            }
        }

        public IEnumerable<RazorDiagnostic> GetAllDiagnostics()
        {
            if (_allDiagnostics == null)
            {
                var requiredAttributeDiagnostics = Attributes.SelectMany(attribute => attribute.Diagnostics);
                var combinedDiagnostics = Diagnostics.Concat(requiredAttributeDiagnostics);
                _allDiagnostics = combinedDiagnostics.ToArray();
            }

            return _allDiagnostics;
        }
    }
}
