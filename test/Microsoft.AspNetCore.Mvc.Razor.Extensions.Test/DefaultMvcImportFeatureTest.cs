﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.AspNetCore.Razor.Language;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions
{
    public class DefaultMvcImportFeatureTest
    {
        [Fact]
        public void AddDefaultDirectivesImport_AddsSingleDynamicImport()
        {
            // Arrange
            var imports = new List<RazorSourceDocument>();

            // Act
            DefaultMvcImportFeature.AddDefaultDirectivesImport(imports);

            // Assert
            var import = Assert.Single(imports);
            Assert.Null(import.FilePath);
        }

        [Fact]
        public void AddHierarchicalImports_AddsViewImportSourceDocumentsOnDisk()
        {
            // Arrange
            var imports = new List<RazorSourceDocument>();
            var testFileSystem = new TestRazorProjectFileSystem(new[]
            {
                new TestRazorProjectItem("/Index.cshtml"),
                new TestRazorProjectItem("/_ViewImports.cshtml"),
                new TestRazorProjectItem("/Contact/_ViewImports.cshtml"),
                new TestRazorProjectItem("/Contact/Index.cshtml"),
            });
            var mvcImportFeature = new DefaultMvcImportFeature()
            {
                ProjectEngine = Mock.Of<RazorProjectEngine>(projectEngine => projectEngine.FileSystem == testFileSystem)
            };

            // Act
            mvcImportFeature.AddHierarchicalImports("/Contact/Index.cshtml", imports);

            // Assert
            Assert.Collection(imports,
                import => Assert.Equal("/_ViewImports.cshtml", import.FilePath),
                import => Assert.Equal("/Contact/_ViewImports.cshtml", import.FilePath));
        }

        [Fact]
        public void AddHierarchicalImports_AddsViewImportSourceDocumentsNotOnDisk()
        {
            // Arrange
            var imports = new List<RazorSourceDocument>();
            var testFileSystem = new TestRazorProjectFileSystem(new[]
            {
                new TestRazorProjectItem("/Pages/Contact/Index.cshtml"),
            });
            var mvcImportFeature = new DefaultMvcImportFeature()
            {
                ProjectEngine = Mock.Of<RazorProjectEngine>(projectEngine => projectEngine.FileSystem == testFileSystem)
            };

            // Act
            mvcImportFeature.AddHierarchicalImports("/Pages/Contact/Index.cshtml", imports);

            // Assert
            Assert.Collection(imports,
                import => Assert.Equal("/_ViewImports.cshtml", import.FilePath),
                import => Assert.Equal("/Pages/_ViewImports.cshtml", import.FilePath),
                import => Assert.Equal("/Pages/Contact/_ViewImports.cshtml", import.FilePath));
        }
    }
}
