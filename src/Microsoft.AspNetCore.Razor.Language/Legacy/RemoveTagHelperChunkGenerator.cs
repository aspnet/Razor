// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Internal;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal class RemoveTagHelperChunkGenerator : SpanChunkGenerator
    {
        public RemoveTagHelperChunkGenerator(
            string lookupText,
            string directiveText,
            string typePattern,
            string assemblyName,
            List<RazorDiagnostic> diagnostics)
        {
            LookupText = lookupText;
            DirectiveText = directiveText;
            TypePattern = typePattern;
            AssemblyName = assemblyName;
            Diagnostics = diagnostics;
        }

        public string LookupText { get; }

        public string DirectiveText { get; set; }

        public string TypePattern { get; set; }

        public string AssemblyName { get; set; }

        public List<RazorDiagnostic> Diagnostics { get; }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            var other = obj as RemoveTagHelperChunkGenerator;
            return base.Equals(other) &&
                Enumerable.SequenceEqual(Diagnostics, other.Diagnostics) &&
                string.Equals(LookupText, other.LookupText, StringComparison.Ordinal) &&
                string.Equals(DirectiveText, other.DirectiveText, StringComparison.Ordinal) &&
                string.Equals(TypePattern, other.TypePattern, StringComparison.Ordinal) &&
                string.Equals(AssemblyName, other.AssemblyName, StringComparison.Ordinal);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            var combiner = HashCodeCombiner.Start();
            combiner.Add(base.GetHashCode());
            combiner.Add(LookupText, StringComparer.Ordinal);
            combiner.Add(DirectiveText, StringComparer.Ordinal);
            combiner.Add(TypePattern, StringComparer.Ordinal);
            combiner.Add(AssemblyName, StringComparer.Ordinal);

            return combiner.CombinedHash;
        }

        public override string ToString()
        {
            var builder = new StringBuilder("RemoveTagHelper:{");
            builder.Append(LookupText);
            builder.Append(";");
            builder.Append(DirectiveText);
            builder.Append(";");
            builder.Append(TypePattern);
            builder.Append(";");
            builder.Append(AssemblyName);
            builder.Append("}");

            if (Diagnostics.Count > 0)
            {
                builder.Append(" [");
                var ids = string.Join(", ", Diagnostics.Select(diagnostic => $"{diagnostic.Id}{diagnostic.Span}"));
                builder.Append(ids);
                builder.Append("]");
            }

            return builder.ToString();
        }
    }
}