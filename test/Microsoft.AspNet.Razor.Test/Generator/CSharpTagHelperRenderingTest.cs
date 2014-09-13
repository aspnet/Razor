// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Parser.TagHelpers.Internal;
using Microsoft.AspNet.Razor.TagHelpers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class CSharpTagHelperRenderingTest : CSharpRazorCodeGeneratorTest
    {
        [Theory]
        [InlineData("BasicTagHelpers")]
        [InlineData("ComplexTagHelpers")]
        public void TagHelpers_ChangeGeneratedOutput(string testType)
        {
            var inputTypePropertyInfo = new Mock<PropertyInfo>();
            inputTypePropertyInfo.Setup(ipi => ipi.PropertyType).Returns(typeof(string));
            inputTypePropertyInfo.Setup(ipi => ipi.Name).Returns("Type");
            var checkedPropertyInfo = new Mock<PropertyInfo>();
            checkedPropertyInfo.Setup(ipi => ipi.PropertyType).Returns(typeof(bool));
            checkedPropertyInfo.Setup(ipi => ipi.Name).Returns("Checked");
            // Arrange
            var tagHelperProvider = new TagHelperDescriptorProvider(
                new TagHelperDescriptor[]
                {
                    new TagHelperDescriptor("p", "pTagHelper", ContentBehavior.None),
                    new TagHelperDescriptor("input",
                                            "inputTagHelper",
                                            ContentBehavior.None,
                                            new TagHelperAttributeDescriptor[] {
                                                new TagHelperAttributeDescriptor("type", inputTypePropertyInfo.Object)
                                            }),
                    new TagHelperDescriptor("input",
                                            "inputTagHelper2",
                                            ContentBehavior.None,
                                            new TagHelperAttributeDescriptor[] {
                                                new TagHelperAttributeDescriptor("type", inputTypePropertyInfo.Object),
                                                new TagHelperAttributeDescriptor("checked", checkedPropertyInfo.Object)
                                            }),
                });

            // Act & Assert
            RunTagHelperTest(testType, tagHelperProvider);
        }

        [Fact]
        public void TagHelpers_ContentBehavior()
        {
            // Arrange
            var tagHelperProvider = new TagHelperDescriptorProvider(
                new TagHelperDescriptor[]
                {
                    new TagHelperDescriptor("modify", "modifyTagHelper", ContentBehavior.Modify),
                    new TagHelperDescriptor("none", "noneTagHelper", ContentBehavior.None),
                    new TagHelperDescriptor("append", "appendTagHelper", ContentBehavior.Append),
                    new TagHelperDescriptor("prepend", "prependTagHelper", ContentBehavior.Prepend),
                    new TagHelperDescriptor("replace", "replaceTagHelper", ContentBehavior.Replace),
                });

            // Act & Assert
            RunTagHelperTest("ContentBehaviorTagHelpers", tagHelperProvider);
        }

        private void RunTagHelperTest(string testName, TagHelperDescriptorProvider tagHelperProvider)
        {
            RunTest(
                name: testName,
                templateEngineConfig: (engine) =>
                {
                    return new TagHelperTemplateEngine(engine, tagHelperProvider);
                });
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