﻿// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Reflection;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;
using Microsoft.AspNet.Razor.TagHelpers;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class TagHelperAttributeCodeGeneratorTests : TagHelperTestBase
    {
        [Fact]
        public void TagHelpers_CanReplaceAttributeCodeGeneratorLogic()
        {
            // Arrange
            var inputTypePropertyInfo = new Mock<PropertyInfo>();
            inputTypePropertyInfo.Setup(ipi => ipi.PropertyType).Returns(typeof(string));
            inputTypePropertyInfo.Setup(ipi => ipi.Name).Returns("Type");
            var checkedPropertyInfo = new Mock<PropertyInfo>();
            checkedPropertyInfo.Setup(ipi => ipi.PropertyType).Returns(typeof(bool));
            checkedPropertyInfo.Setup(ipi => ipi.Name).Returns("Checked");
            var tagHelperProvider = new TagHelperDescriptorProvider(
                new TagHelperDescriptor[]
                {
                    new TagHelperDescriptor("p", "PTagHelper", ContentBehavior.None),
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
            RunTagHelperTest(testName: "BasicTagHelpers",
                             baseLineName: "BasicTagHelpers.CustomAttributeCodeGenerator",
                             tagHelperProvider: tagHelperProvider,
                             hostConfig: (host) =>
                             {
                                 return new CodeBuilderReplacingHost();
                             });
        }

        private class CodeBuilderReplacingHost : RazorEngineHost
        {
            public CodeBuilderReplacingHost()
                : base(new CSharpRazorCodeLanguage())
            {
            }

            public override CodeBuilder DecorateCodeBuilder(CodeBuilder incomingBuilder, CodeBuilderContext context)
            {
                return new AttributeCodeGeneratorReplacingCodeBuilder(context);
            }
        }

        private class AttributeCodeGeneratorReplacingCodeBuilder : CSharpCodeBuilder
        {
            public AttributeCodeGeneratorReplacingCodeBuilder(CodeBuilderContext context)
                : base(context)
            {
            }

            protected override CSharpCodeVisitor CreateCSharpCodeVisitor([NotNull] CSharpCodeWriter writer,
                                                                         [NotNull] CodeBuilderContext context)
            {
                var bodyVisitor = base.CreateCSharpCodeVisitor(writer, context);

                bodyVisitor.TagHelperRenderer.AttributeValueCodeRenderer = new CustomTagHelperAttributeCodeGenerator();

                return bodyVisitor;
            }
        }

        private class CustomTagHelperAttributeCodeGenerator : TagHelperAttributeValueCodeRenderer
        {
            public override void RenderAttributeValue([NotNull]TagHelperAttributeDescriptor attributeInfo,
                                                      [NotNull]CSharpCodeWriter writer,
                                                      [NotNull]CodeGeneratorContext context,
                                                      [NotNull]Action<CSharpCodeWriter> renderAttributeValue)
            {
                writer.Write("**From custom attribute code renderer**: ");

                base.RenderAttributeValue(attributeInfo, writer, context, renderAttributeValue);
            }
        }
    }
}