// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

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
            pTagHelper = manager.StartTagHelper<PTagHelper>();
            manager.StartActiveTagHelpers("p");
            pTagHelperNested = manager.StartTagHelper<PTagHelper>();
            var currentContext = manager.ExposedCurrentContext;

            // Assert
            var tagHelper = Assert.Single(currentContext.ActiveTagHelpers);
            Assert.Same(pTagHelperNested, tagHelper);
            manager.EndTagHelpers();
            Assert.NotSame(currentContext, manager.ExposedCurrentContext);
            currentContext = manager.ExposedCurrentContext;
            tagHelper = Assert.Single(currentContext.ActiveTagHelpers);
            Assert.Same(pTagHelper, tagHelper);
            manager.EndTagHelpers();
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
            pTagHelper = manager.StartTagHelper<PTagHelper>();
            pTagHelper2 = manager.StartTagHelper<PTagHelper>();
            manager.StartActiveTagHelpers("p");
            var activeTagHelpers = manager.ExposedCurrentContext.ActiveTagHelpers;

            // Assert
            Assert.Equal(2, activeTagHelpers.Count);
            Assert.Same(pTagHelper, activeTagHelpers[0]);
            Assert.Same(pTagHelper2, activeTagHelpers[1]);
            manager.EndTagHelpers();
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
            pTagHelper = manager.StartTagHelper<PTagHelper>();
            manager.AddHTMLAttribute("class", "btn");
            manager.AddHTMLAttribute("foo", "bar");
            manager.StartActiveTagHelpers("p");
            pTagHelperNested = manager.StartTagHelper<PTagHelper>();
            manager.AddHTMLAttribute("nested", "true");
            var currentContext = manager.ExposedCurrentContext;

            // Assert
            var attribute = Assert.Single(currentContext.HTMLAttributes);
            Assert.Equal("nested", attribute.Key);
            Assert.Equal("true", attribute.Value);
            manager.EndTagHelpers();
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
            pTagHelper = manager.StartTagHelper<PTagHelper>();
            manager.AddHTMLAttribute("class", "btn");
            manager.AddTagHelperAttribute("someBoolAttribute", true);
            manager.StartActiveTagHelpers("p");
            pTagHelperNested = manager.StartTagHelper<PTagHelper>();
            manager.AddHTMLAttribute("nested", "true");
            manager.AddTagHelperAttribute("nestedInt", 100);
            var currentContext = manager.ExposedCurrentContext;

            // Assert
            Assert.Single(currentContext.HTMLAttributes);
            Assert.Equal(2, currentContext.AllAttributes.Count);
            var strAttributeValue = (string)currentContext.AllAttributes["nested"];
            Assert.Equal("true", strAttributeValue);
            var intAttributeValue = (int)currentContext.AllAttributes["nestedInt"];
            Assert.Equal(100, intAttributeValue);

            manager.EndTagHelpers();
            currentContext = manager.ExposedCurrentContext;
            Assert.Single(currentContext.HTMLAttributes);
            Assert.Equal(2, currentContext.AllAttributes.Count);
            strAttributeValue = (string)currentContext.AllAttributes["class"];
            Assert.Equal("btn", strAttributeValue);
            var boolAttributeValue = (bool)currentContext.AllAttributes["someBoolAttribute"];
            Assert.Equal(true, boolAttributeValue);
        }

        private class PTagHelper : ITestTagHelper
        {
        }
    }
}