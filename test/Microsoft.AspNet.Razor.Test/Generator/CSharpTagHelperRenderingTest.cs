// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Reflection;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.TagHelpers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class CSharpTagHelperRenderingTest : TagHelperTestBase
    {
        [Fact]
        public void CSharpCodeGeneratorCorrectlyGeneratesMappingsForAddTagHelperDirective()
        {
            RunTest("AddTagHelperDirective",
                    designTimeMode: true,
                    tabTest: TabTest.NoTabs,
                    expectedDesignTimePragmas: new List<LineMapping>()
                    {
                        BuildLineMapping(14, 0, 440, 14, 14, 11)
                    });
        }

        [Fact]
        public void TagHelpers_DirectivesGenerateDesignTimeMappings()
        {
            // Act & Assert
            RunTagHelperTest(testName: "AddTagHelperDirective",
                             designTimeMode: true,
                             tagHelperDescriptors: new[] {
                                new TagHelperDescriptor("p", "pTagHelper", ContentBehavior.None)
                             });
        }

        [Theory]
        [InlineData("SingleTagHelper")]
        [InlineData("BasicTagHelpers")]
        [InlineData("ComplexTagHelpers")]
        public void TagHelpers_ChangeGeneratedOutput(string testType)
        {
            var pFooPropertyInfo = new Mock<PropertyInfo>();
            pFooPropertyInfo.Setup(ppi => ppi.PropertyType).Returns(typeof(int));
            pFooPropertyInfo.Setup(ppi => ppi.Name).Returns("Foo");
            var inputTypePropertyInfo = new Mock<PropertyInfo>();
            inputTypePropertyInfo.Setup(ipi => ipi.PropertyType).Returns(typeof(string));
            inputTypePropertyInfo.Setup(ipi => ipi.Name).Returns("Type");
            var checkedPropertyInfo = new Mock<PropertyInfo>();
            checkedPropertyInfo.Setup(ipi => ipi.PropertyType).Returns(typeof(bool));
            checkedPropertyInfo.Setup(ipi => ipi.Name).Returns("Checked");
            // Arrange
            var tagHelperDescriptors = new TagHelperDescriptor[]
            {
                new TagHelperDescriptor("p",
                                        "PTagHelper", 
                                        ContentBehavior.None,
                                        new [] {
                                            new TagHelperAttributeDescriptor("foo", pFooPropertyInfo.Object)
                                        }),
                new TagHelperDescriptor("input",
                                        "InputTagHelper",
                                        ContentBehavior.None,
                                        new TagHelperAttributeDescriptor[] {
                                            new TagHelperAttributeDescriptor("type", inputTypePropertyInfo.Object)
                                        }),
                new TagHelperDescriptor("input",
                                        "InputTagHelper2",
                                        ContentBehavior.None,
                                        new TagHelperAttributeDescriptor[] {
                                            new TagHelperAttributeDescriptor("type", inputTypePropertyInfo.Object),
                                            new TagHelperAttributeDescriptor("checked", checkedPropertyInfo.Object)
                                        })
            };

            // Act & Assert
            RunTagHelperTest(testType, tagHelperDescriptors);
        }

        [Fact]
        public void TagHelpers_ContentBehavior()
        {
            // Arrange
            var tagHelperDescriptors = new TagHelperDescriptor[]
            {
                    new TagHelperDescriptor("modify", "ModifyTagHelper", ContentBehavior.Modify),
                    new TagHelperDescriptor("none", "NoneTagHelper", ContentBehavior.None),
                    new TagHelperDescriptor("append", "AppendTagHelper", ContentBehavior.Append),
                    new TagHelperDescriptor("prepend", "PrependTagHelper", ContentBehavior.Prepend),
                    new TagHelperDescriptor("replace", "ReplaceTagHelper", ContentBehavior.Replace),
            };

            // Act & Assert
            RunTagHelperTest("ContentBehaviorTagHelpers", tagHelperDescriptors);
        }
    }
}