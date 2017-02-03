// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    public class RazorProjectTest
    {
        [Fact]
        public void EnumerateHierarchicalItems_ReturnsEmptySequenceIfPathIsAtRoot()
        {
            // Arrange
            var project = new TestRazorProject(new Dictionary<string, RazorProjectItem>());

            // Act
            var result = project.EnumerateHierarchicalItems("/", "File.cshtml");

            // Assert
            Assert.Empty(result);
        }

        [Theory]
        [InlineData("_ViewStart.cshtml")]
        [InlineData("_ViewImports.cshtml")]
        public void EnumerateHierarchicalItems_ReturnsItemsForPath(string fileName)
        {
            // Arrange
            var path = "/Views/Home/Index.cshtml";
            var items = new Dictionary<string, RazorProjectItem>
            {
                { $"/{fileName}", CreateProjectItem($"/{fileName}") },
                { $"/Views/{fileName}", CreateProjectItem($"/Views/{fileName}") },
                { $"/Views/Home/{fileName}", CreateProjectItem($"/Views/Home/{fileName}") },
            };
            var project = new TestRazorProject(items);

            // Act
            var result = project.EnumerateHierarchicalItems(path, $"{fileName}");

            // Assert
            Assert.Collection(
                result,
                item => Assert.Equal($"/Views/Home/{fileName}", item.Path),
                item => Assert.Equal($"/Views/{fileName}", item.Path),
                item => Assert.Equal($"/{fileName}", item.Path));
        }

        [Fact]
        public void EnumerateHierarchicalItems_ReturnsItemsForPathAtRoot()
        {
            // Arrange
            var path = "/Index.cshtml";
            var items = new Dictionary<string, RazorProjectItem>
            {
                { "/File.cshtml", CreateProjectItem("/File.cshtml") },
            };
            var project = new TestRazorProject(items);

            // Act
            var result = project.EnumerateHierarchicalItems(path, "File.cshtml");

            // Assert
            Assert.Collection(
                result,
                item => Assert.Equal("/File.cshtml", item.Path));
        }

        [Fact]
        public void EnumerateHierarchicalItems_DoesNotIncludePassedInItem()
        {
            // Arrange
            var path = "/Areas/MyArea/Views/Home/File.cshtml";
            var items = new Dictionary<string, RazorProjectItem>
            {
                { "/Areas/MyArea/Views/Home/File.cshtml", CreateProjectItem("/Areas/MyArea/Views/Home/File.cshtml") },
                { "/Areas/MyArea/Views/File.cshtml", CreateProjectItem("/Areas/MyArea/Views/File.cshtml") },
                { "/Areas/MyArea/File.cshtml", CreateProjectItem("/Areas/MyArea/File.cshtml") },
                { "/Areas/File.cshtml", CreateProjectItem("/Areas/File.cshtml") },
                { "/File.cshtml", CreateProjectItem("/File.cshtml") },
            };
            var project = new TestRazorProject(items);

            // Act
            var result = project.EnumerateHierarchicalItems(path, "File.cshtml");

            // Assert
            Assert.Collection(
                result,
                item => Assert.Equal("/Areas/MyArea/Views/File.cshtml", item.Path),
                item => Assert.Equal("/Areas/MyArea/File.cshtml", item.Path),
                item => Assert.Equal("/Areas/File.cshtml", item.Path),
                item => Assert.Equal("/File.cshtml", item.Path));
        }

        [Fact]
        public void EnumerateHierarchicalItems_ReturnsEmptySequenceIfPassedInItemWithFileNameIsAtRoot()
        {
            // Arrange
            var path = "/File.cshtml";
            var items = new Dictionary<string, RazorProjectItem>
            {
                { "/File.cshtml", CreateProjectItem("/File.cshtml") },
            };
            var project = new TestRazorProject(items);

            // Act
            var result = project.EnumerateHierarchicalItems(path, "File.cshtml");

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public void EnumerateHierarchicalItems_IncludesNonExistentFiles()
        {
            // Arrange
            var path = "/Areas/MyArea/Views/Home/Test.cshtml";
            var items = new Dictionary<string, RazorProjectItem>
            {
                { "/Areas/MyArea/File.cshtml", CreateProjectItem("/Areas/MyArea/File.cshtml") },
                { "/File.cshtml", CreateProjectItem("/File.cshtml") },
            };
            var project = new TestRazorProject(items);

            // Act
            var result = project.EnumerateHierarchicalItems(path, "File.cshtml");

            // Assert
            Assert.Collection(
                result,
                item =>
                {
                    Assert.Equal("/Areas/MyArea/Views/Home/File.cshtml", item.Path);
                    Assert.False(item.Exists);
                },
                item =>
                {
                    Assert.Equal("/Areas/MyArea/Views/File.cshtml", item.Path);
                    Assert.False(item.Exists);
                },
                item =>
                {
                    Assert.Equal("/Areas/MyArea/File.cshtml", item.Path);
                    Assert.True(item.Exists);
                },
                item =>
                {
                    Assert.Equal("/Areas/File.cshtml", item.Path);
                    Assert.False(item.Exists);
                },
                item =>
                {
                    Assert.Equal("/File.cshtml", item.Path);
                    Assert.True(item.Exists);
                });
        }

        private RazorProjectItem CreateProjectItem(string path)
        {
            var projectItem = new Mock<RazorProjectItem>();
            projectItem.SetupGet(f => f.Path).Returns(path);
            projectItem.SetupGet(f => f.Exists).Returns(true);
            return projectItem.Object;
        }

        private class TestRazorProject : RazorProject
        {
            private readonly Dictionary<string, RazorProjectItem> _items;

            public TestRazorProject(Dictionary<string, RazorProjectItem> items)
            {
                _items = items;
            }

            public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath) => throw new NotImplementedException();

            public override RazorProjectItem GetItem(string path)
            {
                if (!_items.TryGetValue(path, out var item))
                {
                    item = new NotFoundProjectItem("", path);
                }

                return item;
            }
        }
    }
}
