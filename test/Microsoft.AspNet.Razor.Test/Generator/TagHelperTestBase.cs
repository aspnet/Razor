// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class TagHelperTestBase : CSharpRazorCodeGeneratorTest
    {
        protected void RunTagHelperTest(string testName, IEnumerable<TagHelperDescriptor> tagHelperDescriptors)
        {
            RunTagHelperTest(testName, testName, tagHelperDescriptors, host => host);
        }

        protected void RunTagHelperTest(string testName, 
                                        bool designTimeMode, 
                                        IEnumerable<TagHelperDescriptor> tagHelperDescriptors)
        {
            RunTagHelperTest(testName, testName, designTimeMode, tagHelperDescriptors);
        }

        protected void RunTagHelperTest(string testName,
                                        string baseLineName,
                                        IEnumerable<TagHelperDescriptor> tagHelperDescriptors,
                                        Func<RazorEngineHost, RazorEngineHost> hostConfig)
        {
            RunTagHelperTest(testName,
                             baseLineName,
                             designTimeMode: false,
                             tagHelperDescriptors: tagHelperDescriptors,
                             hostConfig: hostConfig);
        }

        protected void RunTagHelperTest(string testName,
                                        string baseLineName,
                                        bool designTimeMode,
                                        IEnumerable<TagHelperDescriptor> tagHelperDescriptors)
        {
            RunTagHelperTest(testName, 
                             baseLineName, 
                             designTimeMode, 
                             tagHelperDescriptors, 
                             hostConfig: host => host);
        }

        protected void RunTagHelperTest(string testName,
                                        string baseLineName,
                                        bool designTimeMode,
                                        IEnumerable<TagHelperDescriptor> tagHelperDescriptors,
                                        Func<RazorEngineHost, RazorEngineHost> hostConfig)
        {
            RunTest(name: testName,
                    baselineName: baseLineName,
                    designTimeMode: designTimeMode,
                    tabTest: TabTest.NoTabs,
                    templateEngineConfig: (engine) =>
                    {
                        return new TagHelperTemplateEngine(engine, tagHelperDescriptors);
                    },
                    hostConfig: hostConfig);
        }

        private class CustomTagHelperDescriptorResolver : ITagHelperDescriptorResolver
        {
            private IEnumerable<TagHelperDescriptor> _tagHelperDescriptors;
            private bool _resolved;

            public CustomTagHelperDescriptorResolver(IEnumerable<TagHelperDescriptor> tagHelperDescriptors)
            {
                _tagHelperDescriptors = tagHelperDescriptors;
            }

            public IEnumerable<TagHelperDescriptor> Resolve(string lookupText)
            {
                if (!_resolved)
                {
                    _resolved = true;

                    return _tagHelperDescriptors;
                }
                else
                {
                    return Enumerable.Empty<TagHelperDescriptor>();
                }
            }
        }

        private class TagHelperTemplateEngine : RazorTemplateEngine
        {
            private IEnumerable<TagHelperDescriptor> _tagHelperDescriptors;

            public TagHelperTemplateEngine(RazorTemplateEngine engine,
                                           IEnumerable<TagHelperDescriptor> tagHelperDescriptors)
                : base(engine.Host)
            {
                _tagHelperDescriptors = tagHelperDescriptors;
            }

            protected internal override RazorParser CreateParser()
            {
                var parser = base.CreateParser();

                return new RazorParser(new CustomTagHelperDescriptorResolver(_tagHelperDescriptors),
                                       parser.CodeParser,
                                       parser.MarkupParser);
            }
        }
    }
}