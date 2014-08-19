// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor.Text;
using Xunit;

namespace Microsoft.AspNet.Razor.Generator.Compiler
{
    public class CodeWriterTest
    {
        private static readonly int NewLineLength = Environment.NewLine.Length;

        [Fact]
        public void CodeWriter_TracksPosition_WithWrite()
        {
            // Arrange
            var writer = new CodeWriter();

            // Act
            writer.Write("1234");

            // Assert
            var location = writer.GetCurrentSourceLocation();
            var expected = new SourceLocation(absoluteIndex: 4, lineIndex: 0, characterIndex: 4);

            Assert.Equal(expected, location);
        }

        [Fact]
        public void CodeWriter_TracksPosition_WithIndent()
        {
            // Arrange
            var writer = new CodeWriter();

            // Act
            writer.WriteLine();
            writer.Indent(size: 3);

            // Assert
            var location = writer.GetCurrentSourceLocation();
            var expected = new SourceLocation(absoluteIndex: 3 + NewLineLength, lineIndex: 1, characterIndex: 3);

            Assert.Equal(expected, location);
        }

        [Fact]
        public void CodeWriter_TracksPosition_WithWriteLine()
        {
            // Arrange
            var writer = new CodeWriter();

            // Act
            writer.WriteLine("1234");

            // Assert
            var location = writer.GetCurrentSourceLocation();

            var expected = new SourceLocation(absoluteIndex: 4 + NewLineLength, lineIndex: 1, characterIndex: 0);

            Assert.Equal(expected, location);
        }

        [Fact]
        public void CodeWriter_TracksPosition_WithWriteLine_WithNewLineInContent()
        {
            // Arrange
            var writer = new CodeWriter();

            // Act
            writer.WriteLine("1234" + Environment.NewLine + "12");

            // Assert
            var location = writer.GetCurrentSourceLocation();

            var expected = new SourceLocation(
                absoluteIndex: 6 + NewLineLength + NewLineLength, 
                lineIndex: 2, 
                characterIndex: 0);

            Assert.Equal(expected, location);
        }

        [Fact]
        public void CodeWriter_TracksPosition_WithWrite_WithNewlineInDataString()
        {
            // Arrange
            var writer = new CodeWriter();

            // Act
            writer.Write("1234" + Environment.NewLine + "123" + Environment.NewLine + "12");

            // Assert
            var location = writer.GetCurrentSourceLocation();

            var expected = new SourceLocation(
                absoluteIndex: 9 + NewLineLength + NewLineLength, 
                lineIndex: 2, 
                characterIndex: 2);

            Assert.Equal(expected, location);
        }

        [Fact]
        public void CodeWriter_TracksPosition_WithNewline_SplitAcrossWrites()
        {
            // This test is only relevant when the 'Environment.NewLine' value is multiple characters
            if (NewLineLength < 1)
            {
                return;
            }

            Assert.Equal(2, NewLineLength);

            // Arrange
            var writer = new CodeWriter();

            // Act
            writer.Write("1234" + Environment.NewLine[0]);
            var location1 = writer.GetCurrentSourceLocation();

            writer.Write(Environment.NewLine[1].ToString());
            var location2 = writer.GetCurrentSourceLocation();

            // Assert
            var expected1 = new SourceLocation(absoluteIndex: 5, lineIndex: 0, characterIndex: 5);
            Assert.Equal(expected1, location1);

            var expected2 = new SourceLocation(absoluteIndex: 6, lineIndex: 1, characterIndex: 0);
            Assert.Equal(expected2, location2);
        }

        [Fact]
        public void CodeWriter_TracksPosition_WithNewline_SplitAcrossWrites_AtBeginning()
        {
            // This test is only relevant when the 'Environment.NewLine' value is multiple characters
            if (NewLineLength < 1)
            {
                return;
            }

            Assert.Equal(2, NewLineLength);

            // Arrange
            var writer = new CodeWriter();

            // Act
            writer.Write(Environment.NewLine[0].ToString());
            var location1 = writer.GetCurrentSourceLocation();

            writer.Write(Environment.NewLine[1].ToString());
            var location2 = writer.GetCurrentSourceLocation();

            // Assert
            var expected1 = new SourceLocation(absoluteIndex: 1, lineIndex: 0, characterIndex: 1);
            Assert.Equal(expected1, location1);

            var expected2 = new SourceLocation(absoluteIndex: 2, lineIndex: 1, characterIndex: 0);
            Assert.Equal(expected2, location2);
        }
    }
}