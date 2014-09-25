// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;
using Xunit;

namespace Microsoft.AspNet.Razor.Runtime.Test.TagHelpers
{
    public class TagHelperScopeManagerTest
    {
        [Fact]
        public void TagHelperScopeManager_BeginCreatesContextWithAppropriateTagName()
        {
            // Arrange
            var scopeManager = new TagHelperScopeManager();

            // Act
            var executionContext = scopeManager.Begin("p");

            // Assert
            Assert.Equal("p", executionContext.TagName);
        }

        [Fact]
        public void TagHelperScopeManager_BeginCanNest()
        {
            // Arrange
            var scopeManager = new TagHelperScopeManager();

            // Act
            var executionContext = scopeManager.Begin("p");
            executionContext = scopeManager.Begin("div");

            // Assert
            Assert.Equal("div", executionContext.TagName);
        }

        [Fact]
        public void TagHelperScopeManager_EndReturnsParentExecutionContext()
        {
            // Arrange
            var scopeManager = new TagHelperScopeManager();

            // Act
            var executionContext = scopeManager.Begin("p");
            executionContext = scopeManager.Begin("div");
            executionContext = scopeManager.End();

            // Assert
            Assert.Equal("p", executionContext.TagName);
        }

        [Fact]
        public void TagHelperScopeManager_EndReturnsNullIfNoNestedContext()
        {
            // Arrange
            var scopeManager = new TagHelperScopeManager();

            // Act
            var executionContext = scopeManager.Begin("p");
            executionContext = scopeManager.Begin("div");
            executionContext = scopeManager.End();
            executionContext = scopeManager.End();

            // Assert
            Assert.Null(executionContext);
        }

        [Fact]
        public void TagHelperScopeManager_EndThrowsIfNoScope()
        {
            // Arrange
            var scopeManager = new TagHelperScopeManager();
            var expectedError =
                "You cannot call 'End' without first calling 'Begin' on the " + nameof(TagHelperScopeManager) + ".";

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
            {
                scopeManager.End();
            });

            Assert.Equal(expectedError, ex.Message);

        }
    }
}