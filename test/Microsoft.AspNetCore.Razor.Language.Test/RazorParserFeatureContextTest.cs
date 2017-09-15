// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language
{
    public class RazorParserFeatureContextTest
    {
        [Fact]
        public void Create_NullVersion_Throws()
        {
            // Arrange, Act & Assert
            Assert.Throws<ArgumentNullException>(() => RazorParserFeatureContext.Create(version: null));
        }

        [Fact]
        public void Create_LatestVersion_AllowsMinimizedBooleanTagHelperAttributes()
        {
            // Arrange
            var version = new Version(2, 1, 0);

            // Act
            var context = RazorParserFeatureContext.Create(version);

            // Assert
            Assert.True(context.AllowMinimizedBooleanTagHelperAttributes);
        }

        [Fact]
        public void Create_UnknownVersion_DoesNotAllowMinimizedBooleanTagHelperAttributes()
        {
            // Arrange
            var version = new Version(2, 0, 0);

            // Act
            var context = RazorParserFeatureContext.Create(version);

            // Assert
            Assert.False(context.AllowMinimizedBooleanTagHelperAttributes);
        }
    }
}
