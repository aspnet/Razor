// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language
{
    public class DefaultImportFeatureTest
    {
        [Fact]
        public void GetImports_ReturnsEmptyImportsWhenZeroExpanders()
        {
            // Arrange
            var projectEngine = RazorProjectEngine.CreateEmpty(Mock.Of<RazorProjectFileSystem>(), builder => { });
            var importFeature = new DefaultImportFeature()
            {
                ProjectEngine = projectEngine,
            };

            // Act
            var imports = importFeature.GetImports("somepath");

            // Assert
            Assert.Empty(imports);
        }

        [Fact]
        public void GetImports_InvokesExpandersWithCorrectData()
        {
            // Arrange
            var sourceFilePath = "/Views/Index.cshtml";
            var expander1 = new Mock<IRazorImportExpander>();
            expander1.Setup(expander => expander.Populate(It.IsAny<ImportExpanderContext>()))
                .Callback<ImportExpanderContext>(context =>
                {
                    Assert.Equal(sourceFilePath, context.SourceFilePath);
                    Assert.Empty(context.Results);
                })
                .Verifiable();
            var projectEngine = RazorProjectEngine.CreateEmpty(
                Mock.Of<RazorProjectFileSystem>(),
                builder =>
                {
                    builder.Features.Add(expander1.Object);
                });
            var importFeature = new DefaultImportFeature()
            {
                ProjectEngine = projectEngine,
            };

            // Act
            importFeature.GetImports(sourceFilePath);

            // Assert
            expander1.Verify();
        }

        // This is more of an integration test that tests the end-to-end behavior of having expanders, 
        // them adding to the context and then returning the final import result.
        [Fact]
        public void GetImports_PopulatesImportsFromAllExpanders()
        {
            // Arrange
            var sourceDocument1 = TestRazorSourceDocument.Create();
            var expander1 = new Mock<IRazorImportExpander>();
            expander1.Setup(expander => expander.Populate(It.IsAny<ImportExpanderContext>()))
                .Callback<ImportExpanderContext>(context => context.Results.Add(sourceDocument1));
            var sourceDocument2 = TestRazorSourceDocument.Create();
            var expander2 = new Mock<IRazorImportExpander>();
            expander2.Setup(expander => expander.Populate(It.IsAny<ImportExpanderContext>()))
                .Callback<ImportExpanderContext>(context => context.Results.Add(sourceDocument2));
            var projectEngine = RazorProjectEngine.CreateEmpty(
                Mock.Of<RazorProjectFileSystem>(), 
                builder => 
                {
                    builder.Features.Add(expander1.Object);
                    builder.Features.Add(expander2.Object);
                });
            var importFeature = new DefaultImportFeature()
            {
                ProjectEngine = projectEngine,
            };

            // Act
            var imports = importFeature.GetImports("somepath");

            // Assert
            Assert.Collection(
                imports,
                import => Assert.Same(sourceDocument1, import),
                import => Assert.Same(sourceDocument2, import));
        }
    }
}
