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
            // Arrange & Act
            var tagHelperOutput = new TagHelperOutput("p")
            {
                TagName = null
            };

            // Assert
            Assert.Null(tagHelperOutput.TagName);
        }

        [Fact]
        public void CanSetPreContent()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");
            tagHelperOutput.PreContent.SetContent("Hello World");

            // Act & Assert
            var result = Assert.IsType<DefaultTagHelperContent>(tagHelperOutput.PreContent);
            Assert.Equal("Hello World", result.GetContent());
        }

        [Fact]
        public void CanSetContent()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");
            tagHelperOutput.Content.SetContent("Hello World");

            // Act & Assert
            var result = Assert.IsType<DefaultTagHelperContent>(tagHelperOutput.Content);
            Assert.Equal("Hello World", result.GetContent());
        }

        [Fact]
        public void CanSetPostContent()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");
            tagHelperOutput.PostContent.SetContent("Hello World");

            // Act & Assert
            var result = Assert.IsType<DefaultTagHelperContent>(tagHelperOutput.PostContent);
            Assert.Equal("Hello World", result.GetContent());
        }

        [Fact]
        public void SuppressOutput_Sets_TagName_Content_PreContent_PostContent_ToNull()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");
            tagHelperOutput.PreContent.Append("Pre Content");
            tagHelperOutput.Content.Append("Content");
            tagHelperOutput.PostContent.Append("Post Content");

            // Act
            tagHelperOutput.SuppressOutput();

            // Assert
            Assert.Null(tagHelperOutput.TagName);
            var result = Assert.IsType<DefaultTagHelperContent>(tagHelperOutput.PreContent);
            Assert.Empty(result.GetContent());
            result = Assert.IsType<DefaultTagHelperContent>(tagHelperOutput.Content);
            Assert.Empty(result.GetContent());
            result = Assert.IsType<DefaultTagHelperContent>(tagHelperOutput.PostContent);
            Assert.Empty(result.GetContent());
        }

        [Fact]
        public void SuppressOutput_PreventsTagOutput()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p",
                attributes: new Dictionary<string, string>
                {
                    { "class", "btn" },
                    { "something", "   spaced    " }
                });
            tagHelperOutput.PreContent.Append("Pre Content");
            tagHelperOutput.Content.Append("Content");
            tagHelperOutput.PostContent.Append("Post Content");

            // Act
            tagHelperOutput.SuppressOutput();

            // Assert
            var result = Assert.IsType<DefaultTagHelperContent>(tagHelperOutput.PreContent);
            Assert.Empty(result.GetContent());
            result = Assert.IsType<DefaultTagHelperContent>(tagHelperOutput.Content);
            Assert.Empty(result.GetContent());
            result = Assert.IsType<DefaultTagHelperContent>(tagHelperOutput.PostContent);
            Assert.Empty(result.GetContent());
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