﻿using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.TestCommon;
using Moq;

namespace Microsoft.AspNet.Razor.Test.Generator.CodeTree
{
    public class CSharpCodeBuilderTests
    {
        [Fact]
        public void CodeTreeWithUsings()
        {
            var syntaxTreeNode = new Mock<Span>(new SpanBuilder());
            var language = new CSharpRazorCodeLanguage();
            var host = new RazorEngineHost(language);
            var context = CodeGeneratorContext.Create(host, "TestClass", "TestNamespace", "Foo.cs", shouldGenerateLinePragmas: false);
            context.CodeTreeBuilder.AddUsingChunk("FakeNamespace1", syntaxTreeNode.Object);
            context.CodeTreeBuilder.AddUsingChunk("FakeNamespace2.SubNamespace", syntaxTreeNode.Object);
            var codeBuilder = language.CreateCodeBuilder(context);

            // Act
            var result = codeBuilder.Build();
            // Assert
            Assert.Equal(@"namespace TestNamespace
{
#line 1 """"
using FakeNamespace1

#line default
#line hidden
    ;
#line 1 """"
using FakeNamespace2.SubNamespace

#line default
#line hidden
    ;
    using System.Threading.Tasks;

    public class TestClass
    {
        #line hidden
        public TestClass()
        {
        }

        public override async Task ExecuteAsync()
        {
        }
    }
}", result.Code.TrimEnd());
        }
    }
}
