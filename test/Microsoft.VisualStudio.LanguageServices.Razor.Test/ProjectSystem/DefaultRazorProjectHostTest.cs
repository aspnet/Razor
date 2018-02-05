// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServices.Razor;
using Microsoft.VisualStudio.ProjectSystem;
using Microsoft.VisualStudio.ProjectSystem.References;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    public class DefaultRazorProjectHostTest
    {
        public DefaultRazorProjectHostTest()
        {
            ForegroundDispatcher = new VisualStudioForegroundDispatcher();

            Workspace = new AdhocWorkspace();
            ProjectManager = new TestProjectSnapshotManager(ForegroundDispatcher, Workspace);
        }

        private VisualStudioForegroundDispatcher ForegroundDispatcher { get; }

        private TestProjectSnapshotManager ProjectManager { get; }

        private Workspace Workspace { get; }

        [ForegroundFact]
        public async Task DefaultRazorProjectHost_ForegroundThread_CreateAndDispose_Succeeds()
        {
            // Arrange
            var services = new TestProjectSystemServices("Test.csproj");
            var host = new DefaultRazorProjectHost(services, Workspace, ProjectManager);

            // Act & Assert
            await host.LoadAsync();
            Assert.Empty(ProjectManager.Projects);

            await host.DisposeAsync();
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task DefaultRazorProjectHost_BackgroundThread_CreateAndDispose_Succeeds()
        {
            // Arrange
            var services = new TestProjectSystemServices("Test.csproj");
            var host = new DefaultRazorProjectHost(services, Workspace, ProjectManager);

            // Act & Assert
            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            await Task.Run(async () => await host.DisposeAsync());
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task OnProjectChanged_ReadsProperties_InitializesProject()
        {
            // Arrange
            var changes = new TestProjectChangeDescription[]
            {
                new TestProjectChangeDescription()
                {
                    RuleName = RazorGeneral.SchemaName,
                    After = TestProjectRuleSnapshot.CreateProperties(RazorGeneral.SchemaName, new Dictionary<string, string>()
                    {
                        { RazorGeneral.RazorLangVersionProperty, "2.1" },
                    }),
                },
            };

            var services = new TestProjectSystemServices("Test.csproj");

            var host = new DefaultRazorProjectHost(services, Workspace, ProjectManager);

            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            // Act
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate(changes)));

            // Assert
            var snapshot = Assert.Single(ProjectManager.Projects);
            Assert.Equal("Test.csproj", snapshot.FilePath);
            Assert.Equal("2.1", snapshot.LanguageVersion);

            await Task.Run(async () => await host.DisposeAsync());
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task OnProjectChanged_NoVersionFound_DoesNotIniatializeProject()
        {
            // Arrange
            var changes = new TestProjectChangeDescription[]
            {
                new TestProjectChangeDescription()
                {
                    RuleName = RazorGeneral.SchemaName,
                    After = TestProjectRuleSnapshot.CreateProperties(RazorGeneral.SchemaName, new Dictionary<string, string>()
                    {
                        { RazorGeneral.RazorLangVersionProperty, "" },
                    }),
                },
            };

            var services = new TestProjectSystemServices("Test.csproj");

            var host = new DefaultRazorProjectHost(services, Workspace, ProjectManager);

            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            // Act
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate(changes)));

            // Assert
            Assert.Empty(ProjectManager.Projects);

            await Task.Run(async () => await host.DisposeAsync());
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task RazorProjectHost_UpdateProject_Succeeds()
        {
            // Arrange
            var changes = new TestProjectChangeDescription[]
            {
                new TestProjectChangeDescription()
                {
                    RuleName = RazorGeneral.SchemaName,
                    After = TestProjectRuleSnapshot.CreateProperties(RazorGeneral.SchemaName, new Dictionary<string, string>()
                    {
                        { RazorGeneral.RazorLangVersionProperty, "2.1" },
                    }),
                },
            };

            var services = new TestProjectSystemServices("Test.csproj");

            var host = new DefaultRazorProjectHost(services, Workspace, ProjectManager);

            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            // Act - 1
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate(changes)));

            // Assert - 1
            var snapshot = Assert.Single(ProjectManager.Projects);
            Assert.Equal("Test.csproj", snapshot.FilePath);
            Assert.Equal("2.1", snapshot.LanguageVersion);

            // Act - 2
            changes[0].After.SetProperty(RazorGeneral.RazorLangVersionProperty, "2.0");
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate(changes)));

            // Assert - 2
            snapshot = Assert.Single(ProjectManager.Projects);
            Assert.Equal("Test.csproj", snapshot.FilePath);
            Assert.Equal("2.0", snapshot.LanguageVersion);

            await Task.Run(async () => await host.DisposeAsync());
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task OnProjectChanged_VersionRemoved_DeinitializesProject()
        {
            // Arrange
            var changes = new TestProjectChangeDescription[]
            {
                new TestProjectChangeDescription()
                {
                    RuleName = RazorGeneral.SchemaName,
                    After = TestProjectRuleSnapshot.CreateProperties(RazorGeneral.SchemaName, new Dictionary<string, string>()
                    {
                        { RazorGeneral.RazorLangVersionProperty, "2.1" },
                    }),
                },
            };

            var services = new TestProjectSystemServices("Test.csproj");

            var host = new DefaultRazorProjectHost(services, Workspace, ProjectManager);

            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            // Act - 1
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate(changes)));

            // Assert - 1
            var snapshot = Assert.Single(ProjectManager.Projects);
            Assert.Equal("Test.csproj", snapshot.FilePath);
            Assert.Equal("2.1", snapshot.LanguageVersion);

            // Act - 2
            changes[0].After.SetProperty(RazorGeneral.RazorLangVersionProperty, "");
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate(changes)));

            // Assert - 2
            Assert.Empty(ProjectManager.Projects);

            await Task.Run(async () => await host.DisposeAsync());
            Assert.Empty(ProjectManager.Projects);
        }

        [ForegroundFact]
        public async Task RazorProjectHost_AfterDispose_IgnoresUpdate()
        {
            // Arrange
            var changes = new TestProjectChangeDescription[]
            {
                new TestProjectChangeDescription()
                {
                    RuleName = RazorGeneral.SchemaName,
                    After = TestProjectRuleSnapshot.CreateProperties(RazorGeneral.SchemaName, new Dictionary<string, string>()
                    {
                        { RazorGeneral.RazorLangVersionProperty, "2.1" },
                    }),
                },
            };

            var services = new TestProjectSystemServices("Test.csproj");

            var host = new DefaultRazorProjectHost(services, Workspace, ProjectManager);

            await Task.Run(async () => await host.LoadAsync());
            Assert.Empty(ProjectManager.Projects);

            // Act - 1
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate(changes)));

            // Assert - 1
            var snapshot = Assert.Single(ProjectManager.Projects);
            Assert.Equal("Test.csproj", snapshot.FilePath);
            Assert.Equal("2.1", snapshot.LanguageVersion);

            // Act - 2
            await Task.Run(async () => await host.DisposeAsync());

            // Assert - 2
            Assert.Empty(ProjectManager.Projects);

            // Act - 3
            changes[0].After.SetProperty(RazorGeneral.RazorLangVersionProperty, "2.0");
            await Task.Run(async () => await host.OnProjectChanged(services.CreateUpdate(changes)));

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
