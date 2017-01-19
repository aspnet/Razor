// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Evolution.Legacy;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    internal class DefaultRazorSyntaxTree : RazorSyntaxTree
    {
        public DefaultRazorSyntaxTree(
            Block root,
            RazorSourceDocument source,
            HtmlLanguageCharacteristics htmlLanguage,
            CSharpLanguageCharacteristics cSharpLanguage,
            IReadOnlyList<RazorError> diagnostics,
            RazorParserOptions options)
        {
            Root = root;
            Source = source;
            HtmlLanguage = htmlLanguage;
            CSharpLanguage = cSharpLanguage;
            Diagnostics = diagnostics;
            Options = options;
        }

        internal override IReadOnlyList<RazorError> Diagnostics { get; }

        public override RazorParserOptions Options { get; }

        internal override Block Root { get; }

        public override RazorSourceDocument Source { get; }

        internal override HtmlLanguageCharacteristics HtmlLanguage { get; }

        internal override CSharpLanguageCharacteristics CSharpLanguage { get; }
    }
}
