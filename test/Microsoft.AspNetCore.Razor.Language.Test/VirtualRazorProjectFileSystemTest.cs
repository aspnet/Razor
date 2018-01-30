// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNetCore.Razor.Language
{
    public class VirtualRazorProjectFileSystemTest
    {
        [Fact]
        public void GetItem_ReturnsNotFound_IfFileDoesNotExistInRoot()
        {
            // Arrange
            var path = "/root-file.cshtml";
            var projectSystem = new VirtualRazorProjectFileSystem();

            // Act
            projectSystem.Add(new TestRazorProjectItem("/different-file.cshtml"));
            var result = projectSystem.GetItem(path);

            // Assert
            Assert.False(result.Exists);
        }

        [Fact]
        public void GetItem_ReturnsItemAddedToRoot()
        {
            // Arrange
            var path = "/root-file.cshtml";
            var projectSystem = new VirtualRazorProjectFileSystem();
            var projectItem = new TestRazorProjectItem(path);

            // Act
            projectSystem.Add(projectItem);
            var actual = projectSystem.GetItem(path);

            // Assert
            Assert.Same(projectItem, actual);
        }

        [Theory]
        [InlineData("/dir1/file.cshtml")]
        [InlineData("/dir1/dir2/file.cshtml")]
        [InlineData("/dir1/dir2/dir3/file.cshtml")]
        public void GetItem_ReturnsItemAddedToNestedDirectory(string path)
        {
            // Arrange
            var projectSystem = new VirtualRazorProjectFileSystem();
            var projectItem = new TestRazorProjectItem(path);

            // Act
            projectSystem.Add(projectItem);
            var actual = projectSystem.GetItem(path);

            // Assert
            Assert.Same(projectItem, actual);
        }

        [Fact]
        public void GetItem_ReturnsNotFound_WhenNestedDirectoryDoesNotExist()
        {
            // Arrange
            var projectSystem = new VirtualRazorProjectFileSystem();

            // Act
            var actual = projectSystem.GetItem("/dir1/dir3/file.cshtml");

            // Assert
            Assert.False(actual.Exists);
        }

        [Fact]
        public void GetItem_ReturnsNotFound_WhenNestedDirectoryDoesNotExist_AndPeerDirectoryExists()
        {
            // Arrange
            var projectSystem = new VirtualRazorProjectFileSystem();
            var projectItem = new TestRazorProjectItem("/dir1/dir2/file.cshtml");

            // Act
            projectSystem.Add(projectItem);
            var actual = projectSystem.GetItem("/dir1/dir3/file.cshtml");

            // Assert
            Assert.False(actual.Exists);
        }

        [Fact]
        public void GetItem_ReturnsNotFound_WhenFileDoesNotExistInNestedDirectory()
        {
            // Arrange
            var projectSystem = new VirtualRazorProjectFileSystem();
            var projectItem = new TestRazorProjectItem("/dir1/dir2/file.cshtml");

            // Act
            projectSystem.Add(projectItem);
            var actual = projectSystem.GetItem("/dir1/dir2/file2.cshtml");

            // Assert
            Assert.False(actual.Exists);
        }

        [Fact]
        public void EnumerateItems_AtRoot_ReturnsAllFiles()
        {
            // Arrange
            var projectSystem = new VirtualRazorProjectFileSystem();
            var file1 = new TestRazorProjectItem("/dir1/dir2/file1.cshtml");
            var file2 = new TestRazorProjectItem("/file2.cshtml");
            var file3 = new TestRazorProjectItem("/dir3/file3.cshtml");
            var file4 = new TestRazorProjectItem("/dir1/file4.cshtml");
            projectSystem.Add(file1);
            projectSystem.Add(file2);
            projectSystem.Add(file3);
            projectSystem.Add(file4);

            // Act
            var result = projectSystem.EnumerateItems("/");

            // Assert
            Assert.Equal(new[] { file2, file4, file1, file3 }, result);
        }

        [Fact]
        public void EnumerateItems_AtSubDirectory_ReturnsAllFilesUnderDirectoryHierarchy()
        {
            // Arrange
            var projectSystem = new VirtualRazorProjectFileSystem();
            var file1 = new TestRazorProjectItem("/dir1/dir2/file1.cshtml");
            var file2 = new TestRazorProjectItem("/file2.cshtml");
            var file3 = new TestRazorProjectItem("/dir3/file3.cshtml");
            var file4 = new TestRazorProjectItem("/dir1/file4.cshtml");
            projectSystem.Add(file1);
            projectSystem.Add(file2);
            projectSystem.Add(file3);
            projectSystem.Add(file4);

            // Act
            var result = projectSystem.EnumerateItems("/dir1");

            // Assert
            Assert.Equal(new[] { file4, file1 }, result);
        }

        [Fact]
        public void EnumerateItems_WithNoFilesInRoot_ReturnsEmptySequence()
        {
            // Arrange
            var projectSystem = new VirtualRazorProjectFileSystem();

            // Act
            var result = projectSystem.EnumerateItems("/");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void EnumerateItems_ForNonExistentDirectory_ReturnsEmptySequence()
        {
            // Arrange
            var projectSystem = new VirtualRazorProjectFileSystem();
            projectSystem.Add(new TestRazorProjectItem("/dir1/dir2/file1.cshtml"));
            projectSystem.Add(new TestRazorProjectItem("/file2.cshtml"));

            // Act
            var result = projectSystem.EnumerateItems("/dir3");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void GetHierarchicalItems_Works()
        {
            // Arrange
            var projectSystem = new VirtualRazorProjectFileSystem();
            var viewImport1 = new TestRazorProjectItem("/_ViewImports.cshtml");
            var viewImport2 = new TestRazorProjectItem("/Views/Home/_ViewImports.cshtml");
            projectSystem.Add(viewImport1);
            projectSystem.Add(viewImport2);

            // Act
            var items = projectSystem.FindHierarchicalItems("/", "/Views/Home/Index.cshtml", "_ViewImports.cshtml");

            // Assert
            Assert.Collection(
                items,
                item => Assert.Same(viewImport2, item),
                item => Assert.False(item.Exists),
                item => Assert.Same(viewImport1, item));
        }
    }
}
