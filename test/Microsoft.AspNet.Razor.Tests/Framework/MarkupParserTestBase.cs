// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;

namespace Microsoft.AspNet.Razor.Tests.Framework
{
    public abstract class MarkupParserTestBase : CodeParserTestBase
    {
        protected override ParserBase SelectActiveParser(ParserBase codeParser, ParserBase markupParser)
        {
            return markupParser;
        }

        protected virtual void SingleSpanDocumentTest(string document, BlockType blockType, SpanKind spanType)
        {
            var b = CreateSimpleBlockAndSpan(document, blockType, spanType);
            ParseDocumentTest(document, b);
        }
    }
}
