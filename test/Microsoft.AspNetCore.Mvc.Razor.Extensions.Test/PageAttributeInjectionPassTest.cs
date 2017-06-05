﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions
{
    public class PageAttributeInjectionPassTest
    {
        [Fact]
        public void Execute_NoOps_IfNamespaceNodeIsMissing()
        {
            // Arrange
            var irDocument = new DocumentIRNode();
            var pass = new PageAttributeInjectionPass
            {
                Engine = RazorEngine.Create(),
            };

            // Act
            pass.Execute(TestRazorCodeDocument.CreateEmpty(), irDocument);

            // Assert
            Assert.Empty(irDocument.Children);
        }

        [Fact]
        public void Execute_NoOps_IfNamespaceNodeHasEmptyContent()
        {
            // Arrange
            var irDocument = new DocumentIRNode();
            var builder = RazorIRBuilder.Create(irDocument);
            var @namespace = new NamespaceDeclarationIRNode() { Content = string.Empty };
            builder.Push(@namespace);

            var pass = new PageAttributeInjectionPass
            {
                Engine = RazorEngine.Create(),
            };

            // Act
            pass.Execute(TestRazorCodeDocument.CreateEmpty(), irDocument);

            // Assert
            Assert.Collection(irDocument.Children,
                node => Assert.Same(@namespace, node));
        }

        [Fact]
        public void Execute_NoOps_IfClassNameNodeIsMissing()
        {
            // Arrange
            var irDocument = new DocumentIRNode();
            var builder = RazorIRBuilder.Create(irDocument);
            var @namespace = new NamespaceDeclarationIRNode() { Content = "SomeNamespace" };
            builder.Push(@namespace);

            var pass = new PageAttributeInjectionPass
            {
                Engine = RazorEngine.Create(),
            };

            // Act
            pass.Execute(TestRazorCodeDocument.CreateEmpty(), irDocument);

            // Assert
            Assert.Collection(irDocument.Children,
                node => Assert.Same(@namespace, node));
        }

        [Fact]
        public void Execute_NoOps_IfClassNameIsEmpty()
        {
            // Arrange
            var irDocument = new DocumentIRNode();
            var builder = RazorIRBuilder.Create(irDocument);
            var @namespace = new NamespaceDeclarationIRNode() { Content = "SomeNamespace" };
            builder.Push(@namespace);
            var @class = new ClassDeclarationIRNode();
            builder.Add(@class);

            var pass = new PageAttributeInjectionPass
            {
                Engine = RazorEngine.Create(),
            };

            // Act
            pass.Execute(TestRazorCodeDocument.CreateEmpty(), irDocument);

            // Assert
            Assert.Collection(irDocument.Children,
                node => Assert.Same(@namespace, node));
        }

        [Fact]
        public void Execute_NoOps_IfDocumentIsNotViewOrPage()
        {
            // Arrange
            var irDocument = new DocumentIRNode
            {
                DocumentKind = "Default",
            };
            var builder = RazorIRBuilder.Create(irDocument);
            var @namespace = new NamespaceDeclarationIRNode() { Content = "SomeNamespace" };
            builder.Push(@namespace);
            var @class = new ClassDeclarationIRNode { Name = "SomeName" };
            builder.Add(@class);

            var pass = new PageAttributeInjectionPass
            {
                Engine = RazorEngine.Create(),
            };

            // Act
            pass.Execute(TestRazorCodeDocument.CreateEmpty(), irDocument);

            // Assert
            Assert.Collection(irDocument.Children,
                node => Assert.Same(@namespace, node));
        }

        [Fact]
        public void Execute_AddsRazorViewAttribute_ToViews()
        {
            // Arrange
            var expectedAttribute = "[assembly:Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@\"/Views/Index.cshtml\", typeof(SomeNamespace.SomeName))]";
            var irDocument = new DocumentIRNode
            {
                DocumentKind = MvcViewDocumentClassifierPass.MvcViewDocumentKind,
            };
            var builder = RazorIRBuilder.Create(irDocument);
            var @namespace = new NamespaceDeclarationIRNode() { Content = "SomeNamespace" };
            builder.Push(@namespace);
            var @class = new ClassDeclarationIRNode { Name = "SomeName" };
            builder.Add(@class);

            var pass = new PageAttributeInjectionPass
            {
                Engine = RazorEngine.Create(),
            };
            var document = TestRazorCodeDocument.CreateEmpty();
            document.SetRelativePath("/Views/Index.cshtml");

            // Act
            pass.Execute(document, irDocument);

            // Assert
            Assert.Collection(irDocument.Children,
                node =>
                {
                    var csharpStatement = Assert.IsType<CSharpStatementIRNode>(node);
                    var token = Assert.IsType<RazorIRToken>(Assert.Single(csharpStatement.Children));
                    Assert.Equal(RazorIRToken.TokenKind.CSharp, token.Kind);
                    Assert.Equal(expectedAttribute, token.Content);
                },
                node => Assert.Same(@namespace, node));
        }

        [Fact]
        public void Execute_EscapesViewPathWhenAddingAttributeToViews()
        {
            // Arrange
            var expectedAttribute = "[assembly:Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@\"\\test\\\"\"Index.cshtml\", typeof(SomeNamespace.SomeName))]";
            var irDocument = new DocumentIRNode
            {
                DocumentKind = MvcViewDocumentClassifierPass.MvcViewDocumentKind,
            };
            var builder = RazorIRBuilder.Create(irDocument);
            var @namespace = new NamespaceDeclarationIRNode() { Content = "SomeNamespace" };
            builder.Push(@namespace);
            var @class = new ClassDeclarationIRNode { Name = "SomeName" };
            builder.Add(@class);

            var pass = new PageAttributeInjectionPass
            {
                Engine = RazorEngine.Create(),
            };
            var document = TestRazorCodeDocument.CreateEmpty();
            document.SetRelativePath("\\test\\\"Index.cshtml");

            // Act
            pass.Execute(document, irDocument);

            // Assert
            Assert.Collection(irDocument.Children,
                node =>
                {
                    var csharpStatement = Assert.IsType<CSharpStatementIRNode>(node);
                    var token = Assert.IsType<RazorIRToken>(Assert.Single(csharpStatement.Children));
                    Assert.Equal(RazorIRToken.TokenKind.CSharp, token.Kind);
                    Assert.Equal(expectedAttribute, token.Content);
                },
                node => Assert.Same(@namespace, node));
        }

        [Fact]
        public void Execute_AddsRazorPagettribute_ToPage()
        {
            // Arrange
            var expectedAttribute = "[assembly:Microsoft.AspNetCore.Mvc.RazorPages.Infrastructure.RazorPageAttribute(@\"/Views/Index.cshtml\", typeof(SomeNamespace.SomeName), null)]";
            var irDocument = new DocumentIRNode
            {
                DocumentKind = RazorPageDocumentClassifierPass.RazorPageDocumentKind,
            };
            var builder = RazorIRBuilder.Create(irDocument);
            var pageDirective = new DirectiveIRNode
            {
                Descriptor = PageDirective.Directive,
            };
            builder.Add(pageDirective);

            var @namespace = new NamespaceDeclarationIRNode() { Content = "SomeNamespace" };
            builder.Push(@namespace);
            var @class = new ClassDeclarationIRNode { Name = "SomeName" };
            builder.Add(@class);

            var pass = new PageAttributeInjectionPass
            {
                Engine = RazorEngine.Create(),
            };
            var document = TestRazorCodeDocument.CreateEmpty();
            document.SetRelativePath("/Views/Index.cshtml");

            // Act
            pass.Execute(document, irDocument);

            // Assert
            Assert.Collection(irDocument.Children,
                node => Assert.Same(pageDirective, node),
                node =>
                {
                    var csharpStatement = Assert.IsType<CSharpStatementIRNode>(node);
                    var token = Assert.IsType<RazorIRToken>(Assert.Single(csharpStatement.Children));
                    Assert.Equal(RazorIRToken.TokenKind.CSharp, token.Kind);
                    Assert.Equal(expectedAttribute, token.Content);
                },
                node => Assert.Same(@namespace, node));
        }

        [Fact]
        public void Execute_EscapesViewPathAndRouteWhenAddingAttributeToPage()
        {
            // Arrange
            var expectedAttribute = "[assembly:Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(@\"\\test\\\"\"Index.cshtml\", typeof(SomeNamespace.SomeName))]";
            var irDocument = new DocumentIRNode
            {
                DocumentKind = MvcViewDocumentClassifierPass.MvcViewDocumentKind,
            };
            var builder = RazorIRBuilder.Create(irDocument);
            var @namespace = new NamespaceDeclarationIRNode() { Content = "SomeNamespace" };
            builder.Push(@namespace);
            var @class = new ClassDeclarationIRNode { Name = "SomeName" };
            builder.Add(@class);

            var pass = new PageAttributeInjectionPass
            {
                Engine = RazorEngine.Create(),
            };
            var document = TestRazorCodeDocument.CreateEmpty();
            document.SetRelativePath("\\test\\\"Index.cshtml");

            // Act
            pass.Execute(document, irDocument);

            // Assert
            Assert.Collection(irDocument.Children,
                node =>
                {
                    var csharpStatement = Assert.IsType<CSharpStatementIRNode>(node);
                    var token = Assert.IsType<RazorIRToken>(Assert.Single(csharpStatement.Children));
                    Assert.Equal(RazorIRToken.TokenKind.CSharp, token.Kind);
                    Assert.Equal(expectedAttribute, token.Content);
                },
                node => Assert.Same(@namespace, node));
        }

        private RazorEngine CreateEngine()
        {
            return RazorEngine.Create(b =>
            {
                // Notice we're not registering the InjectDirective.Pass here so we can run it on demand.
                b.Features.Add(new PageAttributeInjectionPass());
            });
        }

        private DocumentIRNode CreateIRDocument(RazorEngine engine, RazorCodeDocument codeDocument)
        {
            for (var i = 0; i < engine.Phases.Count; i++)
            {
                var phase = engine.Phases[i];
                phase.Execute(codeDocument);

                if (phase is IRazorDocumentClassifierPhase)
                {
                    break;
                }
            }

            return codeDocument.GetIRDocument();
        }
    }
}
