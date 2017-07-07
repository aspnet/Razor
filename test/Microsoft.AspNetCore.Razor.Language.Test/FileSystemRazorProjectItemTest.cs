﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language
{
    public class FileSystemRazorProjectItemTest
    {
        private static string TestFolder { get; } = Path.Combine(
            TestProject.GetProjectDirectory(typeof(FileSystemRazorProjectItemTest)), 
            "TestFiles", 
            "FileSystemRazorProject");

        [Fact]
        public void FileSystemRazorProjectItem_SetsProperties()
        {
            // Arrange
            var fileInfo = new FileInfo(Path.Combine(TestFolder, "Home.cshtml"));

            // Act
            var projectItem = new FileSystemRazorProjectItem("/Views", "/Home.cshtml", fileInfo);

            // Assert
            Assert.Equal("/Home.cshtml", projectItem.FilePath);
            Assert.Equal("/Views", projectItem.BasePath);
            Assert.True(projectItem.Exists);
            Assert.Equal("Home.cshtml", projectItem.FileName);
            Assert.Equal(fileInfo.FullName, projectItem.PhysicalPath);
        }

        [Fact]
        public void Exists_ReturnsFalseWhenFileDoesNotExist()
        {
            // Arrange
            var fileInfo = new FileInfo(Path.Combine(TestFolder, "Views", "FileDoesNotExist.cshtml"));

            // Act
            var projectItem = new FileSystemRazorProjectItem("/Views", "/FileDoesNotExist.cshtml", fileInfo);

            // Assert
            Assert.False(projectItem.Exists);
        }

        [Fact]
        public void Read_ReturnsReadStream()
        {
            // Arrange
            var fileInfo = new FileInfo(Path.Combine(TestFolder, "Home.cshtml"));
            var projectItem = new FileSystemRazorProjectItem("/", "/Home.cshtml", fileInfo);

            // Act
            var stream = projectItem.Read();

            // Assert
            Assert.Equal("home-content", new StreamReader(stream).ReadToEnd());
        }
    }
}
