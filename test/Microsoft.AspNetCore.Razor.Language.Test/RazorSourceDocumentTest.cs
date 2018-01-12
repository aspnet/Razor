﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language
{
    public class RazorSourceDocumentTest
    {
        [Fact]
        public void ReadFrom()
        {
            // Arrange
            var content = TestRazorSourceDocument.CreateStreamContent();

            // Act
            var document = RazorSourceDocument.ReadFrom(content, "file.cshtml");

            // Assert
            Assert.IsType<StreamSourceDocument>(document);
            Assert.Equal("file.cshtml", document.FilePath);
            Assert.Same(Encoding.UTF8, document.Encoding);
        }

        [Fact]
        public void ReadFrom_WithEncoding()
        {
            // Arrange
            var content = TestRazorSourceDocument.CreateStreamContent(encoding: Encoding.UTF32);

            // Act
            var document = RazorSourceDocument.ReadFrom(content, "file.cshtml", Encoding.UTF32);

            // Assert
            Assert.Equal("file.cshtml", document.FilePath);
            Assert.Same(Encoding.UTF32, Assert.IsType<StreamSourceDocument>(document).Encoding);
        }

        [Fact]
        public void ReadFrom_WithProperties()
        {
            // Arrange
            var content = TestRazorSourceDocument.CreateStreamContent(encoding: Encoding.UTF32);
            var properties = new RazorSourceDocumentProperties("c:\\myapp\\filePath.cshtml", "filePath.cshtml");

            // Act
            var document = RazorSourceDocument.ReadFrom(content, Encoding.UTF32, properties);

            // Assert
            Assert.Equal("c:\\myapp\\filePath.cshtml", document.FilePath);
            Assert.Equal("filePath.cshtml", document.RelativePath);
            Assert.Same(Encoding.UTF32, Assert.IsType<StreamSourceDocument>(document).Encoding);
        }

        [Fact]
        public void ReadFrom_EmptyStream_WithEncoding()
        {
            // Arrange
            var content = TestRazorSourceDocument.CreateStreamContent(content: string.Empty, encoding: Encoding.UTF32);

            // Act
            var document = RazorSourceDocument.ReadFrom(content, "file.cshtml", Encoding.UTF32);

            // Assert
            Assert.Equal("file.cshtml", document.FilePath);
            Assert.Same(Encoding.UTF32, Assert.IsType<StreamSourceDocument>(document).Encoding);
        }

        [Fact]
        public void ReadFrom_ProjectItem()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem("filePath.cshtml", "c:\\myapp\\filePath.cshtml", "filePath.cshtml", "c:\\myapp\\");

            // Act
            var document = RazorSourceDocument.ReadFrom(projectItem);

            // Assert
            Assert.Equal("c:\\myapp\\filePath.cshtml", document.FilePath);
            Assert.Equal("filePath.cshtml", document.RelativePath);
            Assert.Equal(projectItem.Content, ReadContent(document));
        }

        [Fact]
        public void ReadFrom_ProjectItem_NoRelativePath()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem("filePath.cshtml", "c:\\myapp\\filePath.cshtml", basePath: "c:\\myapp\\");

            // Act
            var document = RazorSourceDocument.ReadFrom(projectItem);

            // Assert
            Assert.Equal("c:\\myapp\\filePath.cshtml", document.FilePath);
            Assert.Equal("filePath.cshtml", document.RelativePath);
            Assert.Equal(projectItem.Content, ReadContent(document));
        }

        [Fact]
        public void ReadFrom_ProjectItem_FallbackToRelativePath()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem("filePath.cshtml", relativePhysicalPath: "filePath.cshtml", basePath: "c:\\myapp\\");

            // Act
            var document = RazorSourceDocument.ReadFrom(projectItem);

            // Assert
            Assert.Equal("filePath.cshtml", document.FilePath);
            Assert.Equal("filePath.cshtml", document.RelativePath);
            Assert.Equal(projectItem.Content, ReadContent(document));
        }

        [Fact]
        public void ReadFrom_ProjectItem_FallbackToFileName()
        {
            // Arrange
            var projectItem = new TestRazorProjectItem("filePath.cshtml", basePath: "c:\\myapp\\");

            // Act
            var document = RazorSourceDocument.ReadFrom(projectItem);

            // Assert
            Assert.Equal("filePath.cshtml", document.FilePath);
            Assert.Equal("filePath.cshtml", document.RelativePath);
            Assert.Equal(projectItem.Content, ReadContent(document));
        }

        [Fact]
        public void Create_WithoutEncoding()
        {
            // Arrange
            var content = "Hello world";
            var fileName = "some-file-name";

            // Act
            var document = RazorSourceDocument.Create(content, fileName);

            // Assert
            Assert.Equal(fileName, document.FilePath);
            Assert.Equal(content, ReadContent(document));
            Assert.Same(Encoding.UTF8, document.Encoding);
        }

        [Fact]
        public void Create_WithEncoding()
        {
            // Arrange
            var content = "Hello world";
            var fileName = "some-file-name";
            var encoding = Encoding.UTF32;

            // Act
            var document = RazorSourceDocument.Create(content, fileName, encoding);

            // Assert
            Assert.Equal(fileName, document.FilePath);
            Assert.Equal(content, ReadContent(document));
            Assert.Same(encoding, document.Encoding);
        }

        [Fact]
        public void Create_WithProperties()
        {
            // Arrange
            var content = "Hello world";
            var properties = new RazorSourceDocumentProperties("c:\\myapp\\filePath.cshtml", "filePath.cshtml");

            // Act
            var document = RazorSourceDocument.Create(content, Encoding.UTF32, properties);

            // Assert
            Assert.Equal("c:\\myapp\\filePath.cshtml", document.FilePath);
            Assert.Equal("filePath.cshtml", document.RelativePath);
            Assert.Equal(content, ReadContent(document));
            Assert.Same(Encoding.UTF32, Assert.IsType<StringSourceDocument>(document).Encoding);
        }

        [Fact]
        public void ReadFrom_WithProjectItem_FallbackToFilePath_WhenRelativePhysicalPathIsNull()
        {
            // Arrange
            var filePath = "filePath.cshtml";
            var projectItem = new TestRazorProjectItem(filePath, relativePhysicalPath: null);

            // Act
            var document = RazorSourceDocument.ReadFrom(projectItem);

            // Assert
            Assert.Equal(filePath, document.FilePath);
            Assert.Equal(filePath, document.RelativePath);
        }

        [Fact]
        public void ReadFrom_WithProjectItem_UsesRelativePhysicalPath()
        {
            // Arrange
            var filePath = "filePath.cshtml";
            var relativePhysicalPath = "relative-path.cshtml";
            var projectItem = new TestRazorProjectItem(filePath, relativePhysicalPath: relativePhysicalPath);

            // Act
            var document = RazorSourceDocument.ReadFrom(projectItem);

            // Assert
            Assert.Equal(relativePhysicalPath, document.FilePath);
            Assert.Equal(relativePhysicalPath, document.RelativePath);
        }

        private static string ReadContent(RazorSourceDocument razorSourceDocument)
        {
            var buffer = new char[razorSourceDocument.Length];
            razorSourceDocument.CopyTo(0, buffer, 0, buffer.Length);

            return new string(buffer);
        }
    }
}
