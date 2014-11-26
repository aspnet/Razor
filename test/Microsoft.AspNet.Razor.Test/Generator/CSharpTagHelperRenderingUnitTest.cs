using System;
using System.Collections.Generic;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;
using Microsoft.AspNet.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class CSharpTagHelperRenderingUnitTest
    {
        [Fact]
        public void CreatesAUniqueIdForSingleTagHelperChunk()
        {
            var chunk = new TagHelperChunk
            {
                TagName = "div",
                Descriptors = new[] { new TagHelperDescriptor("div", "DivTagHelper", "FakeAssemblyName", ContentBehavior.None) },
                Children = new List<Chunk>(),
                Attributes = new Dictionary<string, Chunk>()
            };
            var codeRenderer = CreateCodeRenderer();

            codeRenderer.RenderTagHelper(chunk);

            Assert.Equal(1, codeRenderer.GenerateUniqueIdCount);
        }

        [Fact]
        public void UsesTheSameUniqueIdForTagHelperChunkWithMultipleTagHelpers()
        {
            var chunk = new TagHelperChunk
            {
                TagName = "div",
                Descriptors = new[] {
                    new TagHelperDescriptor("div", "DivTagHelper", "FakeAssemblyName", ContentBehavior.None),
                    new TagHelperDescriptor("div", "Div2TagHelper", "FakeAssemblyName", ContentBehavior.None)
                },
                Children = new List<Chunk>(),
                Attributes = new Dictionary<string, Chunk>()
            };
            var codeRenderer = CreateCodeRenderer();

            codeRenderer.RenderTagHelper(chunk);

            Assert.Equal(1, codeRenderer.GenerateUniqueIdCount);
        }

        [Fact]
        public void UsesDifferentUniqueIdForMultipleTagHelperChunksForSameTagHelper()
        {
            var chunk1 = new TagHelperChunk
            {
                TagName = "div",
                Descriptors = new[] { new TagHelperDescriptor("div", "DivTagHelper", "FakeAssemblyName", ContentBehavior.None) },
                Children = new List<Chunk>(),
                Attributes = new Dictionary<string, Chunk>()
            };
            var chunk2 = new TagHelperChunk
            {
                TagName = "div",
                Descriptors = new[] { new TagHelperDescriptor("div", "DivTagHelper", "FakeAssemblyName", ContentBehavior.None) },
                Children = new List<Chunk>(),
                Attributes = new Dictionary<string, Chunk>()
            };
            var codeRenderer = CreateCodeRenderer();

            codeRenderer.RenderTagHelper(chunk1);
            codeRenderer.RenderTagHelper(chunk2);

            Assert.Equal(2, codeRenderer.GenerateUniqueIdCount);
        }

        [Fact]
        public void UsesDifferentUniqueIdForMultipleTagHelperChunksForDifferentTagHelpers()
        {
            var divChunk = new TagHelperChunk
            {
                TagName = "div",
                Descriptors = new[] { new TagHelperDescriptor("div", "DivTagHelper", "FakeAssemblyName", ContentBehavior.None) },
                Children = new List<Chunk>(),
                Attributes = new Dictionary<string, Chunk>()
            };
            var spanChunk = new TagHelperChunk
            {
                TagName = "span",
                Descriptors = new[] { new TagHelperDescriptor("span", "SpanTagHelper", "FakeAssemblyName", ContentBehavior.None) },
                Children = new List<Chunk>(),
                Attributes = new Dictionary<string, Chunk>()
            };
            var codeRenderer = CreateCodeRenderer();

            codeRenderer.RenderTagHelper(divChunk);
            codeRenderer.RenderTagHelper(spanChunk);

            Assert.Equal(2, codeRenderer.GenerateUniqueIdCount);
        }

        [Fact]
        public void UsesCorrectUniqueIdForMultipleTagHelperChunksSomeWithSameSameTagHelpersSomeWithDifferentTagHelpers()
        {
            var chunk1 = new TagHelperChunk
            {
                TagName = "div",
                Descriptors = new[] {
                    new TagHelperDescriptor("div", "DivTagHelper", "FakeAssemblyName", ContentBehavior.None),
                    new TagHelperDescriptor("div", "Div2TagHelper", "FakeAssemblyName", ContentBehavior.None)
                },
                Children = new List<Chunk>(),
                Attributes = new Dictionary<string, Chunk>()
            };
            var chunk2 = new TagHelperChunk
            {
                TagName = "span",
                Descriptors = new[] { new TagHelperDescriptor("span", "SpanTagHelper", "FakeAssemblyName", ContentBehavior.None) },
                Children = new List<Chunk>(),
                Attributes = new Dictionary<string, Chunk>()
            };
            var chunk3 = new TagHelperChunk
            {
                TagName = "span",
                Descriptors = new[] {
                    new TagHelperDescriptor("span", "SpanTagHelper", "FakeAssemblyName", ContentBehavior.None),
                    new TagHelperDescriptor("span", "Span2TagHelper", "FakeAssemblyName", ContentBehavior.None)
                },
                Children = new List<Chunk>(),
                Attributes = new Dictionary<string, Chunk>()
            };
            var codeRenderer = CreateCodeRenderer();

            codeRenderer.RenderTagHelper(chunk1);
            codeRenderer.RenderTagHelper(chunk2);
            codeRenderer.RenderTagHelper(chunk3);

            Assert.Equal(3, codeRenderer.GenerateUniqueIdCount);
        }

        private static TrackingUniqueIdsTagHelperCodeRenderer CreateCodeRenderer()
        {
            var writer = new CSharpCodeWriter();
            var codeBuilderContext = CreateContext();
            var codeRenderer = new TrackingUniqueIdsTagHelperCodeRenderer(new CSharpCodeVisitor(writer, codeBuilderContext), writer, codeBuilderContext);
            return codeRenderer;
        }

        private static CodeBuilderContext CreateContext()
        {
            return new CodeBuilderContext(
                new CodeGeneratorContext(
                    new RazorEngineHost(new CSharpRazorCodeLanguage()),
                    "MyClass",
                    "MyNamespace",
                    string.Empty,
                    shouldGenerateLinePragmas: true));
        }

        private class TrackingUniqueIdsTagHelperCodeRenderer : CSharpTagHelperCodeRenderer
        {
            public TrackingUniqueIdsTagHelperCodeRenderer(
                IChunkVisitor bodyVisitor,
                CSharpCodeWriter writer,
                CodeBuilderContext context)
                : base(bodyVisitor, writer, context)
            {

            }

            internal override string GenerateUniqueId()
            {
                GenerateUniqueIdCount++;
                return "test";
            }

            public int GenerateUniqueIdCount { get; private set; }
        }
    }
}