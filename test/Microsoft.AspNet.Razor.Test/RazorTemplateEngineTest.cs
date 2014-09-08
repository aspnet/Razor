// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using System.Web.WebPages.TestUtils;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Text;
using Moq;
using Xunit;

namespace Microsoft.AspNet.Razor.Test
{
    public class RazorTemplateEngineTest
    {
        [Fact]
        public void ConstructorRequiresNonNullHost()
        {
            Assert.Throws<ArgumentNullException>("host", () => new RazorTemplateEngine(null));
        }

        [Fact]
        public void ConstructorInitializesHost()
        {
            // Arrange
            var host = new RazorEngineHost(new CSharpRazorCodeLanguage());

            // Act
            var engine = new RazorTemplateEngine(host);

            // Assert
            Assert.Same(host, engine.Host);
        }

        [Fact]
        public void CreateParserMethodIsConstructedFromHost()
        {
            // Arrange
            var host = CreateHost();
            var engine = new RazorTemplateEngine(host);

            // Act
            var parser = engine.CreateParser();

            // Assert
            Assert.IsType<CSharpCodeParser>(parser.CodeParser);
            Assert.IsType<HtmlMarkupParser>(parser.MarkupParser);
        }

        [Fact]
        public void CreateParserMethodSetsParserContextToDesignTimeModeIfHostSetToDesignTimeMode()
        {
            // Arrange
            var host = CreateHost();
            var engine = new RazorTemplateEngine(host);
            host.DesignTimeMode = true;

            // Act
            var parser = engine.CreateParser();

            // Assert
            Assert.True(parser.DesignTimeMode);
        }

        [Fact]
        public void CreateParserMethodPassesParsersThroughDecoratorMethodsOnHost()
        {
            // Arrange
            var expectedCode = new Mock<ParserBase>().Object;
            var expectedMarkup = new Mock<ParserBase>().Object;

            var mockHost = new Mock<RazorEngineHost>(new CSharpRazorCodeLanguage()) { CallBase = true };
            mockHost.Setup(h => h.DecorateCodeParser(It.IsAny<CSharpCodeParser>()))
                .Returns(expectedCode);
            mockHost.Setup(h => h.DecorateMarkupParser(It.IsAny<HtmlMarkupParser>()))
                .Returns(expectedMarkup);
            var engine = new RazorTemplateEngine(mockHost.Object);

            // Act
            var actual = engine.CreateParser();

            // Assert
            Assert.Equal(expectedCode, actual.CodeParser);
            Assert.Equal(expectedMarkup, actual.MarkupParser);
        }

        [Fact]
        public void CreateCodeGeneratorMethodPassesCodeGeneratorThroughDecorateMethodOnHost()
        {
            // Arrange
            var mockHost = new Mock<RazorEngineHost>(new CSharpRazorCodeLanguage()) { CallBase = true };

            var expected = new Mock<RazorCodeGenerator>("Foo", "Bar", "Baz", mockHost.Object).Object;

            mockHost.Setup(h => h.DecorateCodeGenerator(It.IsAny<CSharpRazorCodeGenerator>()))
                .Returns(expected);
            var engine = new RazorTemplateEngine(mockHost.Object);

            // Act
            var actual = engine.CreateCodeGenerator("Foo", "Bar", "Baz");

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void CreateCodeBuilder_PassesCodeGeneratorThroughDecorateMethodOnHost()
        {
            // Arrange
            var mockHost = new Mock<RazorEngineHost>(new CSharpRazorCodeLanguage()) { CallBase = true };
            var context = CodeGeneratorContext.Create(mockHost.Object,
                                                      "different-class",
                                                      "different-ns",
                                                      string.Empty,
                                                      shouldGenerateLinePragmas: true);
            var expected = new CSharpCodeBuilder(context);

            mockHost.Setup(h => h.DecorateCodeBuilder(It.IsAny<CSharpCodeBuilder>(), context))
                    .Returns(expected);
            var engine = new RazorTemplateEngine(mockHost.Object);

            // Act
            var actual = engine.CreateCodeBuilder(context);

            // Assert
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ParseTemplateCopiesTextReaderContentToSeekableTextReaderAndPassesToParseTemplateCore()
        {
            // Arrange
            Mock<RazorTemplateEngine> mockEngine = new Mock<RazorTemplateEngine>(CreateHost());
            var reader = new StringReader("foo");
            var source = new CancellationTokenSource();

            // Act
            mockEngine.Object.ParseTemplate(reader, cancelToken: source.Token);

            // Assert
            mockEngine.Verify(e => e.ParseTemplateCore(It.Is<SeekableTextReader>(l => l.ReadToEnd() == "foo"),
                                                       source.Token));
        }

        [Fact]
        public void GenerateCodeCopiesTextReaderContentToSeekableTextReaderAndPassesToGenerateCodeCore()
        {
            // Arrange
            Mock<RazorTemplateEngine> mockEngine = new Mock<RazorTemplateEngine>(CreateHost());
            var reader = new StringReader("foo");
            var source = new CancellationTokenSource();
            var className = "Foo";
            var ns = "Bar";
            var src = "Baz";

            // Act
            mockEngine.Object.GenerateCode(reader, className: className, rootNamespace: ns, sourceFileName: src, cancelToken: source.Token);

            // Assert
            mockEngine.Verify(e => e.GenerateCodeCore(It.Is<SeekableTextReader>(l => l.ReadToEnd() == "foo"),
                                                      className, ns, src, source.Token));
        }

        [Fact]
        public void ParseTemplateOutputsResultsOfParsingProvidedTemplateSource()
        {
            // Arrange
            var engine = new RazorTemplateEngine(CreateHost());

            // Act
            var results = engine.ParseTemplate(new StringTextBuffer("foo @bar("));

            // Assert
            Assert.False(results.Success);
            Assert.Single(results.ParserErrors);
            Assert.NotNull(results.Document);
        }

        [Fact]
        public void GenerateOutputsResultsOfParsingAndGeneration()
        {
            // Arrange
            var engine = new RazorTemplateEngine(CreateHost());

            // Act
            var results = engine.GenerateCode(new StringTextBuffer("foo @bar("));

            // Assert
            Assert.False(results.Success);
            Assert.Single(results.ParserErrors);
            Assert.NotNull(results.Document);
            Assert.NotNull(results.GeneratedCode);
        }

        [Fact]
        public void GenerateOutputsDesignTimeMappingsIfDesignTimeSetOnHost()
        {
            // Arrange
            var engine = new RazorTemplateEngine(CreateHost(designTime: true));

            // Act
            var results = engine.GenerateCode(new StringTextBuffer("foo @bar()"), className: null, rootNamespace: null, sourceFileName: "foo.cshtml");

            // Assert
            Assert.True(results.Success);
            Assert.Empty(results.ParserErrors);
            Assert.NotNull(results.Document);
            Assert.NotNull(results.GeneratedCode);
            Assert.NotNull(results.DesignTimeLineMappings);
        }

        private static RazorEngineHost CreateHost(bool designTime = false)
        {
            return new RazorEngineHost(new CSharpRazorCodeLanguage())
            {
                DesignTimeMode = designTime
            };
        }
    }
}
