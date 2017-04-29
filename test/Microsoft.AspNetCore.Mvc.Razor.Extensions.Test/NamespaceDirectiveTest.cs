﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions
{
    public class NamespaceDirectiveTest
    {
        [Fact]
        public void TryComputeNamespace_IncompleteDirective_UsesEmptyNamespace()
        {
            // Arrange
            var source = "c:\\foo\\bar\\bleh.cshtml";
            var imports = "c:\\foo\\baz\\bleh.cshtml";
            var node = new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan(imports, 0, 0, 0, 0),
            };

            // Act
            var computed = NamespaceDirective.TryComputeNamespace(source, node, out var @namespace);

            // Assert
            Assert.False(computed);
            Assert.Equal(string.Empty, @namespace);
        }

        [Fact]
        public void TryComputeNamespace_EmptyDirective_UsesEmptyNamespace()
        {
            // Arrange
            var source = "c:\\foo\\bar\\bleh.cshtml";
            var imports = "c:\\foo\\baz\\bleh.cshtml";
            var node = new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan(imports, 0, 0, 0, 0),
            };
            node.Children.Add(new DirectiveTokenIRNode() { Content = string.Empty });
            node.Children[0].Parent = node;

            // Act
            var computed = NamespaceDirective.TryComputeNamespace(source, node, out var @namespace);

            // Assert
            Assert.False(computed);
            Assert.Equal(string.Empty, @namespace);
        }

        // When we don't have a relationship between the source file and the imports file
        // we will just use the namespace on the node directly.
        [Theory]
        [InlineData((string)null, (string)null)]
        [InlineData("", "")]
        [InlineData(null, "/foo/bar")]
        [InlineData("/foo/baz", "/foo/bar/bleh")]
        [InlineData("/foo.cshtml", "/foo/bar.cshtml")]
        [InlineData("c:\\foo.cshtml", "d:\\foo\\bar.cshtml")]
        [InlineData("c:\\foo\\bar\\bleh.cshtml", "c:\\foo\\baz\\bleh.cshtml")]
        public void TryComputeNamespace_ForNonRelatedFiles_UsesNamespaceVerbatim(string source, string imports)
        {
            // Arrange
            var node = new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan(imports, 0, 0, 0, 0),
            };

            node.Children.Add(new DirectiveTokenIRNode() { Content = "Base" });
            node.Children[0].Parent = node;

            // Act
            var computed = NamespaceDirective.TryComputeNamespace(source, node, out var @namespace);

            // Assert
            Assert.False(computed);
            Assert.Equal("Base", @namespace);
        }

        [Theory]
        [InlineData("/foo.cshtml", "/_ViewImports.cshtml", "Base")]
        [InlineData("/foo/bar.cshtml", "/_ViewImports.cshtml", "Base.foo")]
        [InlineData("/foo/bar/baz.cshtml", "/_ViewImports.cshtml", "Base.foo.bar")]
        [InlineData("/foo/bar/baz.cshtml", "/foo/_ViewImports.cshtml", "Base.bar")]
        [InlineData("/Foo/bar/baz.cshtml", "/foo/_ViewImports.cshtml", "Base.bar")]
        [InlineData("c:\\foo.cshtml", "c:\\_ViewImports.cshtml", "Base")]
        [InlineData("c:\\foo\\bar.cshtml", "c:\\_ViewImports.cshtml", "Base.foo")]
        [InlineData("c:\\foo\\bar\\baz.cshtml", "c:\\_ViewImports.cshtml", "Base.foo.bar")]
        [InlineData("c:\\foo\\bar\\baz.cshtml", "c:\\foo\\_ViewImports.cshtml", "Base.bar")]
        [InlineData("c:\\Foo\\bar\\baz.cshtml", "c:\\foo\\_ViewImports.cshtml", "Base.bar")]
        public void TryComputeNamespace_ForRelatedFiles_ComputesNamespaceWithSuffix(string source, string imports, string expected)
        {
            // Arrange
            var node = new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan(imports, 0, 0, 0, 0),
            };

            node.Children.Add(new DirectiveTokenIRNode() { Content = "Base" });
            node.Children[0].Parent = node;

            // Act
            var computed = NamespaceDirective.TryComputeNamespace(source, node, out var @namespace);

            // Assert
            Assert.True(computed);
            Assert.Equal(expected, @namespace);
        }

        // This is the case where a _ViewImports sets the namespace.
        [Fact]
        public void Pass_SetsNamespaceAndClassName_ComputedFromImports()
        {
            // Arrange
            var builder = RazorIRBuilder.Document();

            builder.Push(new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan("/Account/_ViewImports.cshtml", 0, 0, 0, 0),
            });
            builder.Add(new DirectiveTokenIRNode() { Content = "WebApplication.Account" });
            builder.Pop();

            var @namespace = new NamespaceDeclarationIRNode() { Content = "default" };
            builder.Push(@namespace);

            var @class = new ClassDeclarationIRNode() { Name = "default" };
            builder.Add(@class);

            var irDocument = (DocumentIRNode)builder.Build();
            irDocument.DocumentKind = RazorPageDocumentClassifierPass.RazorPageDocumentKind;

            var codeDocument = RazorCodeDocument.Create(RazorSourceDocument.Create("ignored", "/Account/Manage/AddUser.cshtml"));

            var pass = new NamespaceDirective.Pass();
            pass.Engine = RazorEngine.CreateEmpty(b => { });

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            Assert.Equal("WebApplication.Account.Manage", @namespace.Content);
            Assert.Equal("AddUser_Page", @class.Name);
        }

        // This is the case where the source file sets the namespace.
        [Fact]
        public void Pass_SetsNamespaceAndClassName_ComputedFromSource()
        {
            // Arrange
            var builder = RazorIRBuilder.Document();

            // This will be ignored.
            builder.Push(new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan("/Account/_ViewImports.cshtml", 0, 0, 0, 0),
            });
            builder.Add(new DirectiveTokenIRNode() { Content = "ignored" });
            builder.Pop();

            // This will be used.
            builder.Push(new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan("/Account/Manage/AddUser.cshtml", 0, 0, 0, 0),
            });
            builder.Add(new DirectiveTokenIRNode() { Content = "WebApplication.Account.Manage" });
            builder.Pop();

            var @namespace = new NamespaceDeclarationIRNode() { Content = "default" };
            builder.Push(@namespace);

            var @class = new ClassDeclarationIRNode() { Name = "default" };
            builder.Add(@class);

            var irDocument = (DocumentIRNode)builder.Build();
            irDocument.DocumentKind = RazorPageDocumentClassifierPass.RazorPageDocumentKind;

            var codeDocument = RazorCodeDocument.Create(RazorSourceDocument.Create("ignored", "/Account/Manage/AddUser.cshtml"));

            var pass = new NamespaceDirective.Pass();
            pass.Engine = RazorEngine.CreateEmpty(b => { });

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            Assert.Equal("WebApplication.Account.Manage", @namespace.Content);
            Assert.Equal("AddUser_Page", @class.Name);
        }

        // This is the case where the source file sets the namespace.
        [Fact]
        public void Pass_SetsNamespaceAndClassName_ComputedFromSource_ForView()
        {
            // Arrange
            var builder = RazorIRBuilder.Document();

            // This will be ignored.
            builder.Push(new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan("/Account/_ViewImports.cshtml", 0, 0, 0, 0),
            });
            builder.Add(new DirectiveTokenIRNode() { Content = "ignored" });
            builder.Pop();

            // This will be used.
            builder.Push(new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan("/Account/Manage/AddUser.cshtml", 0, 0, 0, 0),
            });
            builder.Add(new DirectiveTokenIRNode() { Content = "WebApplication.Account.Manage" });
            builder.Pop();

            var @namespace = new NamespaceDeclarationIRNode() { Content = "default" };
            builder.Push(@namespace);

            var @class = new ClassDeclarationIRNode() { Name = "default" };
            builder.Add(@class);

            var irDocument = (DocumentIRNode)builder.Build();
            irDocument.DocumentKind = MvcViewDocumentClassifierPass.MvcViewDocumentKind;

            var codeDocument = RazorCodeDocument.Create(RazorSourceDocument.Create("ignored", "/Account/Manage/AddUser.cshtml"));

            var pass = new NamespaceDirective.Pass();
            pass.Engine = RazorEngine.CreateEmpty(b => { });

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            Assert.Equal("WebApplication.Account.Manage", @namespace.Content);
            Assert.Equal("AddUser_View", @class.Name);
        }

        // This handles an error case where we can't determine the relationship between the
        // imports and the source.
        [Fact]
        public void Pass_SetsNamespaceButNotClassName_VerbatimFromImports()
        {
            // Arrange
            var builder = RazorIRBuilder.Document();

            builder.Push(new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan(null, 0, 0, 0, 0),
            });
            builder.Add(new DirectiveTokenIRNode() { Content = "WebApplication.Account" });
            builder.Pop();

            var @namespace = new NamespaceDeclarationIRNode() { Content = "default" };
            builder.Push(@namespace);

            var @class = new ClassDeclarationIRNode() { Name = "default" };
            builder.Add(@class);

            var irDocument = (DocumentIRNode)builder.Build();
            irDocument.DocumentKind = RazorPageDocumentClassifierPass.RazorPageDocumentKind;

            var codeDocument = RazorCodeDocument.Create(RazorSourceDocument.Create("ignored", "/Account/Manage/AddUser.cshtml"));

            var pass = new NamespaceDirective.Pass();
            pass.Engine = RazorEngine.CreateEmpty(b => { });

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            Assert.Equal("WebApplication.Account", @namespace.Content);
            Assert.Equal("default", @class.Name);
        }

        [Fact]
        public void Pass_DoesNothing_ForUnknownDocumentKind()
        {
            // Arrange
            var builder = RazorIRBuilder.Document();

            builder.Push(new DirectiveIRNode()
            {
                Descriptor = NamespaceDirective.Directive,
                Source = new SourceSpan(null, 0, 0, 0, 0),
            });
            builder.Add(new DirectiveTokenIRNode() { Content = "WebApplication.Account" });
            builder.Pop();

            var @namespace = new NamespaceDeclarationIRNode() { Content = "default" };
            builder.Push(@namespace);

            var @class = new ClassDeclarationIRNode() { Name = "default" };
            builder.Add(@class);

            var irDocument = (DocumentIRNode)builder.Build();
            irDocument.DocumentKind = null;

            var codeDocument = RazorCodeDocument.Create(RazorSourceDocument.Create("ignored", "/Account/Manage/AddUser.cshtml"));

            var pass = new NamespaceDirective.Pass();
            pass.Engine = RazorEngine.CreateEmpty(b => { });

            // Act
            pass.Execute(codeDocument, irDocument);

            // Assert
            Assert.Equal("default", @namespace.Content);
            Assert.Equal("default", @class.Name);
        }
    }
}
