// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Cascade.Contracts;
using Microsoft.VisualStudio.Text;
using Moq;
using Xunit;

namespace Microsoft.VisualStudio.LiveShare.Razor.Guest
{
    public class GuestWorkspaceProviderTest
    {
        [Fact]
        public void TryGetWorkspace_ReturnsFalseIfNotARemoteSession()
        {
            // Arrange
            var textBuffer = Mock.Of<ITextBuffer>();
            var workspaceManager = Mock.Of<IVsRemoteWorkspaceManager>(manager => manager.IsRemoteSession == false);
            var workspaceProvider = new GuestWorkspaceProvider(workspaceManager, Mock.Of<IRemoteLanguageServiceWorkspaceHost>());

            // Act
            var result = workspaceProvider.TryGetWorkspace(textBuffer, out var workspace);

            // Assert
            Assert.False(result);
            Assert.Null(workspace);
        }

        [Fact]
        public void TryGetWorkspace_ReturnsTrueIfRemoteSession()
        {
            // Arrange
            var textBuffer = Mock.Of<ITextBuffer>();
            var workspaceManager = Mock.Of<IVsRemoteWorkspaceManager>(manager => manager.IsRemoteSession == true);
            var expectedWorkspace = TestWorkspace.Create();
            var workspaceHost = Mock.Of<IRemoteLanguageServiceWorkspaceHost>(host => host.Workspace == expectedWorkspace);
            var workspaceProvider = new GuestWorkspaceProvider(workspaceManager, workspaceHost);

            // Act
            var result = workspaceProvider.TryGetWorkspace(textBuffer, out var workspace);

            // Assert
            Assert.True(result);
            Assert.Same(expectedWorkspace, workspace);
        }
    }
}
