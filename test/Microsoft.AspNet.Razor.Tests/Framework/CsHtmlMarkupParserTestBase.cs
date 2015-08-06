// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Razor.Parser;

namespace Microsoft.AspNet.Razor.Tests.Framework
{
    public abstract class CsHtmlMarkupParserTestBase : MarkupParserTestBase
    {
        protected override ISet<string> KeywordSet
        {
            get { return CSharpCodeParser.DefaultKeywords; }
        }

        protected override BlockFactory CreateBlockFactory()
        {
            return new BlockFactory(Factory ?? CreateSpanFactory());
        }

        protected override SpanFactory CreateSpanFactory()
        {
            return SpanFactory.CreateCsHtml();
        }

        public override ParserBase CreateMarkupParser()
        {
            return new HtmlMarkupParser();
        }

        public override ParserBase CreateCodeParser()
        {
            return new CSharpCodeParser();
        }
    }
}
