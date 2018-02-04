// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServices.Razor;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    public class RazorProjectHostTest
    {
        public RazorProjectHostTest()
        {
            ForegroundDispatcher = new VisualStudioForegroundDispatcher();

            Workspace = new AdhocWorkspace();
            ProjectManager = new TestProjectSnapshotManager(ForegroundDispatcher, Workspace);
        }

        private VisualStudioForegroundDispatcher ForegroundDispatcher { get; }

        private TestProjectSnapshotManager ProjectManager { get; }

        private Workspace Workspace { get; }

        [ForegroundFact]
        public async Task RazorProjectHost_ForegroundThread_CreateAndDispose_Succeeds()
        {
            // Arrange
            var services = new TestProjectSystemServices("Test.csproj");
            var host = new RazorProjectHost(services, Workspace, ProjectManager);

            // Act & Assert
            await host.LoadAsync();
            Assert.Empty(ProjectManager.Projects);

            await host.DisposeAsync();
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task RazorProjectHost_BackgroundThread_CreateAndDispose_Succeeds()
        {
            // Arrange
            var services = new TestProjectSystemServices("Test.csproj");
            var host = new RazorProjectHost(services, Workspace, ProjectManager);

            // Act & Assert
            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            await Task.Run(async () => await host.DisposeAsync());
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task RazorProjectHost_IntializeProject_Succeeds()
        {
            // Arrange
            var razorLangVersion = new TestPropertyData()
            {
                Category = RazorGeneral.SchemaName,
                PropertyName = RazorGeneral.RazorLangVersionProperty,
                Value = "2.1",
                SetValues = new List<object>(),
            };

            var services = new TestProjectSystemServices("Test.csproj", razorLangVersion);

            var host = new RazorProjectHost(services, Workspace, ProjectManager);

            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            // Act
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate()));

            // Assert
            var snapshot = Assert.Single(ProjectManager.Projects);
            Assert.Equal("Test.csproj", snapshot.FilePath);
            Assert.Equal("2.1", snapshot.LanguageVersion);

            await Task.Run(async () => await host.DisposeAsync());
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task RazorProjectHost_IntializeProject_DefaultsToLatest()
        {
            // Arrange
            var razorLangVersion = new TestPropertyData()
            {
                Category = RazorGeneral.SchemaName,
                PropertyName = RazorGeneral.RazorLangVersionProperty,
                Value = "",
                SetValues = new List<object>(),
            };

            var services = new TestProjectSystemServices("Test.csproj", razorLangVersion);

            var host = new RazorProjectHost(services, Workspace, ProjectManager);

            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            // Act
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate()));

            // Assert
            var snapshot = Assert.Single(ProjectManager.Projects);
            Assert.Equal("Test.csproj", snapshot.FilePath);
            Assert.Equal("2.1", snapshot.LanguageVersion);

            await Task.Run(async () => await host.DisposeAsync());
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task RazorProjectHost_UpdateProject_Succeeds()
        {
            // Arrange
            var razorLangVersion = new TestPropertyData()
            {
                Category = RazorGeneral.SchemaName,
                PropertyName = RazorGeneral.RazorLangVersionProperty,
                Value = "2.1",
                SetValues = new List<object>(),
            };

            var services = new TestProjectSystemServices("Test.csproj", razorLangVersion);

            var host = new RazorProjectHost(services, Workspace, ProjectManager);

            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            // Act - 1
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate()));

            // Assert - 1
            var snapshot = Assert.Single(ProjectManager.Projects);
            Assert.Equal("Test.csproj", snapshot.FilePath);
            Assert.Equal("2.1", snapshot.LanguageVersion);

            // Act - 2
            razorLangVersion.Value = "2.0";
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate()));

            // Assert - 2
            snapshot = Assert.Single(ProjectManager.Projects);
            Assert.Equal("Test.csproj", snapshot.FilePath);
            Assert.Equal("2.0", snapshot.LanguageVersion);

            await Task.Run(async () => await host.DisposeAsync());
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task RazorProjectHost_AfterDispose_IgnoresUpdate()
        {
            // Arrange
            var razorLangVersion = new TestPropertyData()
            {
                Category = RazorGeneral.SchemaName,
                PropertyName = RazorGeneral.RazorLangVersionProperty,
                Value = "2.1",
                SetValues = new List<object>(),
            };

            var services = new TestProjectSystemServices("Test.csproj", razorLangVersion);

            var host = new RazorProjectHost(services, Workspace, ProjectManager);

            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            // Act - 1
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate()));

            // Assert - 1
            var snapshot = Assert.Single(ProjectManager.Projects);
            Assert.Equal("Test.csproj", snapshot.FilePath);
            Assert.Equal("2.1", snapshot.LanguageVersion);

            // Act - 2
            await Task.Run(async () => await host.DisposeAsync());

            // Assert - 2
            Assert.Empty(ProjectManager.Projects);

            // Act - 3
            razorLangVersion.Value = "2.0";
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate()));

            // Assert - 3
            Assert.Empty(ProjectManager.Projects);
        }

        private class TestProjectSnapshotManager : DefaultProjectSnapshotManager
        {
            public TestProjectSnapshotManager(ForegroundDispatcher dispatcher, Workspace workspace) 
                : base(dispatcher, Mock.Of<ErrorReporter>(), Mock.Of<ProjectSnapshotWorker>(), Array.Empty<ProjectSnapshotChangeTrigger>(), workspace)
            {
            }

            protected override void NotifyBackgroundWorker(ProjectSnapshotUpdateContext context)
            {
            }
        }
    }
}
