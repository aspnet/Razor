// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.ObjectModel;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Moq;
using Xunit;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    public class DefaultTextViewRazorDocumentTrackerServiceTest
    {
        [Fact]
        public void CreateTracker_CreatesNewTracker_WhenDocumentNotTracked()
        {
            // Arrange
            var service = CreateTrackerService();

            var textView = CreateTextView();

            // Act
            var tracker = service.CreateTracker(textView);

            // Assert
            Assert.NotNull(tracker);
            Assert.Same(tracker, textView.Properties.GetProperty(typeof(RazorDocumentTracker)));
        }

        [Fact]
        public void CreateTracker_ReturnsExistingTracker_WhenDocumentIsAlreadyTracked()
        {
            // Arrange
            var service = CreateTrackerService();

            var textView = CreateTextView();

            var expected = new DefaultTextViewRazorDocumentTracker(service, textView);
            textView.Properties.AddProperty(typeof(RazorDocumentTracker), expected);

            // Act
            var tracker = service.CreateTracker(textView);

            // Assert
            Assert.NotNull(tracker);
            Assert.Same(tracker, expected);
        }

        [Fact]
        public void SubjectBuffersConnected_CreatesAndReturnsTracker()
        {
            // Arrange
            var service = CreateTrackerService();

            var textView = CreateTextView();

            // Act
            service.SubjectBuffersConnected(textView, ConnectionReason.BufferGraphChange, new Collection<ITextBuffer>());

            // Assert
            Assert.NotNull(textView.Properties.GetProperty(typeof(RazorDocumentTracker)));
        }

        private IWpfTextView CreateTextView()
        {
            var properties = new PropertyCollection();
            var textView = Mock.Of<IWpfTextView>(t => t.Properties == properties);
            return textView;
        }

        private DefaultTextViewRazorDocumentTrackerService CreateTrackerService()
        {
            var services = Mock.Of<IServiceProvider>();
            var documentFactory = Mock.Of<ITextDocumentFactoryService>();
            var workspace = Mock.Of<VisualStudioWorkspace>();
            return new DefaultTextViewRazorDocumentTrackerService(services, documentFactory, workspace);
        }
    }
}
