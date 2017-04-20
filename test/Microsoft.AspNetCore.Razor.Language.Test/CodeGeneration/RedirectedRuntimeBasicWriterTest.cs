﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language.CodeGeneration
{
    public class RedirectedRuntimeBasicWriterTest
    {
        [Fact]
        public void WriteCSharpExpression_Runtime_SkipsLinePragma_WithoutSource()
        {
            // Arrange
            var writer = new RedirectedRuntimeBasicWriter("test_writer")
            {
                WriteCSharpExpressionMethod = "Test",
            };

            var context = new CSharpRenderingContext()
            {
                Options = RazorParserOptions.CreateDefaultOptions(),
                Writer = new Legacy.CSharpCodeWriter(),
            };

            var node = new CSharpExpressionIRNode();
            var builder = RazorIRBuilder.Create(node);
            builder.Add(new RazorIRToken()
            {
                Content = "i++",
                Kind = RazorIRToken.TokenKind.CSharp,
            });

            // Act
            writer.WriteCSharpExpression(context, node);

            // Assert
            var csharp = context.Writer.Builder.ToString();
            Assert.Equal(
@"Test(test_writer, i++);
",
                csharp,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void WriteCSharpExpression_Runtime_WritesLinePragma_WithSource()
        {
            // Arrange
            var writer = new RedirectedRuntimeBasicWriter("test_writer")
            {
                WriteCSharpExpressionMethod = "Test",
            };

            var context = new CSharpRenderingContext()
            {
                Options = RazorParserOptions.CreateDefaultOptions(),
                Writer = new Legacy.CSharpCodeWriter(),
            };

            var node = new CSharpExpressionIRNode()
            {
                Source = new SourceSpan("test.cshtml", 0, 0, 0, 3),
            };
            var builder = RazorIRBuilder.Create(node);
            builder.Add(new RazorIRToken()
            {
                Content = "i++",
                Kind = RazorIRToken.TokenKind.CSharp,
            });

            // Act
            writer.WriteCSharpExpression(context, node);

            // Assert
            var csharp = context.Writer.Builder.ToString();
            Assert.Equal(
@"#line 1 ""test.cshtml""
Test(test_writer, i++);

#line default
#line hidden
",
                csharp,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void WriteCSharpExpression_Runtime_WithExtensionNode_WritesPadding()
        {
            // Arrange
            var writer = new RedirectedRuntimeBasicWriter("test_writer")
            {
                WriteCSharpExpressionMethod = "Test",
            };

            var context = new CSharpRenderingContext()
            {
                Options = RazorParserOptions.CreateDefaultOptions(),
                Writer = new Legacy.CSharpCodeWriter(),
            };

            var node = new CSharpExpressionIRNode();
            var builder = RazorIRBuilder.Create(node);
            builder.Add(new RazorIRToken()
            {
                Content = "i",
                Kind = RazorIRToken.TokenKind.CSharp,
            });
            builder.Add(new MyExtensionIRNode());
            builder.Add(new RazorIRToken()
            {
                Content = "++",
                Kind = RazorIRToken.TokenKind.CSharp,
            });

            context.RenderNode = (n) => Assert.IsType<MyExtensionIRNode>(n);

            // Act
            writer.WriteCSharpExpression(context, node);

            // Assert
            var csharp = context.Writer.Builder.ToString();
            Assert.Equal(
@"Test(test_writer, i++);
",
                csharp,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void WriteCSharpExpression_Runtime_WithSource_WritesPadding()
        {
            // Arrange
            var writer = new RedirectedRuntimeBasicWriter("test_writer")
            {
                WriteCSharpExpressionMethod = "Test",
            };
            var sourceDocument = TestRazorSourceDocument.Create("                     @i++");

            var context = new CSharpRenderingContext()
            {
                Options = RazorParserOptions.CreateDefaultOptions(),
                CodeDocument = RazorCodeDocument.Create(sourceDocument),
                Writer = new Legacy.CSharpCodeWriter(),
            };

            var node = new CSharpExpressionIRNode()
            {
                Source = new SourceSpan("test.cshtml", 24, 0, 24, 3),
            };
            var builder = RazorIRBuilder.Create(node);
            builder.Add(new RazorIRToken()
            {
                Content = "i",
                Kind = RazorIRToken.TokenKind.CSharp,
            });
            builder.Add(new MyExtensionIRNode());
            builder.Add(new RazorIRToken()
            {
                Content = "++",
                Kind = RazorIRToken.TokenKind.CSharp,
            });

            context.RenderNode = (n) => Assert.IsType<MyExtensionIRNode>(n);

            // Act
            writer.WriteCSharpExpression(context, node);

            // Assert
            var csharp = context.Writer.Builder.ToString();
            Assert.Equal(
@"#line 1 ""test.cshtml""
      Test(test_writer, i++);

#line default
#line hidden
",
                csharp,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void WriteHtmlContent_RendersContentCorrectly()
        {
            var writer = new RedirectedRuntimeBasicWriter("test_writer");
            var context = new CSharpRenderingContext()
            {
                Writer = new Legacy.CSharpCodeWriter(),
                Options = RazorParserOptions.CreateDefaultOptions(),
            };

            var node = new HtmlContentIRNode();
            node.Children.Add(new RazorIRToken()
            {
                Content = "SomeContent",
                Kind = RazorIRToken.TokenKind.Html,
                Parent = node
            });

            // Act
            writer.WriteHtmlContent(context, node);

            // Assert
            var csharp = context.Writer.Builder.ToString();
            Assert.Equal(
@"WriteLiteralTo(test_writer, ""SomeContent"");
",
                csharp,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void WriteHtmlContent_LargeStringLiteral_UsesMultipleWrites()
        {
            var writer = new RedirectedRuntimeBasicWriter("test_writer");
            var context = new CSharpRenderingContext()
            {
                Writer = new Legacy.CSharpCodeWriter(),
                Options = RazorParserOptions.CreateDefaultOptions(),
            };

            var node = new HtmlContentIRNode();
            node.Children.Add(new RazorIRToken()
            {
                Content = new string('*', 2000),
                Kind = RazorIRToken.TokenKind.Html,
                Parent = node
            });

            // Act
            writer.WriteHtmlContent(context, node);

            // Assert
            var csharp = context.Writer.Builder.ToString();
            Assert.Equal(string.Format(
@"WriteLiteralTo(test_writer, @""{0}"");
WriteLiteralTo(test_writer, @""{1}"");
", new string('*', 1024), new string('*', 976)),
                csharp,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void WriteHtmlAttribute_RendersCorrectly()
        {
            var writer = new RedirectedRuntimeBasicWriter("test_writer");
            var context = GetCSharpRenderingContext(writer);

            var content = "<input checked=\"hello-world @false\" />";
            var sourceDocument = TestRazorSourceDocument.Create(content);
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var irDocument = Lower(codeDocument);
            var node = irDocument.Children.OfType<HtmlAttributeIRNode>().Single();

            // Act
            writer.WriteHtmlAttribute(context, node);

            // Assert
            var csharp = context.Writer.Builder.ToString();
            Assert.Equal(
@"BeginWriteAttributeTo(test_writer, ""checked"", "" checked=\"""", 6, ""\"""", 34, 2);
Render Children
EndWriteAttributeTo(test_writer);
",
                csharp,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void WriteHtmlAttributeValue_RendersCorrectly()
        {
            var writer = new RedirectedRuntimeBasicWriter("test_writer");
            var context = GetCSharpRenderingContext(writer);

            var content = "<input checked=\"hello-world @false\" />";
            var sourceDocument = TestRazorSourceDocument.Create(content);
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var irDocument = Lower(codeDocument);
            var node = irDocument.Children.OfType<HtmlAttributeIRNode>().Single().Children[0] as HtmlAttributeValueIRNode;

            // Act
            writer.WriteHtmlAttributeValue(context, node);

            // Assert
            var csharp = context.Writer.Builder.ToString();
            Assert.Equal(
@"WriteAttributeValueTo(test_writer, """", 16, ""hello-world"", 16, 11, true);
",
                csharp,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void WriteCSharpAttributeValue_RendersCorrectly()
        {
            var writer = new RedirectedRuntimeBasicWriter("test_writer");
            var context = GetCSharpRenderingContext(writer);

            var content = "<input checked=\"hello-world @false\" />";
            var sourceDocument = TestRazorSourceDocument.Create(content);
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var irDocument = Lower(codeDocument);
            var node = irDocument.Children.OfType<HtmlAttributeIRNode>().Single().Children[1] as CSharpAttributeValueIRNode;

            // Act
            writer.WriteCSharpAttributeValue(context, node);

            // Assert
            var csharp = context.Writer.Builder.ToString();
            Assert.Equal(
@"#line 1 ""test.cshtml""
WriteAttributeValueTo(test_writer, "" "", 27, false, 28, 6, false);

#line default
#line hidden
",
                csharp,
                ignoreLineEndingDifferences: true);
        }

        [Fact]
        public void WriteCSharpAttributeValue_NonExpression_BuffersResult()
        {
            var writer = new RedirectedRuntimeBasicWriter("test_writer");
            var context = GetCSharpRenderingContext(writer);

            var content = "<input checked=\"hello-world @if(@true){ }\" />";
            var sourceDocument = TestRazorSourceDocument.Create(content);
            var codeDocument = RazorCodeDocument.Create(sourceDocument);
            var irDocument = Lower(codeDocument);
            var node = irDocument.Children.OfType<HtmlAttributeIRNode>().Single().Children[1] as CSharpAttributeValueIRNode;

            // Act
            writer.WriteCSharpAttributeValue(context, node);

            // Assert
            var csharp = context.Writer.Builder.ToString();
            Assert.Equal(
@"WriteAttributeValueTo(test_writer, "" "", 27, new Microsoft.AspNetCore.Mvc.Razor.HelperResult(async(__razor_attribute_value_writer) => {
    Render Children
}
), 28, 13, false);
",
                csharp,
                ignoreLineEndingDifferences: true);
        }

        private static CSharpRenderingContext GetCSharpRenderingContext(BasicWriter writer)
        {
            var options = RazorParserOptions.CreateDefaultOptions();
            var codeWriter = new Legacy.CSharpCodeWriter();
            var context = new CSharpRenderingContext()
            {
                Writer = codeWriter,
                Options = options,
                BasicWriter = writer,
                RenderChildren = n =>
                {
                    codeWriter.WriteLine("Render Children");
                }
            };

            return context;
        }

        private static DocumentIRNode Lower(RazorCodeDocument codeDocument)
        {
            var engine = RazorEngine.Create();

            return Lower(codeDocument, engine);
        }

        private static DocumentIRNode Lower(RazorCodeDocument codeDocument, RazorEngine engine)
        {
            for (var i = 0; i < engine.Phases.Count; i++)
            {
                var phase = engine.Phases[i];
                phase.Execute(codeDocument);

                if (phase is IRazorIRLoweringPhase)
                {
                    break;
                }
            }

            var irDocument = codeDocument.GetIRDocument();
            Assert.NotNull(irDocument);

            return irDocument;
        }

        private class MyExtensionIRNode : ExtensionIRNode
        {
            public override IList<RazorIRNode> Children => throw new NotImplementedException();

            public override RazorIRNode Parent { get; set; }
            public override SourceSpan? Source { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

            public override void Accept(RazorIRNodeVisitor visitor)
            {
                throw new NotImplementedException();
            }

            public override void WriteNode(RuntimeTarget target, CSharpRenderingContext context)
            {
                throw new NotImplementedException();
            }
        }
    }
}
