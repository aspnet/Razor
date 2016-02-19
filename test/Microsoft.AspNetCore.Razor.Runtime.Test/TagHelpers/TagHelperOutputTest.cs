// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Razor.Runtime.TagHelpers;
using Microsoft.Extensions.WebEncoders.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Razor.TagHelpers
{
    public class TagHelperOutputTest
    {
        [Fact]
        public async Task GetChildContentAsync_CallsGetChildContentAsync()
        {
            // Arrange
            bool? passedUseCacheResult = null;
            HtmlEncoder passedEncoder = null;
            var content = new DefaultTagHelperContent();
            var output = new TagHelperOutput(
                tagName: "tag",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoder) =>
                {
                    passedUseCacheResult = useCachedResult;
                    passedEncoder = encoder;
                    return Task.FromResult<TagHelperContent>(content);
                });

            // Act
            var result = await output.GetChildContentAsync();

            // Assert
            Assert.True(passedUseCacheResult.HasValue);
            Assert.True(passedUseCacheResult.Value);
            Assert.Null(passedEncoder);
            Assert.Same(content, result);
        }

        public static TheoryData<HtmlEncoder> HtmlEncoderData
        {
            get
            {
                return new TheoryData<HtmlEncoder>
                {
                    null,
                    HtmlEncoder.Default,
                    NullHtmlEncoder.Default,
                    new HtmlTestEncoder(),
                };
            }
        }

        [Theory]
        [MemberData(nameof(HtmlEncoderData))]
        public async Task GetChildContentAsync_CallsGetChildContentAsync(HtmlEncoder encoder)
        {
            // Arrange
            bool? passedUseCacheResult = null;
            HtmlEncoder passedEncoder = null;
            var content = new DefaultTagHelperContent();
            var output = new TagHelperOutput(
                tagName: "tag",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResult, encoderArgument) =>
                {
                    passedUseCacheResult = useCachedResult;
                    passedEncoder = encoderArgument;
                    return Task.FromResult<TagHelperContent>(content);
                });

            // Act
            var result = await output.GetChildContentAsync(encoder);

            // Assert
            Assert.True(passedUseCacheResult.HasValue);
            Assert.True(passedUseCacheResult.Value);
            Assert.Same(encoder, passedEncoder);
            Assert.Same(content, result);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task GetChildContentAsync_CallsGetChildContentAsync(bool useCachedResult)
        {
            // Arrange
            bool? passedUseCacheResult = null;
            HtmlEncoder passedEncoder = null;
            var content = new DefaultTagHelperContent();
            var output = new TagHelperOutput(
                tagName: "tag",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResultArgument, encoder) =>
                {
                    passedUseCacheResult = useCachedResultArgument;
                    passedEncoder = encoder;
                    return Task.FromResult<TagHelperContent>(content);
                });

            // Act
            var result = await output.GetChildContentAsync(useCachedResult);

            // Assert
            Assert.True(passedUseCacheResult.HasValue);
            Assert.Equal(useCachedResult, passedUseCacheResult.Value);
            Assert.Null(passedEncoder);
            Assert.Same(content, result);
        }

        public static TheoryData<bool, HtmlEncoder> UseCachedResultAndHtmlEncoderData
        {
            get
            {
                var data = new TheoryData<bool, HtmlEncoder>();
                foreach (var useCachedResult in new[] { false, true })
                {
                    foreach (var encoderEntry in HtmlEncoderData)
                    {
                        var encoder = (HtmlEncoder)(encoderEntry[0]);
                        data.Add(useCachedResult, encoder);
                    }
                }

                return data;
            }
        }

        [Theory]
        [MemberData(nameof(UseCachedResultAndHtmlEncoderData))]
        public async Task GetChildContentAsync_CallsGetChildContentAsync(bool useCachedResult, HtmlEncoder encoder)
        {
            // Arrange
            bool? passedUseCacheResult = null;
            HtmlEncoder passedEncoder = null;
            var content = new DefaultTagHelperContent();
            var output = new TagHelperOutput(
                tagName: "tag",
                attributes: new TagHelperAttributeList(),
                getChildContentAsync: (useCachedResultArgument, encoderArgument) =>
                {
                    passedUseCacheResult = useCachedResultArgument;
                    passedEncoder = encoderArgument;
                    return Task.FromResult<TagHelperContent>(content);
                });

            // Act
            var result = await output.GetChildContentAsync(useCachedResult, encoder);

            // Assert
            Assert.True(passedUseCacheResult.HasValue);
            Assert.Equal(useCachedResult, passedUseCacheResult.Value);
            Assert.Same(encoder, passedEncoder);
            Assert.Same(content, result);
        }

        [Fact]
        public void PreElement_SetContent_ChangesValue()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");
            tagHelperOutput.PreElement.SetContent("Hello World");

            // Act & Assert
            Assert.NotNull(tagHelperOutput.PreElement);
            Assert.NotNull(tagHelperOutput.PreContent);
            Assert.NotNull(tagHelperOutput.Content);
            Assert.NotNull(tagHelperOutput.PostContent);
            Assert.NotNull(tagHelperOutput.PostElement);
            Assert.Equal(
                "HtmlEncode[[Hello World]]",
                tagHelperOutput.PreElement.GetContent(new HtmlTestEncoder()));
        }

        [Fact]
        public void PostElement_SetContent_ChangesValue()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");
            tagHelperOutput.PostElement.SetContent("Hello World");

            // Act & Assert
            Assert.NotNull(tagHelperOutput.PreElement);
            Assert.NotNull(tagHelperOutput.PreContent);
            Assert.NotNull(tagHelperOutput.Content);
            Assert.NotNull(tagHelperOutput.PostContent);
            Assert.NotNull(tagHelperOutput.PostElement);
            Assert.Equal(
                "HtmlEncode[[Hello World]]",
                tagHelperOutput.PostElement.GetContent(new HtmlTestEncoder()));
        }

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
        public void PreContent_SetContent_ChangesValue()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");
            tagHelperOutput.PreContent.SetContent("Hello World");

            // Act & Assert
            Assert.NotNull(tagHelperOutput.PreElement);
            Assert.NotNull(tagHelperOutput.PreContent);
            Assert.NotNull(tagHelperOutput.Content);
            Assert.NotNull(tagHelperOutput.PostContent);
            Assert.NotNull(tagHelperOutput.PostElement);
            Assert.Equal(
                "HtmlEncode[[Hello World]]",
                tagHelperOutput.PreContent.GetContent(new HtmlTestEncoder()));
        }

        [Fact]
        public void Content_SetContent_ChangesValue()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");
            tagHelperOutput.Content.SetContent("Hello World");

            // Act & Assert
            Assert.NotNull(tagHelperOutput.PreElement);
            Assert.NotNull(tagHelperOutput.PreContent);
            Assert.NotNull(tagHelperOutput.Content);
            Assert.NotNull(tagHelperOutput.PostContent);
            Assert.NotNull(tagHelperOutput.PostElement);
            Assert.Equal(
                "HtmlEncode[[Hello World]]",
                tagHelperOutput.Content.GetContent(new HtmlTestEncoder()));
        }

        [Fact]
        public void PostContent_SetContent_ChangesValue()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput("p");
            tagHelperOutput.PostContent.SetContent("Hello World");

            // Act & Assert
            Assert.NotNull(tagHelperOutput.PreElement);
            Assert.NotNull(tagHelperOutput.PreContent);
            Assert.NotNull(tagHelperOutput.Content);
            Assert.NotNull(tagHelperOutput.PostContent);
            Assert.NotNull(tagHelperOutput.PostElement);
            Assert.Equal(
                "HtmlEncode[[Hello World]]",
                tagHelperOutput.PostContent.GetContent(new HtmlTestEncoder()));
        }

        [Fact]
        public void SuppressOutput_Sets_AllContent_ToNullOrEmpty()
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
            Assert.NotNull(tagHelperOutput.PreElement);
            Assert.Empty(tagHelperOutput.PreElement.GetContent(new HtmlTestEncoder()));
            Assert.NotNull(tagHelperOutput.PreContent);
            Assert.Empty(tagHelperOutput.PreContent.GetContent(new HtmlTestEncoder()));
            Assert.NotNull(tagHelperOutput.Content);
            Assert.Empty(tagHelperOutput.Content.GetContent(new HtmlTestEncoder()));
            Assert.NotNull(tagHelperOutput.PostContent);
            Assert.Empty(tagHelperOutput.PostContent.GetContent(new HtmlTestEncoder()));
            Assert.NotNull(tagHelperOutput.PostElement);
            Assert.Empty(tagHelperOutput.PostElement.GetContent(new HtmlTestEncoder()));
        }

        [Fact]
        public void SuppressOutput_PreventsTagOutput()
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                new TagHelperAttributeList
                {
                    { "class", "btn" },
                    { "something", "   spaced    " }
                },
                (cachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));
            tagHelperOutput.PreContent.Append("Pre Content");
            tagHelperOutput.Content.Append("Content");
            tagHelperOutput.PostContent.Append("Post Content");

            // Act
            tagHelperOutput.SuppressOutput();

            // Assert
            Assert.NotNull(tagHelperOutput.PreElement);
            Assert.Empty(tagHelperOutput.PreElement.GetContent(new HtmlTestEncoder()));
            Assert.NotNull(tagHelperOutput.PreContent);
            Assert.Empty(tagHelperOutput.PreContent.GetContent(new HtmlTestEncoder()));
            Assert.NotNull(tagHelperOutput.Content);
            Assert.Empty(tagHelperOutput.Content.GetContent(new HtmlTestEncoder()));
            Assert.NotNull(tagHelperOutput.PostContent);
            Assert.Empty(tagHelperOutput.PostContent.GetContent(new HtmlTestEncoder()));
            Assert.NotNull(tagHelperOutput.PostElement);
            Assert.Empty(tagHelperOutput.PostElement.GetContent(new HtmlTestEncoder()));
        }

        [Theory]
        [InlineData("class", "ClASs")]
        [InlineData("CLaSs", "class")]
        [InlineData("cLaSs", "cLasS")]
        public void Attributes_IgnoresCase(string originalName, string updateName)
        {
            // Arrange
            var tagHelperOutput = new TagHelperOutput(
                "p",
                new TagHelperAttributeList
                {
                    { originalName, "btn" },
                },
                (cachedResult, encoder) => Task.FromResult<TagHelperContent>(new DefaultTagHelperContent()));

            // Act
            tagHelperOutput.Attributes.SetAttribute(updateName, "super button");

            // Assert
            var attribute = Assert.Single(tagHelperOutput.Attributes);
            Assert.Equal(
                new TagHelperAttribute(updateName, "super button"),
                attribute,
                CaseSensitiveTagHelperAttributeComparer.Default);
        }

        public static TheoryData<TagHelperOutput, string> WriteTagHelper_InputData
        {
            get
            {
                // parameters: TagHelperOutput, expectedOutput
                return new TheoryData<TagHelperOutput, string>
                {
                    {
                        // parameters: TagName, Attributes, SelfClosing, PreContent, Content, PostContent
                        GetTagHelperOutput(
                            tagName:     "div",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<div>Hello World!</div>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "Hello World!"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "  ",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "Hello World!"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList() { { "test", "testVal" } },
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p test=\"HtmlEncode[[testVal]]\">Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList()
                            {
                                { "test", "testVal" },
                                { "something", "  spaced  " }
                            },
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p test=\"HtmlEncode[[testVal]]\" something=\"HtmlEncode[[  spaced  ]]\">Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList()
                            {
                                new TagHelperAttribute("test"),
                            },
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p test>Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList()
                            {
                                new TagHelperAttribute("test"),
                                new TagHelperAttribute("test2"),
                            },
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p test test2>Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList()
                            {
                                new TagHelperAttribute("first", "unminimized"),
                                new TagHelperAttribute("test"),
                            },
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p first=\"HtmlEncode[[unminimized]]\" test>Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList()
                            {
                                new TagHelperAttribute("test"),
                                new TagHelperAttribute("last", "unminimized"),
                            },
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p test last=\"HtmlEncode[[unminimized]]\">Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList() { { "test", "testVal" } },
                            tagMode: TagMode.SelfClosing,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p test=\"HtmlEncode[[testVal]]\" />"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList()
                            {
                                { "test", "testVal" },
                                { "something", "  spaced  " }
                            },
                            tagMode: TagMode.SelfClosing,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p test=\"HtmlEncode[[testVal]]\" something=\"HtmlEncode[[  spaced  ]]\" />"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList() { { "test", "testVal" } },
                            tagMode: TagMode.StartTagOnly,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p test=\"HtmlEncode[[testVal]]\">"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList()
                            {
                                { "test", "testVal" },
                                { "something", "  spaced  " }
                            },
                            tagMode: TagMode.StartTagOnly,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p test=\"HtmlEncode[[testVal]]\" something=\"HtmlEncode[[  spaced  ]]\">"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  "Hello World!",
                            content:     null,
                            postContent: null,
                            postElement: null),
                        "<p>Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     "Hello World!",
                            postContent: null,
                            postElement: null),
                        "<p>Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     null,
                            postContent: "Hello World!",
                            postElement: null),
                        "<p>Hello World!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: null),
                        "<p>HelloTestWorld!</p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.SelfClosing,
                            preElement:  null,
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: null),
                        "<p />"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "p",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagOnly,
                            preElement:  null,
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: null),
                        "<p>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: null),
                        "<custom>HelloTestWorld!</custom>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "random",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.SelfClosing,
                            preElement:  null,
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: null),
                        "<random />"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "random",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagOnly,
                            preElement:  null,
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: null),
                        "<random>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  "Before",
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: null),
                        "Before<custom></custom>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  "Before",
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: null),
                        "Before"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList { { "test", "testVal" } },
                            tagMode: TagMode.SelfClosing,
                            preElement:  "Before",
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: null),
                        "Before"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList { { "test", "testVal" } },
                            tagMode: TagMode.SelfClosing,
                            preElement:  "Before",
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: null),
                        "Before<custom test=\"HtmlEncode[[testVal]]\" />"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.SelfClosing,
                            preElement:  "Before",
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: null),
                        "Before<custom />"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList { { "test", "testVal" } },
                            tagMode: TagMode.StartTagOnly,
                            preElement:  "Before",
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: null),
                        "Before"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList { { "test", "testVal" } },
                            tagMode: TagMode.StartTagOnly,
                            preElement:  "Before",
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: null),
                        "Before<custom test=\"HtmlEncode[[testVal]]\">"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagOnly,
                            preElement:  "Before",
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: null),
                        "Before<custom>"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: "After"),
                        "<custom></custom>After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  null,
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: "After"),
                        "After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList { { "test", "testVal" } },
                            tagMode: TagMode.SelfClosing,
                            preElement:  null,
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: "After"),
                        "After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList { { "test", "testVal" } },
                            tagMode: TagMode.SelfClosing,
                            preElement:  null,
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: "After"),
                        "<custom test=\"HtmlEncode[[testVal]]\" />After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.SelfClosing,
                            preElement:  null,
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: "After"),
                        "<custom />After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList { { "test", "testVal" } },
                            tagMode: TagMode.StartTagOnly,
                            preElement:  null,
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: "After"),
                        "After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList { { "test", "testVal" } },
                            tagMode: TagMode.StartTagOnly,
                            preElement:  null,
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: "After"),
                        "<custom test=\"HtmlEncode[[testVal]]\">After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagOnly,
                            preElement:  null,
                            preContent:  null,
                            content:     null,
                            postContent: null,
                            postElement: "After"),
                        "<custom>After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  "Before",
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: "After"),
                        "Before<custom>HelloTestWorld!</custom>After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList { { "test", "testVal" } },
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  "Before",
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: "After"),
                        "Before<custom test=\"HtmlEncode[[testVal]]\">HelloTestWorld!</custom>After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.SelfClosing,
                            preElement:  "Before",
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: "After"),
                        "Before<custom />After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.SelfClosing,
                            preElement:  "Before",
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: "After"),
                        "BeforeHelloTestWorld!After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     "custom",
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagOnly,
                            preElement:  "Before",
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: "After"),
                        "Before<custom>After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagOnly,
                            preElement:  "Before",
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: "After"),
                        "BeforeHelloTestWorld!After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList(),
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  "Before",
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: "After"),
                        "BeforeHelloTestWorld!After"
                    },
                    {
                        GetTagHelperOutput(
                            tagName:     string.Empty,
                            attributes:  new TagHelperAttributeList { { "test", "testVal" } },
                            tagMode: TagMode.StartTagAndEndTag,
                            preElement:  "Before",
                            preContent:  "Hello",
                            content:     "Test",
                            postContent: "World!",
                            postElement: "After"),
                        "BeforeHelloTestWorld!After"
                    },
                };
            }
        }

        [Theory]
        [MemberData(nameof(WriteTagHelper_InputData))]
        public void WriteTo_WritesFormattedTagHelper(TagHelperOutput output, string expected)
        {
            // Arrange
            var writer = new StringWriter();
            var tagHelperExecutionContext = new TagHelperExecutionContext(
                tagName: output.TagName,
                tagMode: output.TagMode,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () => Task.FromResult(result: true),
                startTagHelperWritingScope: _ => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());
            tagHelperExecutionContext.Output = output;
            var testEncoder = new HtmlTestEncoder();

            // Act
            output.WriteTo(writer, testEncoder);

            // Assert
            Assert.Equal(expected, writer.ToString(), StringComparer.Ordinal);
        }

        // This tests a separate code path that's used by THO when the writer is an HtmlTextWriter.
        // The output should be the same, but we do some specific perf optimizations on this path.
        [Theory]
        [MemberData(nameof(WriteTagHelper_InputData))]
        public void WriteTo_WritesFormattedTagHelper_HtmlTextWriter(TagHelperOutput output, string expected)
        {
            // Arrange
            var inner = new StringWriter();
            var testEncoder = new HtmlTestEncoder();

            var writer = new StringWriter();
            var buffer = new HtmlContentBuilder();

            var tagHelperExecutionContext = new TagHelperExecutionContext(
                tagName: output.TagName,
                tagMode: output.TagMode,
                items: new Dictionary<object, object>(),
                uniqueId: string.Empty,
                executeChildContentAsync: () => Task.FromResult(result: true),
                startTagHelperWritingScope: _ => { },
                endTagHelperWritingScope: () => new DefaultTagHelperContent());
            tagHelperExecutionContext.Output = output;

            // Act
            output.WriteTo(writer, testEncoder);

            // Assert
            Assert.Equal(expected, inner.ToString(), StringComparer.Ordinal);
        }

        private static TagHelperOutput GetTagHelperOutput(
            string tagName,
            TagHelperAttributeList attributes,
            TagMode tagMode,
            string preElement,
            string preContent,
            string content,
            string postContent,
            string postElement)
        {
            var output = new TagHelperOutput(
                tagName,
                attributes,
                getChildContentAsync: (useCachedContent, encoder) => Task.FromResult<TagHelperContent>(
                    new DefaultTagHelperContent()))
            {
                TagMode = tagMode
            };

            if (preElement != null)
            {
                output.PreElement.AppendHtml(preElement);
            }

            if (preContent != null)
            {
                output.PreContent.AppendHtml(preContent);
            }

            if (content != null)
            {
                output.Content.AppendHtml(content);
            }

            if (postContent != null)
            {
                output.PostContent.AppendHtml(postContent);
            }

            if (postElement != null)
            {
                output.PostElement.AppendHtml(postElement);
            }

            return output;
        }
    }
}