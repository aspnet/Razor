﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Testing;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public class DefaultRazorIntermediateNodeBuilderTest
    {
        [Fact]
        public void Ctor_CreatesEmptyBuilder()
        {
            // Arrange & Act
            var builder = new DefaultRazorIntermediateNodeBuilder();
            var current = builder.Current;

            // Assert
            Assert.Null(current);
        }

        [Fact]
        public void Push_WhenEmpty_AddsNode()
        {
            // Arrange
            var builder = new DefaultRazorIntermediateNodeBuilder();
            var node = new BasicIntermediateNode();

            // Act
            builder.Push(node);

            // Assert
            Assert.Same(node, builder.Current);
        }

        [Fact]
        public void Push_WhenNonEmpty_SetsUpChild()
        {
            // Arrange
            var builder = new DefaultRazorIntermediateNodeBuilder();

            var parent = new BasicIntermediateNode();
            builder.Push(parent);

            var node = new BasicIntermediateNode();

            // Act
            builder.Push(node);

            // Assert
            Assert.Same(node, builder.Current);
            Assert.Collection(parent.Children, n => Assert.Same(node, n));
        }

        [Fact]
        public void Pop_ThrowsWhenEmpty()
        {
            // Arrange
            var builder = new DefaultRazorIntermediateNodeBuilder();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => builder.Pop(),
                "The 'Pop' operation is not valid when the builder is empty.");
        }

        [Fact]
        public void Pop_SingleNodeDepth_RemovesAndReturnsNode()
        {
            // Arrange
            var builder = new DefaultRazorIntermediateNodeBuilder();

            var node = new BasicIntermediateNode();
            builder.Push(node);

            // Act
            var result = builder.Pop();

            // Assert
            Assert.Same(node, result);
            Assert.Null(builder.Current);
        }

        [Fact]
        public void Pop_MultipleNodeDepth_RemovesAndReturnsNode()
        {
            // Arrange
            var builder = new DefaultRazorIntermediateNodeBuilder();

            var parent = new BasicIntermediateNode();
            builder.Push(parent);

            var node = new BasicIntermediateNode();
            builder.Push(node);

            // Act
            var result = builder.Pop();

            // Assert
            Assert.Same(node, result);
            Assert.Same(parent, builder.Current);
        }

        [Fact]
        public void Add_AddsToChildrenAndSetsParent()
        {
            // Arrange
            var builder = new DefaultRazorIntermediateNodeBuilder();

            var parent = new BasicIntermediateNode();
            builder.Push(parent);

            var node = new BasicIntermediateNode();

            // Act
            builder.Add(node);

            // Assert
            Assert.Same(parent, builder.Current);
            Assert.Collection(parent.Children, n => Assert.Same(node, n));
        }

        [Fact]
        public void Insert_AddsToChildren_EmptyCollection()
        {
            // Arrange
            var builder = new DefaultRazorIntermediateNodeBuilder();

            var parent = new BasicIntermediateNode();
            builder.Push(parent);

            var node = new BasicIntermediateNode();

            // Act
            builder.Insert(0, node);

            // Assert
            Assert.Same(parent, builder.Current);
            Assert.Collection(parent.Children, n => Assert.Same(node, n));
        }

        [Fact]
        public void Insert_AddsToChildren_NonEmpyCollection()
        {
            // Arrange
            var builder = new DefaultRazorIntermediateNodeBuilder();

            var parent = new BasicIntermediateNode();
            builder.Push(parent);

            var child = new BasicIntermediateNode();
            builder.Add(child);

            var node = new BasicIntermediateNode();

            // Act
            builder.Insert(0, node);

            // Assert
            Assert.Same(parent, builder.Current);
            Assert.Collection(parent.Children, n => Assert.Same(node, n), n => Assert.Same(child, n));
        }

        [Fact]
        public void Insert_AddsToChildren_NonEmpyCollection_AtEnd()
        {
            // Arrange
            var builder = new DefaultRazorIntermediateNodeBuilder();

            var parent = new BasicIntermediateNode();
            builder.Push(parent);

            var child = new BasicIntermediateNode();
            builder.Add(child);

            var node = new BasicIntermediateNode();

            // Act
            builder.Insert(1, node);

            // Assert
            Assert.Same(parent, builder.Current);
            Assert.Collection(parent.Children, n => Assert.Same(child, n), n => Assert.Same(node, n));
        }

        [Fact]
        public void Build_PopsMultipleLevels()
        {
            // Arrange
            var builder = new DefaultRazorIntermediateNodeBuilder();

            var document = new DocumentIntermediateNode();
            builder.Push(document);

            var node = new BasicIntermediateNode();
            builder.Push(node);

            // Act
            var result = builder.Build();

            // Assert
            Assert.Same(document, result);
            Assert.Null(builder.Current);
        }

        private class BasicIntermediateNode : IntermediateNode
        {
            public override IntermediateNodeCollection Children { get; } = new IntermediateNodeCollection();

            public override void Accept(IntermediateNodeVisitor visitor)
            {
                throw new NotImplementedException();
            }
        }
    }
}
