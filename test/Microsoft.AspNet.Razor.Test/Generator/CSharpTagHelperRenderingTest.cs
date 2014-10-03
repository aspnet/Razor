﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Reflection;
using Microsoft.AspNet.Razor.TagHelpers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class CSharpTagHelperRenderingTest : TagHelperTestBase
    {
        [Theory]
        [InlineData("TagHelpersInSection")]
        [InlineData("TagHelpersInHelper")]
        public void TagHelpers_WithinHelpersAndSections_GeneratesExpectedOutput(string testType)
        {
            // Arrange
            var propertyInfoMock = new Mock<PropertyInfo>();
            propertyInfoMock.Setup(propertyInfo => propertyInfo.PropertyType).Returns(typeof(string));
            propertyInfoMock.Setup(propertyInfo => propertyInfo.Name).Returns("BoundProperty");
            var tagHelperDescriptorProvider = new TagHelperDescriptorProvider(
                new TagHelperDescriptor[]
                {
                    new TagHelperDescriptor("MyTagHelper",
                                            "MyTagHelper",
                                            ContentBehavior.None,
                                            new [] {
                                                new TagHelperAttributeDescriptor("BoundProperty", 
                                                                                 propertyInfoMock.Object)
                                            }),
                    new TagHelperDescriptor("NestedTagHelper", "NestedTagHelper", ContentBehavior.Modify)
                });

            // Act & Assert
            RunTagHelperTest(testType, tagHelperDescriptorProvider: tagHelperDescriptorProvider);
        }

        [Theory]
        [InlineData("SingleTagHelper")]
        [InlineData("BasicTagHelpers")]
        [InlineData("ComplexTagHelpers")]
        public void TagHelpers_GenerateExpectedOutput(string testType)
        {
            // Arrange
            var pFooPropertyInfo = new Mock<PropertyInfo>();
            pFooPropertyInfo.Setup(propertyInfo => propertyInfo.PropertyType).Returns(typeof(int));
            pFooPropertyInfo.Setup(propertyInfo => propertyInfo.Name).Returns("Foo");
            var inputTypePropertyInfo = new Mock<PropertyInfo>();
            inputTypePropertyInfo.Setup(propertyInfo => propertyInfo.PropertyType).Returns(typeof(string));
            inputTypePropertyInfo.Setup(propertyInfo => propertyInfo.Name).Returns("Type");
            var checkedPropertyInfo = new Mock<PropertyInfo>();
            checkedPropertyInfo.Setup(propertyInfo => propertyInfo.PropertyType).Returns(typeof(bool));
            checkedPropertyInfo.Setup(propertyInfo => propertyInfo.Name).Returns("Checked");
            var tagHelperDescriptorProvider = new TagHelperDescriptorProvider(
                new TagHelperDescriptor[]
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
                                            }),
                });

            // Act & Assert
            RunTagHelperTest(testType, tagHelperDescriptorProvider: tagHelperDescriptorProvider);
        }

        [Fact]
        public void TagHelpers_WithContentBehaviors_GenerateExpectedOutput()
        {
            // Arrange
            var tagHelperDescriptorProvider = new TagHelperDescriptorProvider(
                new TagHelperDescriptor[]
                {
                    new TagHelperDescriptor("modify", "ModifyTagHelper", ContentBehavior.Modify),
                    new TagHelperDescriptor("none", "NoneTagHelper", ContentBehavior.None),
                    new TagHelperDescriptor("append", "AppendTagHelper", ContentBehavior.Append),
                    new TagHelperDescriptor("prepend", "PrependTagHelper", ContentBehavior.Prepend),
                    new TagHelperDescriptor("replace", "ReplaceTagHelper", ContentBehavior.Replace),
                });

            // Act & Assert
            RunTagHelperTest("ContentBehaviorTagHelpers", tagHelperDescriptorProvider: tagHelperDescriptorProvider);
        }
    }
}