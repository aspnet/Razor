using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Generator
{
    public class CSharpTagHelperRenderingUnitTest
    {
        [Fact]
        public void CreatesAUniqueIdForSingleTagHelperChunk()
        {
            var chunk = new TagHelperChunk { TagName = "div" };
            chunk.Descriptors = new[] { new TagHelperDescriptor("div", "DivTagHelper", "FakeAssemblyName", ContentBehavior.None ) };
            chunk.Children = new List<Chunk>();
            chunk.Attributes = new Dictionary<string, Chunk>();
            var writer = new CSharpCodeWriter();
            var codeBuilderContext = CreateContext();
            var codeRenderer = new TrackingUniqueIdsTagHelperCodeRenderer(new CSharpCodeVisitor(writer, codeBuilderContext), writer, codeBuilderContext);

            codeRenderer.RenderTagHelper(chunk);

            Assert.Equal(1, codeRenderer.GenerateUniqueIdCount);
        }

        [Fact]
        public void UsesTheSameUniqueIdForTagHelperChunkWithMultipleTagHelpers()
        {
            var chunk = new TagHelperChunk { TagName = "div" };
            chunk.Descriptors = new[] {
                new TagHelperDescriptor("div", "DivTagHelper", "FakeAssemblyName", ContentBehavior.None),
                new TagHelperDescriptor("div", "Div2TagHelper", "FakeAssemblyName", ContentBehavior.None)
            };
            chunk.Children = new List<Chunk>();
            chunk.Attributes = new Dictionary<string, Chunk>();
            var writer = new CSharpCodeWriter();
            var codeBuilderContext = CreateContext();
            var codeRenderer = new TrackingUniqueIdsTagHelperCodeRenderer(new CSharpCodeVisitor(writer, codeBuilderContext), writer, codeBuilderContext);

            codeRenderer.RenderTagHelper(chunk);

            Assert.Equal(1, codeRenderer.GenerateUniqueIdCount);
        }


        private static CodeBuilderContext CreateContext()
        {
            return new CodeBuilderContext(
                new CodeGeneratorContext(new RazorEngineHost(new CSharpRazorCodeLanguage()),
                    "MyClass",
                    "MyNamespace",
                    string.Empty,
                    shouldGenerateLinePragmas: true));
        }

        private class TrackingUniqueIdsTagHelperCodeRenderer : CSharpTagHelperCodeRenderer
        {
            public TrackingUniqueIdsTagHelperCodeRenderer(IChunkVisitor bodyVisitor,
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