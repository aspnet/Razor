// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Testing;
using Xunit;

namespace Microsoft.AspNet.Razor
{
    public class SourceLocationTest
    {
        [Fact]
        public void ConstructorWithLineAndCharacterIndexSetsAssociatedProperties()
        {
            // Act
            var loc = new SourceLocation(0, 42, 24);

            // Assert
            Assert.Null(loc.FilePath);
            Assert.Equal(0, loc.AbsoluteIndex);
            Assert.Equal(42, loc.LineIndex);
            Assert.Equal(24, loc.CharacterIndex);
        }

        [Fact]
        public void Constructor_SetsFilePathAndIndexes()
        {
            // Arrange
            var filePath = "some-file-path";
            var absoluteIndex = 133;
            var lineIndex = 23;
            var characterIndex = 12;

            // Act
            var sourceLocation = new SourceLocation(filePath, absoluteIndex, lineIndex, characterIndex);

            // Assert
            Assert.Equal(filePath, sourceLocation.FilePath);
            Assert.Equal(absoluteIndex, sourceLocation.AbsoluteIndex);
            Assert.Equal(lineIndex, sourceLocation.LineIndex);
            Assert.Equal(characterIndex, sourceLocation.CharacterIndex);
        }

        [Fact]
        public void GetHashCode_ReturnsHashCode_UsingAbsoluteIndex()
        {
            // Arrange
            var sourceLocationA = new SourceLocation(10, 3, 4);
            var sourceLocationB = new SourceLocation(10, 45, 8754);

            // Act
            var hashCodeA = sourceLocationA.GetHashCode();
            var hashCodeB = sourceLocationB.GetHashCode();

            // Assert
            Assert.Equal(hashCodeA, hashCodeB);
        }

        [Fact]
        public void GetHashCode_ReturnsHashCode_UsingFilePathAndAbsoluteIndex_WhenFilePathIsNonNull()
        {
            // Arrange
            var sourceLocationA = new SourceLocation("some-path", 3, 53, 94);
            var sourceLocationB = new SourceLocation("some-path", 3, 37, 46);

            // Act
            var hashCodeA = sourceLocationA.GetHashCode();
            var hashCodeB = sourceLocationB.GetHashCode();

            // Assert
            Assert.Equal(hashCodeA, hashCodeB);
        }

        [Fact]
        public void Equal_ReturnsFalse_IfIndexesDiffer()
        {
            // Arrange
            var sourceLocationA = new SourceLocation(10, 3, 4);
            var sourceLocationB = new SourceLocation(10, 45, 8754);

            // Act
            var result = sourceLocationA.Equals(sourceLocationB);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equal_ReturnsFalse_IfFilePathIsDifferent()
        {
            // Arrange
            var sourceLocationA = new SourceLocation(10, 3, 4);
            var sourceLocationB = new SourceLocation("different-file", 10, 3, 4);

            // Act
            var result = sourceLocationA.Equals(sourceLocationB);

            // Assert
            Assert.False(result);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("some-file")]
        public void Equal_ReturnsTrue_IfFilePathAndIndexesAreSame(string path)
        {
            // Arrange
            var sourceLocationA = new SourceLocation(path, 10, 3, 4);
            var sourceLocationB = new SourceLocation(path, 10, 3, 4);
            var sourceLocationC = new SourceLocation("different-path", 10, 3, 4);

            // Act
            var result1 = sourceLocationA.Equals(sourceLocationB);
            var result2 = sourceLocationA.Equals(sourceLocationC);

            // Assert
            Assert.True(result1);
            Assert.False(result2);
        }

        [Fact]
        public void CompareTo_ReturnsResultOfFilePathComparisons_IfSourceLocationsDoNotMatch()
        {
            // Arrange
            var sourceLocationA = new SourceLocation("a-path", 1, 1, 1);
            var sourceLocationB = new SourceLocation("b-path", 1, 1, 1);

            // Act
            var result = sourceLocationA.CompareTo(sourceLocationB);

            // Assert
            Assert.Equal(string.Compare(sourceLocationA.FilePath, sourceLocationB.FilePath, StringComparison.Ordinal),
                result);
        }

        [Theory]
        [InlineData(null, 1, 2)]
        [InlineData(null, 32, 32)]
        [InlineData("same-path", 34, 32)]
        [InlineData("same-path-b", 18, 32)]
        public void CompareTo_ReturnsResultOfAbsoluteIndexComparisons_IfSourceLocationsMatch(
            string path, int indexA, int indexB)
        {
            // Arrange
            var sourceLocationA = new SourceLocation(path, indexA, 1, 1);
            var sourceLocationB = new SourceLocation(path, indexB, 1, 1);

            // Act
            var result = sourceLocationA.CompareTo(sourceLocationB);

            // Assert
            Assert.Equal(indexA.CompareTo(indexB), result);
        }

        [Fact]
        public void Add_Throws_IfSourceLocationsDoNotMatch()
        {
            // Arrange
            var sourceLocationA = new SourceLocation("a-path", 1, 1, 1);
            var sourceLocationB = new SourceLocation("b-path", 1, 1, 1);

            // Act and Assert
            ExceptionAssert.ThrowsArgument(
                () => { var result = sourceLocationA + sourceLocationB; },
                "right",
                $"Cannot perform operations on 'SourceLocation' instances with different file paths.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("same-path")]
        public void Add_IgnoresCharacterIndexIfLineIndexIsNonZero(string path)
        {
            // Arrange
            var sourceLocationA = new SourceLocation(path, 1, 2, 3);
            var sourceLocationB = new SourceLocation(path, 4, 5, 6);

            // Act
            var result = sourceLocationA + sourceLocationB;

            // Assert
            Assert.Equal(path, result.FilePath);
            Assert.Equal(5, result.AbsoluteIndex);
            Assert.Equal(7, result.LineIndex);
            Assert.Equal(6, result.CharacterIndex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("same-path")]
        public void Add_UsesCharacterIndexIfRightLineIndexIsZero(string path)
        {
            // Arrange
            var sourceLocationA = new SourceLocation(path, 2, 5, 3);
            var sourceLocationB = new SourceLocation(path, 4, 0, 6);

            // Act
            var result = sourceLocationA + sourceLocationB;

            // Assert
            Assert.Equal(path, result.FilePath);
            Assert.Equal(6, result.AbsoluteIndex);
            Assert.Equal(5, result.LineIndex);
            Assert.Equal(9, result.CharacterIndex);
        }

        [Fact]
        public void Subtract_Throws_IfSourceLocationsDoNotMatch()
        {
            // Arrange
            var sourceLocationA = new SourceLocation("a-path", 1, 1, 1);
            var sourceLocationB = new SourceLocation("b-path", 1, 1, 1);

            // Act and Assert
            ExceptionAssert.ThrowsArgument(
                () => { var result = sourceLocationA - sourceLocationB; },
                "right",
                "Cannot perform operations on 'SourceLocation' instances with different file paths.");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("same-path")]
        public void Subtract_UsesDifferenceOfCharacterIndexesIfLineIndexesAreSame(string path)
        {
            // Arrange
            var sourceLocationA = new SourceLocation(path, 1, 5, 3);
            var sourceLocationB = new SourceLocation(path, 5, 5, 6);

            // Act
            var result = sourceLocationB - sourceLocationA;

            // Assert
            Assert.Equal(path, result.FilePath);
            Assert.Equal(4, result.AbsoluteIndex);
            Assert.Equal(0, result.LineIndex);
            Assert.Equal(3, result.CharacterIndex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("same-path")]
        public void Subtract_UsesLeftCharacterIndexIfLineIndexesAreDifferent(string path)
        {
            // Arrange
            var sourceLocationA = new SourceLocation(path, 2, 0, 3);
            var sourceLocationB = new SourceLocation(path, 4, 5, 6);

            // Act
            var result = sourceLocationB - sourceLocationA;

            // Assert
            Assert.Equal(path, result.FilePath);
            Assert.Equal(2, result.AbsoluteIndex);
            Assert.Equal(5, result.LineIndex);
            Assert.Equal(6, result.CharacterIndex);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("path-to-file")]
        public void Advance_PreservesSourceLocationFilePath(string path)
        {
            // Arrange
            var sourceLocation = new SourceLocation(path, 15, 2, 8);

            // Act
            var result = SourceLocation.Advance(sourceLocation, "Hello world");

            // Assert
            Assert.Equal(path, result.FilePath);
            Assert.Equal(26, result.AbsoluteIndex);
            Assert.Equal(2, result.LineIndex);
            Assert.Equal(19, result.CharacterIndex);
        }
    }
}
