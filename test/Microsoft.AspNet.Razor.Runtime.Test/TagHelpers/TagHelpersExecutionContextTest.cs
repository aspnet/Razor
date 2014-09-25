// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Runtime.Test.TagHelpers
{
    public class TagHelpersExecutionContextTest
    {
        [Fact]
        public void TagHelperExecutionContext_MaintainsHTMLAttributes()
        {
            // Arrange
            var executionContext = new TagHelpersExecutionContext("p");
            var expectedAttributes = new Dictionary<string, string>
            {
                { "class", "btn" },
                { "foo", "bar" }
            };

            // Act
            executionContext.AddHtmlAttribute("class", "btn");
            executionContext.AddHtmlAttribute("foo", "bar");

            // Assert
            Assert.Equal(expectedAttributes, executionContext.HTMLAttributes);
        }

        [Fact]
        public void TagHelperExecutionContext_MaintainsAllAttributes()
        {
            // Arrange
            var executionContext = new TagHelpersExecutionContext("p");
            var expectedAttributes = new Dictionary<string, object>
            {
                { "class", "btn" },
                { "something", true },
                { "foo", "bar" }
            };

            // Act
            executionContext.AddHtmlAttribute("class", "btn");
            executionContext.AddTagHelperAttribute("something", true);
            executionContext.AddHtmlAttribute("foo", "bar");

            // Assert
            Assert.Equal(expectedAttributes, executionContext.AllAttributes);
        }

        [Fact]
        public void TagHelperExecutionContext_MaintainsTagHelpers()
        {
            // Arrange
            var executionContext = new TagHelpersExecutionContext("p");
            var tagHelper = new PTagHelper();

            // Act
            executionContext.Add(tagHelper);

            // Assert
            var singleTagHelper = Assert.Single(executionContext.TagHelpers);
            Assert.Same(tagHelper, singleTagHelper);
        }

        private class PTagHelper : TagHelper
        {
        }
    }
}