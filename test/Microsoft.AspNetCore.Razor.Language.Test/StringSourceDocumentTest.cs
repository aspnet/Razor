﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language
{
    public class StringSourceDocumentTest
    {
        [Fact]
        public void GetChecksum_ReturnsCopiedChecksum()
        {
            // Arrange
            var content = "Hello World!";
            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var firstChecksum = document.GetChecksum();
            var secondChecksum = document.GetChecksum();

            // Assert
            Assert.Equal(firstChecksum, secondChecksum);
            Assert.NotSame(firstChecksum, secondChecksum);
        }

        [Fact]
        public void GetChecksum_ComputesCorrectChecksum_UTF8()
        {
            // Arrange
            var content = "Hello World!";
            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);
            var expectedChecksum = new byte[] { 46, 247, 189, 230, 8, 206, 84, 4, 233, 125, 95, 4, 47, 149, 248, 159, 28, 35, 40, 113 };

            // Act
            var checksum = document.GetChecksum();

            // Assert
            Assert.Equal(expectedChecksum, checksum);
        }

        [Fact]
        public void GetChecksum_ComputesCorrectChecksum_UTF32()
        {
            // Arrange
            var content = "Hello World!";
            var document = new StringSourceDocument(content, Encoding.UTF32, RazorSourceDocumentProperties.Default);
            var expectedChecksum = new byte[] { 8, 149, 159, 15, 242, 255, 115, 227, 219, 78, 61, 53, 127, 239, 77, 239, 215, 140, 248, 44 };

            // Act
            var checksum = document.GetChecksum();

            // Assert
            Assert.Equal(expectedChecksum, checksum);
        }

        [Fact]
        public void Indexer_ProvidesCharacterAccessToContent()
        {
            // Arrange
            var expectedContent = "Hello, World!";
            var indexerBuffer = new char[expectedContent.Length];
            var document = new StringSourceDocument(expectedContent, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            for (var i = 0; i < document.Length; i++)
            {
                indexerBuffer[i] = document[i];
            }

            // Assert
            var output = new string(indexerBuffer);
            Assert.Equal(expectedContent, output);
        }

        [Fact]
        public void Length()
        {
            // Arrange
            var expectedContent = "Hello, World!";
            var document = new StringSourceDocument(expectedContent, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act & Assert
            Assert.Equal(expectedContent.Length, document.Length);
        }

        [Fact]
        public void FilePath()
        {
            // Arrange
            var content = "Hello, World!";

            // Act
            var document = new StringSourceDocument(content, Encoding.UTF8, new RazorSourceDocumentProperties(filePath: "file.cshtml", relativePath: null));

            // Assert
            Assert.Equal("file.cshtml", document.FilePath);
        }

        [Fact]
        public void FilePath_Null()
        {
            // Arrange
            var content = "Hello, World!";

            // Act
            var document = new StringSourceDocument(content, Encoding.UTF8, new RazorSourceDocumentProperties(filePath: null, relativePath: null));

            // Assert
            Assert.Null(document.FilePath);
        }

        [Fact]
        public void RelativePath()
        {
            // Arrange
            var content = "Hello, World!";

            // Act
            var document = new StringSourceDocument(content, Encoding.UTF8, new RazorSourceDocumentProperties(filePath: null, relativePath: "file.cshtml"));

            // Assert
            Assert.Equal("file.cshtml", document.RelativePath);
        }

        [Fact]
        public void RelativePath_Null()
        {
            // Arrange
            var content = "Hello, World!";

            // Act
            var document = new StringSourceDocument(content, Encoding.UTF8, new RazorSourceDocumentProperties(filePath: null, relativePath: null));

            // Assert
            Assert.Null(document.RelativePath);
        }

        [Fact]
        public void CopyTo_PartialCopyFromStart()
        {
            // Arrange
            var content = "Hello, World!";
            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);
            var expectedContent = "Hello";
            var charBuffer = new char[expectedContent.Length];

            // Act
            document.CopyTo(0, charBuffer, 0, expectedContent.Length);

            // Assert
            var copiedContent = new string(charBuffer);
            Assert.Equal(expectedContent, copiedContent);
        }

        [Fact]
        public void CopyTo_PartialCopyDestinationOffset()
        {
            // Arrange
            var content = "Hello, World!";
            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);
            var expectedContent = "$Hello";
            var charBuffer = new char[expectedContent.Length];
            charBuffer[0] = '$';

            // Act
            document.CopyTo(0, charBuffer, 1, "Hello".Length);

            // Assert
            var copiedContent = new string(charBuffer);
            Assert.Equal(expectedContent, copiedContent);
        }

        [Fact]
        public void CopyTo_PartialCopySourceOffset()
        {
            // Arrange
            var content = "Hello, World!";
            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);
            var expectedContent = "World";
            var charBuffer = new char[expectedContent.Length];

            // Act
            document.CopyTo(7, charBuffer, 0, expectedContent.Length);

            // Assert
            var copiedContent = new string(charBuffer);
            Assert.Equal(expectedContent, copiedContent);
        }

        [Fact]
        public void CopyTo_CanCopyMultipleTimes()
        {
            // Arrange
            var content = "Hi";
            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act & Assert
            //
            // (we should be able to do this twice to prove that the underlying data isn't disposed)
            for (var i = 0; i < 2; i++)
            {
                var charBuffer = new char[2];
                document.CopyTo(0, charBuffer, 0, 2);
                var copiedContent = new string(charBuffer);
                Assert.Equal("Hi", copiedContent);
            }
        }

        [Fact]
        public void Lines_Count_EmptyDocument()
        {
            // Arrange
            var content = string.Empty;
            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = document.Lines.Count;

            // Assert
            Assert.Equal(1, actual);
        }

        [Fact]
        public void Lines_GetLineLength_EmptyDocument()
        {
            // Arrange
            var content = string.Empty;
            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = document.Lines.GetLineLength(0);

            // Assert
            Assert.Equal(0, actual);
        }

        [Fact]
        public void Lines_GetLineLength_TrailingNewlineDoesNotStartNewLine()
        {
            // Arrange
            var content = "hello\n";
            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = document.Lines.GetLineLength(0);

            // Assert
            Assert.Equal(6, actual);
        }

        [Fact]
        public void Lines_GetLineLength_TrailingNewlineDoesNotStartNewLine_CRLF()
        {
            // Arrange
            var content = "hello\r\n";
            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = document.Lines.GetLineLength(0);

            // Assert
            Assert.Equal(7, actual);
        }

        [Fact]
        public void Lines_Simple_Document()
        {
            // Arrange
            var content = new StringBuilder()
                .Append("The quick brown").Append('\n')
                .Append("fox").Append("\r\n")
                .Append("jumps over the lazy dog.")
                .ToString();

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = GetAllSourceMappings(document);

            // Assert
            Assert.Equal(new int[] { 16, 5, 24 }, actual);
        }

        [Fact]
        public void Lines_CRLF_OnlyCountsAsASingleNewLine()
        {
            // Arrange
            var content = "Hello\r\nWorld!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = GetAllSourceMappings(document);

            // Assert
            Assert.Equal(new int[] { 7, 6 }, actual);
        }

        [Fact]
        public void Lines_CR_IsNewLine()
        {
            // Arrange
            var content = "Hello\rWorld!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = GetAllSourceMappings(document);

            // Assert
            Assert.Equal(new int[] { 6, 6 }, actual);
        }

        // CR handling is stateful in the parser, making sure we properly reset the state.
        [Fact]
        public void Lines_CR_IsNewLine_MultipleCRs()
        {
            // Arrange
            var content = "Hello\rBig\r\nWorld!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = GetAllSourceMappings(document);

            // Assert
            Assert.Equal(new int[] { 6, 5, 6 }, actual);
        }

        [Fact]
        public void Lines_LF_IsNewLine()
        {
            // Arrange
            var content = "Hello\nWorld!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = GetAllSourceMappings(document);

            // Assert
            Assert.Equal(new int[] { 6, 6 }, actual);
        }

        [Fact]
        public void Lines_Unicode0085_IsNewLine()
        {
            // Arrange
            var content = "Hello\u0085World!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = GetAllSourceMappings(document);

            // Assert
            Assert.Equal(new int[] { 6, 6 }, actual);
        }

        [Fact]
        public void Lines_Unicode2028_IsNewLine()
        {
            // Arrange
            var content = "Hello\u2028World!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = GetAllSourceMappings(document);

            // Assert
            Assert.Equal(new int[] { 6, 6 }, actual);
        }

        [Fact]
        public void Lines_Unicode2029_IsNewLine()
        {
            // Arrange
            var content = "Hello\u2029World!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = GetAllSourceMappings(document);

            // Assert
            Assert.Equal(new int[] { 6, 6 }, actual);
        }

        [Fact]
        public void Lines_GetLocation_IncludesAbsoluteIndexAndDocument()
        {
            // Arrange
            var content = "Hello, World!";

            var document = new StringSourceDocument(content, Encoding.UTF8, new RazorSourceDocumentProperties(filePath: "Hi.cshtml", relativePath: null));

            // Act
            var actual = document.Lines.GetLocation(1);

            // Assert
            Assert.Equal("Hi.cshtml", actual.FilePath);
            Assert.Equal(1, actual.AbsoluteIndex);
        }

        [Fact]
        public void Lines_GetLocation_PrefersRelativePath()
        {
            // Arrange
            var content = "Hello, World!";

            var document = new StringSourceDocument(content, Encoding.UTF8, new RazorSourceDocumentProperties(filePath: "Hi.cshtml", relativePath: "Bye.cshtml"));

            // Act
            var actual = document.Lines.GetLocation(1);

            // Assert
            Assert.Equal("Bye.cshtml", actual.FilePath);
            Assert.Equal(1, actual.AbsoluteIndex);
        }

        // Beginnings of lines are special because the BinarySearch in the implementation
        // will succeed. It's a different code path.
        [Fact]
        public void Lines_GetLocation_FirstCharacter()
        {
            // Arrange
            var content = "Hello\nBig\r\nWorld!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = document.Lines.GetLocation(0);

            // Assert
            Assert.Equal(0, actual.LineIndex);
            Assert.Equal(0, actual.CharacterIndex);
        }

        [Fact]
        public void Lines_GetLocation_EndOfFirstLine()
        {
            // Arrange
            var content = "Hello\nBig\r\nWorld!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = document.Lines.GetLocation(5);

            // Assert
            Assert.Equal(0, actual.LineIndex);
            Assert.Equal(5, actual.CharacterIndex);
        }

        [Fact]
        public void Lines_GetLocation_InteriorLine()
        {
            // Arrange
            var content = "Hello\nBig\r\nWorld!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = document.Lines.GetLocation(7);

            // Assert
            Assert.Equal(1, actual.LineIndex);
            Assert.Equal(1, actual.CharacterIndex);
        }

        [Fact]
        public void Lines_GetLocation_StartOfLastLine()
        {
            // Arrange
            var content = "Hello\nBig\r\nWorld!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = document.Lines.GetLocation(11);

            // Assert
            Assert.Equal(2, actual.LineIndex);
            Assert.Equal(0, actual.CharacterIndex);
        }

        [Fact]
        public void Lines_GetLocation_EndOfLastLine()
        {
            // Arrange
            var content = "Hello\nBig\r\nWorld!";

            var document = new StringSourceDocument(content, Encoding.UTF8, RazorSourceDocumentProperties.Default);

            // Act
            var actual = document.Lines.GetLocation(16);

            // Assert
            Assert.Equal(2, actual.LineIndex);
            Assert.Equal(5, actual.CharacterIndex);
        }

        private static int[] GetAllSourceMappings(RazorSourceDocument source)
        {
            var lines = new int[source.Lines.Count];
            for (var i = 0; i < lines.Length; i++)
            {
                lines[i] = source.Lines.GetLineLength(i);
            }

            return lines;
        }
    }
}
