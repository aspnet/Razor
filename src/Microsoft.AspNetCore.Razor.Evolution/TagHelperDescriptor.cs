// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public abstract class TagHelperDescriptor
    {
        private IEnumerable<RazorDiagnostic> _allDiagnostics;

        protected TagHelperDescriptor(string kind)
        {
            Kind = kind;
        }

        public string Kind { get; }

        public IEnumerable<CorrelationRule> CorrelationRules { get; protected set; }

        public string AssemblyName { get; protected set; }

        public IEnumerable<TagHelperAttributeDescriptor> Attributes { get; protected set; }

        public IEnumerable<string> AllowedChildren { get; protected set; }

        public IReadOnlyDictionary<string, string> PropertyBag { get; protected set; }

        public string DisplayName { get; protected set; }

        public string Documentation { get; protected set; }

        public string OutputElementHint { get; protected set; }

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
                var attributeErrors = Attributes.SelectMany(attribute => attribute.Diagnostics);
                var ruleErrors = CorrelationRules.SelectMany(rule => rule.GetAllDiagnostics());
                var combinedDiagnostics = attributeErrors.Concat(ruleErrors);
                _allDiagnostics = combinedDiagnostics.ToArray();
            }

            return _allDiagnostics;
        }
    }
}
