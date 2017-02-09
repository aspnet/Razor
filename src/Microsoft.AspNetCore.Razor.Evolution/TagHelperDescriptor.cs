// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    /// <summary>
    /// A metadata class describing a tag helper.
    /// </summary>
    public abstract class TagHelperDescriptor
    {
        private IEnumerable<RazorDiagnostic> _allDiagnostics;

        protected TagHelperDescriptor(string kind)
        {
            Kind = kind;
        }

        public string Kind { get; }

        public IEnumerable<CorrelationRule> CorrelationRules { get; protected set; }

        /// <summary>
        /// The name of the assembly containing the tag helper class.
        /// </summary>
        public string AssemblyName { get; protected set; }

        /// <summary>
        /// The list of attributes the tag helper expects.
        /// </summary>
        public IEnumerable<TagHelperAttributeDescriptor> Attributes { get; protected set; }

        /// <summary>
        /// Get the names of elements allowed as children.
        /// </summary>
        /// <remarks><c>null</c> indicates all children are allowed.</remarks>
        public IEnumerable<string> AllowedChildren { get; protected set; }

        /// <summary>
        /// A dictionary containing additional information about the <see cref="TagHelperDescriptor"/>.
        /// </summary>
        public IReadOnlyDictionary<string, string> PropertyBag { get; protected set; }

        public string DisplayName { get; protected set; }

        public string Documentation { get; protected set; }

        /// <summary>
        /// The HTML element a tag helper may output.
        /// </summary>
        /// <remarks>
        /// In IDEs supporting IntelliSense, may override the HTML information provided at design time.
        /// </remarks>
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
