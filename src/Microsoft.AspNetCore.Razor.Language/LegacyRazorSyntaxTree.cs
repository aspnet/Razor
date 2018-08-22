// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class LegacyRazorSyntaxTree : RazorSyntaxTree
    {
        public LegacyRazorSyntaxTree(
            Block root,
            RazorSourceDocument source,
            IReadOnlyList<RazorDiagnostic> diagnostics,
            RazorParserOptions options)
        {
            Root = root;
            Source = source;
            Diagnostics = diagnostics;
            Options = options;
        }

        public override IReadOnlyList<RazorDiagnostic> Diagnostics { get; }

        public override RazorParserOptions Options { get; }

        internal override Block Root { get; }

        public override RazorSourceDocument Source { get; }
    }
}
