// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Microsoft.AspNet.Razor.Runtime.Test.Framework;
using Xunit;

namespace Microsoft.AspNet.Razor.Runtime.Test.TagHelpers
{
    public class TagHelperManagerTest
    {
        [Fact]
        public void TagHelperManager_ManagesNestedTagHelpers()
        {
            // Arrange
            var manager = new TestTagHelperManager();
            PTagHelper pTagHelper;
            PTagHelper pTagHelperNested;

            // Act
            pTagHelper = manager.InstantiateTagHelper<PTagHelper>();
            manager.StartTagHelpersScope("p");
            pTagHelperNested = manager.InstantiateTagHelper<PTagHelper>();
            var currentContext = manager.ExposedCurrentContext;

            // Assert
            var tagHelper = Assert.Single(currentContext.ActiveTagHelpers);
            Assert.Same(pTagHelperNested, tagHelper);
            manager.EndTagHelpersScope();
            Assert.NotSame(currentContext, manager.ExposedCurrentContext);
            currentContext = manager.ExposedCurrentContext;
            tagHelper = Assert.Single(currentContext.ActiveTagHelpers);
            Assert.Same(pTagHelper, tagHelper);
            manager.EndTagHelpersScope();
            Assert.Null(manager.ExposedCurrentContext);
        }

        [Fact]
        public void TagHelperManager_ManagesMultipleTagHelpersSimultaneously()
        {
            // Arrange
            var manager = new TestTagHelperManager();
            PTagHelper pTagHelper;
            PTagHelper pTagHelper2;

            // Act
            pTagHelper = manager.InstantiateTagHelper<PTagHelper>();
            pTagHelper2 = manager.InstantiateTagHelper<PTagHelper>();
            manager.StartTagHelpersScope("p");
            var activeTagHelpers = manager.ExposedCurrentContext.ActiveTagHelpers;

            // Assert
            Assert.Equal(2, activeTagHelpers.Count);
            Assert.Same(pTagHelper, activeTagHelpers[0]);
            Assert.Same(pTagHelper2, activeTagHelpers[1]);
            manager.EndTagHelpersScope();
            Assert.Null(manager.ExposedCurrentContext);
        }

        [Fact]
        public void TagHelperManager_MaintainsHTMLAttributes()
        {
            // Arrange
            var manager = new TestTagHelperManager();
            PTagHelper pTagHelper;
            PTagHelper pTagHelperNested;

            // Act
            pTagHelper = manager.InstantiateTagHelper<PTagHelper>();
            manager.AddHtmlAttribute("class", "btn");
            manager.AddHtmlAttribute("foo", "bar");
            manager.StartTagHelpersScope("p");
            pTagHelperNested = manager.InstantiateTagHelper<PTagHelper>();
            manager.AddHtmlAttribute("nested", "true");
            var currentContext = manager.ExposedCurrentContext;

            // Assert
            var attribute = Assert.Single(currentContext.HTMLAttributes);
            Assert.Equal("nested", attribute.Key);
            Assert.Equal("true", attribute.Value);
            manager.EndTagHelpersScope();
            currentContext = manager.ExposedCurrentContext;

            Assert.Equal(2, currentContext.HTMLAttributes.Count);
            var attributeValue = currentContext.HTMLAttributes["class"];
            Assert.Equal("btn", attributeValue);
            attributeValue = currentContext.HTMLAttributes["foo"];
            Assert.Equal("bar", attributeValue);
        }

        [Fact]
        public void TagHelperManager_MaintainsAllAttributes()
        {
            // Arrange
            var manager = new TestTagHelperManager();
            PTagHelper pTagHelper;
            PTagHelper pTagHelperNested;

            // Act
            pTagHelper = manager.InstantiateTagHelper<PTagHelper>();
            manager.AddHtmlAttribute("class", "btn");
            manager.AddTagHelperAttribute("someBoolAttribute", true);
            manager.StartTagHelpersScope("p");
            pTagHelperNested = manager.InstantiateTagHelper<PTagHelper>();
            manager.AddHtmlAttribute("nested", "true");
            manager.AddTagHelperAttribute("nestedInt", 100);
            var currentContext = manager.ExposedCurrentContext;

            // Assert
            Assert.Single(currentContext.HTMLAttributes);
            Assert.Equal(2, currentContext.AllAttributes.Count);
            var strAttributeValue = (string)currentContext.AllAttributes["nested"];
            Assert.Equal("true", strAttributeValue);
            var intAttributeValue = (int)currentContext.AllAttributes["nestedInt"];
            Assert.Equal(100, intAttributeValue);

            manager.EndTagHelpersScope();
            currentContext = manager.ExposedCurrentContext;
            Assert.Single(currentContext.HTMLAttributes);
            Assert.Equal(2, currentContext.AllAttributes.Count);
            strAttributeValue = (string)currentContext.AllAttributes["class"];
            Assert.Equal("btn", strAttributeValue);
            var boolAttributeValue = (bool)currentContext.AllAttributes["someBoolAttribute"];
            Assert.Equal(true, boolAttributeValue);
        }

        [Fact]
        public void TagHelperManager_StartActiveTagHelpersInstantiatesATagHelperOutput()
        {
            // Arrange
            var manager = new TestTagHelperManager();
            PTagHelper pTagHelper;

            // Act
            pTagHelper = manager.InstantiateTagHelper<PTagHelper>();
            var preStartTagHelperOutput = manager.ExposedCurrentContext.TagHelperOutput;
            manager.StartTagHelpersScope("p");
            var currentTagHelperOutput = manager.ExposedCurrentContext.TagHelperOutput;

            // Assert
            Assert.Null(preStartTagHelperOutput);
            Assert.NotNull(currentTagHelperOutput);
        }

        [Fact]
        public void TagHelperManager_GetTagBodyBufferAffectsTagHelperOutput()
        {
            // Arrange
            var manager = new TestTagHelperManager();
            PTagHelper pTagHelper;

            // Act
            pTagHelper = manager.InstantiateTagHelper<PTagHelper>();
            manager.StartTagHelpersScope("p");
            var buffer = manager.GetContentBuffer();
            buffer.Write("Hello World: ");
            buffer.Write(true);
            var currentTagHelperOutput = manager.ExposedCurrentContext.TagHelperOutput;

            // Assert
            Assert.Equal("Hello World: True", currentTagHelperOutput.Content);
        }

        [Fact]
        public void TagHelperManager_GenerateTagStartReturnsTagHelperOutputGenerateTagStart()
        {
            // Arrange
            var manager = new TestTagHelperManager();
            PTagHelper pTagHelper;

            // Act
            pTagHelper = manager.InstantiateTagHelper<PTagHelper>();
            manager.AddHtmlAttribute("class", "btn");
            manager.StartTagHelpersScope("p");
            var currentTagHelperOutput = manager.ExposedCurrentContext.TagHelperOutput;

            // Assert
            Assert.Equal(currentTagHelperOutput.GenerateTagStart(), manager.GenerateTagStart());
        }

        [Fact]
        public void TagHelperManager_GenerateTagContentReturnsTagHelperOutputGenerateTagContent()
        {
            // Arrange
            var manager = new TestTagHelperManager();
            PTagHelper pTagHelper;

            // Act
            pTagHelper = manager.InstantiateTagHelper<PTagHelper>();
            manager.AddHtmlAttribute("class", "btn");
            manager.StartTagHelpersScope("p");
            var currentTagHelperOutput = manager.ExposedCurrentContext.TagHelperOutput;

            // Assert
            Assert.Equal(currentTagHelperOutput.GenerateTagContent(), manager.GenerateTagContent());
        }

        [Fact]
        public void TagHelperManager_GenerateTagEndReturnsTagHelperOutputGenerateTagEnd()
        {
            // Arrange
            var manager = new TestTagHelperManager();
            PTagHelper pTagHelper;

            // Act
            pTagHelper = manager.InstantiateTagHelper<PTagHelper>();
            manager.AddHtmlAttribute("class", "btn");
            manager.StartTagHelpersScope("p");
            var currentTagHelperOutput = manager.ExposedCurrentContext.TagHelperOutput;

            // Assert
            Assert.Equal(currentTagHelperOutput.GenerateTagEnd(), manager.GenerateTagEnd());
        }

        [Fact]
        public async Task TagHelperManager_ExecuteTagHelpersAsyncAllowsModificationOfTagHelperOutput()
        {
            // Arrange
            var manager = new TestTagHelperManager();
            ExecutableTagHelper executableTagHelper;

            // Act
            executableTagHelper = manager.InstantiateTagHelper<ExecutableTagHelper>();
            manager.AddHtmlAttribute("class", "btn");
            manager.StartTagHelpersScope("executable");

            await manager.ExecuteTagHelpersAsync();

            var currentTagHelperOutput = manager.ExposedCurrentContext.TagHelperOutput;

            // Assert
            Assert.Equal("foo", currentTagHelperOutput.TagName);
            Assert.Equal("somethingelse", currentTagHelperOutput.Attributes["class"]);
            Assert.Equal("world", currentTagHelperOutput.Attributes["hello"]);
            Assert.Equal(true, currentTagHelperOutput.SelfClosing);
        }

        private class PTagHelper : TagHelper
        {
            public override void Process(TagHelperOutput output, TagHelperContext context)
            {
            }
        }

        private class ExecutableTagHelper : TagHelper
        {
            public override void Process(TagHelperOutput output, TagHelperContext context)
            {
                output.TagName = "foo";
                output.Attributes["class"] = "somethingelse";
                output.Attributes.Add("hello", "world");
                output.SelfClosing = true;
            }
        }
    }
}