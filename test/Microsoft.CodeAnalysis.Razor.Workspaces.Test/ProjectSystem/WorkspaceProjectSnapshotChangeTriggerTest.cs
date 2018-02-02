﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    public class WorkspaceProjectSnapshotChangeTriggerTest
    {
        public WorkspaceProjectSnapshotChangeTriggerTest()
        {
            Workspace = new AdhocWorkspace();
            EmptySolution = Workspace.CurrentSolution.GetIsolatedSolution();

            ProjectNumberOne = Workspace.CurrentSolution.AddProject("One", "One", LanguageNames.CSharp);
            ProjectNumberTwo = ProjectNumberOne.Solution.AddProject("Two", "Two", LanguageNames.CSharp);
            SolutionWithTwoProjects = ProjectNumberTwo.Solution;

            ProjectNumberThree = EmptySolution.GetIsolatedSolution().AddProject("Three", "Three", LanguageNames.CSharp);
            SolutionWithOneProject = ProjectNumberThree.Solution;
        }

        private Solution EmptySolution { get; }

        private Solution SolutionWithOneProject { get; }

        private Solution SolutionWithTwoProjects { get; }

        private Project ProjectNumberOne { get; }

        private Project ProjectNumberTwo { get; }

        private Project ProjectNumberThree { get; }

        private Workspace Workspace { get; }

        [Theory]
        [InlineData(WorkspaceChangeKind.SolutionAdded)]
        [InlineData(WorkspaceChangeKind.SolutionChanged)]
        [InlineData(WorkspaceChangeKind.SolutionCleared)]
        [InlineData(WorkspaceChangeKind.SolutionReloaded)]
        [InlineData(WorkspaceChangeKind.SolutionRemoved)]
        public void WorkspaceChanged_SolutionEvents_AddsProjectsInSolution(WorkspaceChangeKind kind)
        {
            // Arrange
            var trigger = new WorkspaceProjectSnapshotChangeTrigger();
            var projectManager = new TestProjectSnapshotManager(new[] { trigger }, Workspace);
            
            var e = new WorkspaceChangeEventArgs(kind, oldSolution: EmptySolution, newSolution: SolutionWithTwoProjects);

            // Act
            trigger.Workspace_WorkspaceChanged(Workspace, e);

            // Assert
            Assert.Collection(
                projectManager.Projects.OrderBy(p => p.WorkspaceProject.Name),
                p => Assert.Equal(ProjectNumberOne.Id, p.WorkspaceProject.Id),
                p => Assert.Equal(ProjectNumberTwo.Id, p.WorkspaceProject.Id));
        }

        [Theory]
        [InlineData(WorkspaceChangeKind.SolutionAdded)]
        [InlineData(WorkspaceChangeKind.SolutionChanged)]
        [InlineData(WorkspaceChangeKind.SolutionCleared)]
        [InlineData(WorkspaceChangeKind.SolutionReloaded)]
        [InlineData(WorkspaceChangeKind.SolutionRemoved)]
        public void WorkspaceChanged_SolutionEvents_ClearsExistingProjects_AddsProjectsInSolution(WorkspaceChangeKind kind)
        {
            // Arrange
            var trigger = new WorkspaceProjectSnapshotChangeTrigger();
            var projectManager = new TestProjectSnapshotManager(new[] { trigger }, Workspace);

            // Initialize with a project. This will get removed.
            var e = new WorkspaceChangeEventArgs(WorkspaceChangeKind.SolutionAdded, oldSolution: EmptySolution, newSolution: SolutionWithOneProject);
            trigger.Workspace_WorkspaceChanged(Workspace, e);

            e = new WorkspaceChangeEventArgs(kind, oldSolution: EmptySolution, newSolution: SolutionWithTwoProjects);

            // Act
            trigger.Workspace_WorkspaceChanged(Workspace, e);

            // Assert
            Assert.Collection(
                projectManager.Projects.OrderBy(p => p.WorkspaceProject.Name),
                p => Assert.Equal(ProjectNumberOne.Id, p.WorkspaceProject.Id),
                p => Assert.Equal(ProjectNumberTwo.Id, p.WorkspaceProject.Id));
        }

        [Theory]
        [InlineData(WorkspaceChangeKind.ProjectChanged)]
        [InlineData(WorkspaceChangeKind.ProjectReloaded)]
        public void WorkspaceChanged_ProjectChangeEvents_UpdatesProject(WorkspaceChangeKind kind)
        {
            // Arrange
            var trigger = new WorkspaceProjectSnapshotChangeTrigger();
            var projectManager = new TestProjectSnapshotManager(new[] { trigger }, Workspace);

            // Initialize with some projects.
            var e = new WorkspaceChangeEventArgs(WorkspaceChangeKind.SolutionAdded, oldSolution: EmptySolution, newSolution: SolutionWithTwoProjects);
            trigger.Workspace_WorkspaceChanged(Workspace, e);

            var solution = SolutionWithTwoProjects.WithProjectAssemblyName(ProjectNumberOne.Id, "Changed");
            e = new WorkspaceChangeEventArgs(kind, oldSolution: SolutionWithTwoProjects, newSolution: solution, projectId: ProjectNumberOne.Id);

            // Act
            trigger.Workspace_WorkspaceChanged(Workspace, e);

            // Assert
            Assert.Collection(
                projectManager.Projects.OrderBy(p => p.WorkspaceProject.Name),
                p =>
                {
                    Assert.Equal(ProjectNumberOne.Id, p.WorkspaceProject.Id);
                    Assert.Equal("Changed", p.WorkspaceProject.AssemblyName);
                },
                p => Assert.Equal(ProjectNumberTwo.Id, p.WorkspaceProject.Id));
        }

        [Fact]
        public void WorkspaceChanged_ProjectRemovedEvent_RemovesProject()
        {
            // Arrange
            var trigger = new WorkspaceProjectSnapshotChangeTrigger();
            var projectManager = new TestProjectSnapshotManager(new[] { trigger }, Workspace);

            // Initialize with some projects project.
            var e = new WorkspaceChangeEventArgs(WorkspaceChangeKind.SolutionAdded, oldSolution: EmptySolution, newSolution: SolutionWithTwoProjects);
            trigger.Workspace_WorkspaceChanged(Workspace, e);

            var solution = SolutionWithTwoProjects.RemoveProject(ProjectNumberOne.Id);
            e = new WorkspaceChangeEventArgs(WorkspaceChangeKind.ProjectRemoved, oldSolution: SolutionWithTwoProjects, newSolution: solution, projectId: ProjectNumberOne.Id);

            // Act
            trigger.Workspace_WorkspaceChanged(Workspace, e);

            // Assert
            Assert.Collection(
                projectManager.Projects.OrderBy(p => p.WorkspaceProject.Name),
                p => Assert.Equal(ProjectNumberTwo.Id, p.WorkspaceProject.Id));
        }

        [Fact]
        public void WorkspaceChanged_ProjectAddedEvent_AddsProject()
        {
            // Arrange
            var trigger = new WorkspaceProjectSnapshotChangeTrigger();
            var projectManager = new TestProjectSnapshotManager(new[] { trigger }, Workspace);

            var solution = SolutionWithOneProject;
            var e = new WorkspaceChangeEventArgs(WorkspaceChangeKind.ProjectAdded, oldSolution: EmptySolution, newSolution: solution, projectId: ProjectNumberThree.Id);

            // Act
            trigger.Workspace_WorkspaceChanged(Workspace, e);

            // Assert
            Assert.Collection(
                projectManager.Projects.OrderBy(p => p.WorkspaceProject.Name),
                p => Assert.Equal(ProjectNumberThree.Id, p.WorkspaceProject.Id));
        }

        private class TestProjectSnapshotManager : DefaultProjectSnapshotManager
        {
            public TestProjectSnapshotManager(IEnumerable<ProjectSnapshotChangeTrigger> triggers, Workspace workspace) 
                : base(Mock.Of<ForegroundDispatcher>(), Mock.Of<ErrorReporter>(), new TestProjectSnapshotWorker(), triggers, workspace)
            {
            }

            protected override void NotifyBackgroundWorker(Project project)
            {
            }
        }

        private class TestProjectSnapshotWorker : ProjectSnapshotWorker
        {
            public override Task ProcessUpdateAsync(ProjectSnapshotUpdateContext update, CancellationToken cancellationToken = default(CancellationToken))
            {
                return Task.CompletedTask;
            }
        }
    }
}
