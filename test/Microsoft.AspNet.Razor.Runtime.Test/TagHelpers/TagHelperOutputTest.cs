// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Runtime.Test.TagHelpers
{
    public class TagHelperOutputTest
    {
        [Fact]
        public void TagHelperOutput_CannotSetTagNameToNull()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            // Act
            tagHelperOutput.TagName = null;

            // Assert
            Assert.Equal(string.Empty, tagHelperOutput.TagName);
        }

        [Fact]
        public void TagHelperOutput_CannotSetContentToNull()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            // Act
            tagHelperOutput.Content = null;

            // Assert
            Assert.Equal(string.Empty, tagHelperOutput.Content);
        }

        [Fact]
        public void TagHelperOutput_GeneratesFullStartTag()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p", attributes:
                new Dictionary<string, string>
                {
                    { "class", "btn" },
                    { "something", "   spaced    " }
                });

            // Act
            var output = tagHelperOutput.GenerateTagStart();

            // Assert
            Assert.Equal("<p class=\"btn\" something=\"   spaced    \">", output);
        }

        [Fact]
        public void TagHelperOutput_GeneratesNoAttributeStartTag()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            // Act
            var output = tagHelperOutput.GenerateTagStart();

            // Assert
            Assert.Equal("<p>", output);
        }

        [Fact]
        public void TagHelperOutput_GeneratesSelfclosingStartTag()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p", attributes:
                new Dictionary<string, string>
                {
                    { "class", "btn" },
                    { "something", "   spaced    " }
                });

            tagHelperOutput.SelfClosing = true;

            // Act
            var output = tagHelperOutput.GenerateTagStart();

            // Assert
            Assert.Equal("<p class=\"btn\" something=\"   spaced    \" />", output);
        }

        [Fact]
        public void TagHelperOutput_GeneratesStartTagOutputsNothingWithEmptyTagName()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("  ", attributes:
                new Dictionary<string, string>
                {
                    { "class", "btn" },
                    { "something", "   spaced    " }
                });

            tagHelperOutput.SelfClosing = true;

            // Act
            var output = tagHelperOutput.GenerateTagStart();

            // Assert
            Assert.Equal(string.Empty, output);
        }

        [Fact]
        public void TagHelperOutput_GenerateTagContentReturnsContent()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            tagHelperOutput.Content = "Hello World";

            // Act
            var output = tagHelperOutput.GenerateTagContent();

            // Assert
            Assert.Equal("Hello World", output);
        }

        [Fact]
        public void TagHelperOutput_GenerateTagContentGeneratesNothingIfSelfClosing()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p")
            {
                SelfClosing = true
            };

            tagHelperOutput.Content = "Hello World";

            // Act
            var output = tagHelperOutput.GenerateTagContent();

            // Assert
            Assert.Equal(string.Empty, output);
        }

        [Fact]
        public void TagHelperOutput_GenerateTagEndGeneratesEndTag()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            // Act
            var output = tagHelperOutput.GenerateTagEnd();

            // Assert
            Assert.Equal("</p>", output);
        }

        [Fact]
        public void TagHelperOutput_GenerateTagEndGeneratesNothingIfSelfClosing()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p")
            {
                SelfClosing = true
            };

            // Act
            var output = tagHelperOutput.GenerateTagEnd();

            // Assert
            Assert.Equal(string.Empty, output);
        }

        [Fact]
        public void TagHelperOutput_GetContentWriterWritesToContent()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");
            var writer = tagHelperOutput.GetContentWriter();

            // Act
            writer.Write("Hello World: ");
            writer.Write(true);
            var output = tagHelperOutput.GenerateTagContent();

            // Assert
            Assert.Equal("Hello World: True", output);
        }
    }
}