// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public class TagHelperOutputTest
    {
        [Fact]
        public void TagName_CanSetToNullInCtor()
        {
            // Arrange & Act
            var tagHelperOutput = new TagHelperOutput(null);

            // Assert
            Assert.Null(tagHelperOutput.TagName);
        }

        [Fact]
        public void TagName_CanSetToNull()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            // Act
            tagHelperOutput.TagName = null;

            // Assert
            Assert.Null(tagHelperOutput.TagName);
        }

        [Fact]
        public void Content_CanSetToNull()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            // Act
            tagHelperOutput.Content = null;

            // Assert
            Assert.Null(tagHelperOutput.Content);
        }

        [Fact]
        public void PreContent_CanSetToNull()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            // Act
            tagHelperOutput.PreContent = null;

            // Assert
            Assert.Null(tagHelperOutput.PreContent);
        }

        [Fact]
        public void PostContent_CanSetToNull()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            // Act
            tagHelperOutput.PostContent = null;

            // Assert
            Assert.Null(tagHelperOutput.PostContent);
        }

        [Fact]
        public void GenerateStartTag_ReturnsFullStartTag()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p", attributes:
                new Dictionary<string, string>
                {
                    { "class", "btn" },
                    { "something", "   spaced    " }
                });

            // Act
            var output = tagHelperOutput.GenerateStartTag();

            // Assert
            Assert.Equal("<p class=\"btn\" something=\"   spaced    \">", output);
        }

        [Fact]
        public void GenerateStartTag_ReturnsNoAttributeStartTag()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            // Act
            var output = tagHelperOutput.GenerateStartTag();

            // Assert
            Assert.Equal("<p>", output);
        }

        [Fact]
        public void GenerateStartTag_ReturnsSelfClosingStartTag_Attributes()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p",
                attributes: new Dictionary<string, string>
                {
                    { "class", "btn" },
                    { "something", "   spaced    " }
                });

            tagHelperOutput.SelfClosing = true;

            // Act
            var output = tagHelperOutput.GenerateStartTag();

            // Assert
            Assert.Equal("<p class=\"btn\" something=\"   spaced    \" />", output);
        }

        [Fact]
        public void GenerateStartTag_ReturnsSelfClosingStartTag_NoAttributes()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            tagHelperOutput.SelfClosing = true;

            // Act
            var output = tagHelperOutput.GenerateStartTag();

            // Assert
            Assert.Equal("<p />", output);
        }

        [Fact]
        public void GenerateStartTag_ReturnsNothingIfWhitespaceTagName()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("  ",
                attributes: new Dictionary<string, string>
                {
                    { "class", "btn" },
                    { "something", "   spaced    " }
                });

            tagHelperOutput.SelfClosing = true;

            // Act
            var output = tagHelperOutput.GenerateStartTag();

            // Assert
            Assert.Empty(output);
        }


        [Fact]
        public void GenerateEndTag_ReturnsNothingIfWhitespaceTagName()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(" "); ;

            tagHelperOutput.Content = "Hello World";

            // Act
            var output = tagHelperOutput.GenerateEndTag();

            // Assert
            Assert.Empty(output);
        }

        [Fact]
        public void GeneratePreContent_ReturnsPreContent()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            tagHelperOutput.PreContent = "Hello World";

            // Act
            var output = tagHelperOutput.GeneratePreContent();

            // Assert
            Assert.Equal("Hello World", output);
        }

        [Fact]
        public void GeneratePreContent_ReturnsNothingIfSelfClosing()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p")
            {
                SelfClosing = true
            };

            tagHelperOutput.PreContent = "Hello World";

            // Act
            var output = tagHelperOutput.GeneratePreContent();

            // Assert
            Assert.Empty(output);
        }

        [Fact]
        public void GenerateContent_ReturnsContent()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            tagHelperOutput.Content = "Hello World";

            // Act
            var output = tagHelperOutput.GenerateContent();

            // Assert
            Assert.Equal("Hello World", output);
        }


        [Fact]
        public void GenerateContent_ReturnsNothingIfSelfClosing()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p")
            {
                SelfClosing = true
            };

            tagHelperOutput.Content = "Hello World";

            // Act
            var output = tagHelperOutput.GenerateContent();

            // Assert
            Assert.Empty(output);
        }

        [Fact]
        public void GeneratePostContent_ReturnsPreContent()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            tagHelperOutput.PostContent = "Hello World";

            // Act
            var output = tagHelperOutput.GeneratePostContent();

            // Assert
            Assert.Equal("Hello World", output);
        }

        [Fact]
        public void GeneratePostContent_ReturnsNothingIfSelfClosing()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p")
            {
                SelfClosing = true
            };

            tagHelperOutput.PostContent = "Hello World";

            // Act
            var output = tagHelperOutput.GeneratePostContent();

            // Assert
            Assert.Empty(output);
        }

        [Fact]
        public void GenerateEndTag_ReturnsEndTag()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");

            // Act
            var output = tagHelperOutput.GenerateEndTag();

            // Assert
            Assert.Equal("</p>", output);
        }

        [Fact]
        public void GenerateEndTag_ReturnsNothingIfSelfClosing()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p")
            {
                SelfClosing = true
            };

            // Act
            var output = tagHelperOutput.GenerateEndTag();

            // Assert
            Assert.Empty(output);
        }

        [Fact]
        public void SupressOutput_Sets_TagName_Content_PreContent_PostContent_ToNull()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p")
            {
                PreContent = "Pre Content",
                Content = "Content",
                PostContent = "Post Content"
            };

            // Act
            tagHelperOutput.SupressOutput();

            // Assert
            Assert.Null(tagHelperOutput.TagName);
            Assert.Null(tagHelperOutput.PreContent);
            Assert.Null(tagHelperOutput.Content);
            Assert.Null(tagHelperOutput.PostContent);
        }

        [Fact]
        public void SupressOutput_PreventsTagOutput()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p",
                attributes: new Dictionary<string, string>
                {
                    { "class", "btn" },
                    { "something", "   spaced    " }
                })
            {
                PreContent = "Pre Content",
                Content = "Content",
                PostContent = "Post Content"
            };

            // Act
            tagHelperOutput.SupressOutput();

            var output = tagHelperOutput.GenerateStartTag() +
                         tagHelperOutput.GeneratePreContent() +
                         tagHelperOutput.GenerateContent() +
                         tagHelperOutput.GeneratePostContent() +
                         tagHelperOutput.GenerateEndTag();

            // Assert
            Assert.Empty(output);
        }

        [Theory]
        [InlineData("class", "ClASs")]
        [InlineData("CLaSs", "class")]
        [InlineData("cLaSs", "cLasS")]
        public void Attributes_IgnoresCase(string originalName, string updateName)
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p",
                attributes: new Dictionary<string, string>
                {
                    { originalName, "btn" },
                });

            // Act
            tagHelperOutput.Attributes[updateName] = "super button";

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal(new KeyValuePair<string, string>(originalName, "super button"), attribute);
        }
    }
}