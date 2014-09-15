// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Parser.TagHelpers.Internal;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class TagHelperTestBase : CSharpRazorCodeGeneratorTest
    {
        protected void RunTagHelperTest(string testName, TagHelperDescriptorProvider tagHelperProvider)
        {
            RunTagHelperTest(testName, testName, tagHelperProvider, (host) => host);
        }

        protected void RunTagHelperTest(string testName,
                                        string baseLineName,
                                        TagHelperDescriptorProvider tagHelperProvider,
                                        Func<RazorEngineHost, RazorEngineHost> hostConfig)
        {
            RunTest(name: testName,
                    baselineName: baseLineName,
                    templateEngineConfig: (engine) =>
                    {
                        return new TagHelperTemplateEngine(engine, tagHelperProvider);
                    },
                    hostConfig: hostConfig);
        }

        private class TagHelperTemplateEngine : RazorTemplateEngine
        {
            private TagHelperDescriptorProvider _tagHelperProvider;

            public TagHelperTemplateEngine(RazorTemplateEngine engine, TagHelperDescriptorProvider tagHelperProvider)
                : base(engine.Host)
            {
                _tagHelperProvider = tagHelperProvider;
            }

            protected internal override RazorParser CreateParser()
            {
                var parser = base.CreateParser();
                var optimizers = parser.Optimizers.Where(opmzr => !(opmzr is TagHelperParseTreeRewriter));

                parser.Optimizers = optimizers.Concat(new[] {
                    new TagHelperParseTreeRewriter(_tagHelperProvider)
                }).ToList();

                return parser;
            }
        }
    }
}