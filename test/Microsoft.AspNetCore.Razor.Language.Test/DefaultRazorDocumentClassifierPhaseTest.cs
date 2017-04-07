﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Testing;
using Moq;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language
{
    public class DefaultRazorDocumentClassifierPhaseTest
    {
        [Fact]
        public void OnInitialized_OrdersPassesInAscendingOrder()
        {
            // Arrange & Act
            var phase = new DefaultRazorDocumentClassifierPhase();

            var first = Mock.Of<IRazorDocumentClassifierPass>(p => p.Order == 15);
            var second = Mock.Of<IRazorDocumentClassifierPass>(p => p.Order == 17);

            var engine = RazorEngine.CreateEmpty(b =>
            {
                b.Phases.Add(phase);

                b.Features.Add(second);
                b.Features.Add(first);
            });

            // Assert
            Assert.Collection(
                phase.Passes,
                p => Assert.Same(first, p),
                p => Assert.Same(second, p));
        }

        [Fact]
        public void Execute_ThrowsForMissingDependency()
        {
            // Arrange
            var phase = new DefaultRazorDocumentClassifierPhase();

            var engine = RazorEngine.CreateEmpty(b => b.Phases.Add(phase));

            var codeDocument = TestRazorCodeDocument.CreateEmpty();

            // Act & Assert
            ExceptionAssert.Throws<InvalidOperationException>(
                () => phase.Execute(codeDocument),
                $"The '{nameof(DefaultRazorDocumentClassifierPhase)}' phase requires a '{nameof(DocumentIRNode)}' " + 
                $"provided by the '{nameof(RazorCodeDocument)}'.");
        }

        [Fact]
        public void Execute_ExecutesPhasesInOrder()
        {
            // Arrange
            var codeDocument = TestRazorCodeDocument.CreateEmpty();

            // We're going to set up mocks to simulate a sequence of passes. We don't care about
            // what's in the nodes, we're just going to look at the identity via strict mocks.
            var originalNode = new DocumentIRNode();
            var firstPassNode = new DocumentIRNode();
            var secondPassNode = new DocumentIRNode();
            codeDocument.SetIRDocument(originalNode);

            var firstPass = new Mock<IRazorDocumentClassifierPass>(MockBehavior.Strict);
            firstPass.SetupGet(m => m.Order).Returns(0);
            firstPass.SetupProperty(m => m.Engine);
            firstPass.Setup(m => m.Execute(codeDocument, originalNode)).Callback(() =>
            {
                originalNode.Children.Add(firstPassNode);
            });

            var secondPass = new Mock<IRazorDocumentClassifierPass>(MockBehavior.Strict);
            secondPass.SetupGet(m => m.Order).Returns(1);
            secondPass.SetupProperty(m => m.Engine);
            secondPass.Setup(m => m.Execute(codeDocument, originalNode)).Callback(() =>
            {
                // Works only when the first pass has run before this.
                originalNode.Children[0].Children.Add(secondPassNode);
            });

            var phase = new DefaultRazorDocumentClassifierPhase();

            var engine = RazorEngine.CreateEmpty(b =>
            {
                b.Phases.Add(phase);

                b.Features.Add(firstPass.Object);
                b.Features.Add(secondPass.Object);
            });

            // Act
            phase.Execute(codeDocument);

            // Assert
            Assert.Same(secondPassNode, codeDocument.GetIRDocument().Children[0].Children[0]);
        }
    }
}
