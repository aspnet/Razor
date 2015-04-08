// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.AspNet.Razor.Generator.Compiler.CSharp
{
    public class CSharpCodeWriterTest
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        public void WriteLineNumberDirective_UsesFilePath_WhenFileInSourceLocationIsNullOrEmpty(
            string sourceLocationFilePath)
        {
            // Arrange
            var filePath = "some-path";
            var writer = new CSharpCodeWriter();
            var expected = $"#line 5 \"{filePath}\"" + writer.NewLine;
            var sourceLocation = new SourceLocation(sourceLocationFilePath, 10, 4, 3);

            // Act
            writer.WriteLineNumberDirective(sourceLocation, filePath);
            var code = writer.GenerateCode();

            // Assert
            Assert.Equal(expected, code);
        }

        [Fact]
        public void WriteLineNumberDirectives_UsesSourceLocationFilePath_IfAvailable()
        {
            // Arrange
            var filePath = "some-path";
            var sourceLocationFilePath = "source-location-file-path";
            var writer = new CSharpCodeWriter();
            var expected = $"#line 5 \"{sourceLocationFilePath}\"" + writer.NewLine;
            var sourceLocation = new SourceLocation(sourceLocationFilePath, 10, 4, 3);

            // Act
            writer.WriteLineNumberDirective(sourceLocation, filePath);
            var code = writer.GenerateCode();

            // Assert
            Assert.Equal(expected, code);
        }
    }
}
