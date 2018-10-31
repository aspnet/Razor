﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Text;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    public class DefaultProjectSnapshotManagerTest : ForegroundDispatcherWorkspaceTestBase
    {
        public DefaultProjectSnapshotManagerTest()
        {
            // Force VB and C# to Load
            GC.KeepAlive(typeof(Microsoft.CodeAnalysis.CSharp.SyntaxFactory));
            GC.KeepAlive(typeof(Microsoft.CodeAnalysis.VisualBasic.SyntaxFactory));

            TagHelperResolver = new TestTagHelperResolver();

            Documents = new HostDocument[]
            {
                TestProjectData.SomeProjectFile1,
                TestProjectData.SomeProjectFile2,

                // linked file
                TestProjectData.AnotherProjectNestedFile3,
            };

            HostProject = new HostProject(TestProjectData.SomeProject.FilePath, FallbackRazorConfiguration.MVC_2_0);
            HostProjectWithConfigurationChange = new HostProject(TestProjectData.SomeProject.FilePath, FallbackRazorConfiguration.MVC_1_0);
            
            ProjectManager = new TestProjectSnapshotManager(Dispatcher, Enumerable.Empty<ProjectSnapshotChangeTrigger>(), Workspace);

            var projectId = ProjectId.CreateNewId("Test");
            var solution = Workspace.CurrentSolution.AddProject(ProjectInfo.Create(
                projectId,
                VersionStamp.Default,
                "Test",
                "Test",
                LanguageNames.CSharp,
                TestProjectData.SomeProject.FilePath));
            WorkspaceProject = solution.GetProject(projectId);

            var vbProjectId = ProjectId.CreateNewId("VB");
            solution = solution.AddProject(ProjectInfo.Create(
                vbProjectId,
                VersionStamp.Default,
                "VB",
                "VB",
                LanguageNames.VisualBasic,
                "VB.vbproj"));
            VBWorkspaceProject = solution.GetProject(vbProjectId);

            var projectWithoutFilePathId = ProjectId.CreateNewId("NoFile");
            solution = solution.AddProject(ProjectInfo.Create(
                projectWithoutFilePathId,
                VersionStamp.Default,
                "NoFile",
                "NoFile",
                LanguageNames.CSharp));
            WorkspaceProjectWithoutFilePath = solution.GetProject(projectWithoutFilePathId);

            // Approximates a project with multi-targeting
            var projectIdWithDifferentTfm = ProjectId.CreateNewId("TestWithDifferentTfm");
            solution = Workspace.CurrentSolution.AddProject(ProjectInfo.Create(
                projectIdWithDifferentTfm,
                VersionStamp.Default,
                "Test (Different TFM)",
                "Test",
                LanguageNames.CSharp,
                TestProjectData.SomeProject.FilePath));
            WorkspaceProjectWithDifferentTfm = solution.GetProject(projectIdWithDifferentTfm);

            SomeTagHelpers = TagHelperResolver.TagHelpers;
            SomeTagHelpers.Add(TagHelperDescriptorBuilder.Create("Test1", "TestAssembly").Build());

            SourceText = SourceText.From("Hello world");
        }

        private HostDocument[] Documents { get; }

        private HostProject HostProject { get; }

        private HostProject HostProjectWithConfigurationChange { get; }

        private Project WorkspaceProject { get; }

        private Project WorkspaceProjectWithDifferentTfm { get; }

        private Project WorkspaceProjectWithoutFilePath { get; }

        private Project VBWorkspaceProject { get; }

        private TestTagHelperResolver TagHelperResolver { get; }

        private TestProjectSnapshotManager ProjectManager { get; }

        private SourceText SourceText { get; }

        private IList<TagHelperDescriptor> SomeTagHelpers { get; }

        protected override void ConfigureLanguageServices(List<ILanguageService> services)
        {
            services.Add(TagHelperResolver);
        }

        [ForegroundFact]
        public void DocumentAdded_AddsDocument()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.Collection(snapshot.DocumentFilePaths.OrderBy(f => f), d => Assert.Equal(Documents[0].FilePath, d));

            Assert.Equal(ProjectChangeKind.DocumentAdded, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void DocumentAdded_IgnoresDuplicate()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);
            ProjectManager.Reset();

            // Act
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.Collection(snapshot.DocumentFilePaths.OrderBy(f => f), d => Assert.Equal(Documents[0].FilePath, d));

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void DocumentAdded_IgnoresUnknownProject()
        {
            // Arrange

            // Act
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.Null(snapshot);
        }

        [ForegroundFact]
        public async Task DocumentAdded_NullLoader_HasEmptyText()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            var document = snapshot.GetDocument(snapshot.DocumentFilePaths.Single());

            var text = await document.GetTextAsync();
            Assert.Equal(0, text.Length);
        }

        [ForegroundFact]
        public async Task DocumentAdded_WithLoader_LoadesText()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            var expected = SourceText.From("Hello");

            // Act
            ProjectManager.DocumentAdded(HostProject, Documents[0], TextLoader.From(TextAndVersion.Create(expected,VersionStamp.Default)));

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            var document = snapshot.GetDocument(snapshot.DocumentFilePaths.Single());

            var actual = await document.GetTextAsync();
            Assert.Same(expected, actual);
        }

        [ForegroundFact]
        public async Task DocumentAdded_CachesTagHelpers()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();


            // Adding some computed state
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            await snapshot.GetTagHelpersAsync();

            // Act
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);

            // Assert
            snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.True(snapshot.TryGetTagHelpers(out var _));
        }

        [ForegroundFact]
        public void DocumentAdded_CachesProjectEngine()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.Reset();

            var snapshot = ProjectManager.GetSnapshot(HostProject);
            var projectEngine = snapshot.GetProjectEngine();

            // Act
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);

            // Assert
            snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.Same(projectEngine, snapshot.GetProjectEngine());
        }

        [ForegroundFact]
        public void DocumentRemoved_RemovesDocument()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);
            ProjectManager.DocumentAdded(HostProject, Documents[1], null);
            ProjectManager.DocumentAdded(HostProject, Documents[2], null);
            ProjectManager.Reset();

            // Act
            ProjectManager.DocumentRemoved(HostProject, Documents[1]);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.Collection(
                snapshot.DocumentFilePaths.OrderBy(f => f), 
                d => Assert.Equal(Documents[2].FilePath, d),
                d => Assert.Equal(Documents[0].FilePath, d));

            Assert.Equal(ProjectChangeKind.DocumentRemoved, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void DocumentRemoved_IgnoresNotFoundDocument()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.DocumentRemoved(HostProject, Documents[0]);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.Empty(snapshot.DocumentFilePaths);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void DocumentRemoved_IgnoresUnknownProject()
        {
            // Arrange

            // Act
            ProjectManager.DocumentRemoved(HostProject, Documents[0]);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.Null(snapshot);
        }

        [ForegroundFact]
        public async Task DocumentRemoved_CachesTagHelpers()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);
            ProjectManager.DocumentAdded(HostProject, Documents[1], null);
            ProjectManager.DocumentAdded(HostProject, Documents[2], null);
            ProjectManager.Reset();

            // Adding some computed state
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            await snapshot.GetTagHelpersAsync();

            // Act
            ProjectManager.DocumentRemoved(HostProject, Documents[1]);

            // Assert
            snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.True(snapshot.TryGetTagHelpers(out var _));
        }

        [ForegroundFact]
        public void DocumentRemoved_CachesProjectEngine()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);
            ProjectManager.DocumentAdded(HostProject, Documents[1], null);
            ProjectManager.DocumentAdded(HostProject, Documents[2], null);
            ProjectManager.Reset();

            var snapshot = ProjectManager.GetSnapshot(HostProject);
            var projectEngine = snapshot.GetProjectEngine();

            // Act
            ProjectManager.DocumentRemoved(HostProject, Documents[1]);

            // Assert
            snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.Same(projectEngine, snapshot.GetProjectEngine());
        }

        [ForegroundFact]
        public async Task DocumentOpened_UpdatesDocument()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);
            ProjectManager.Reset();

            // Act
            ProjectManager.DocumentOpened(HostProject.FilePath, Documents[0].FilePath, SourceText);

            // Assert
            Assert.Equal(ProjectChangeKind.DocumentChanged, ProjectManager.ListenersNotifiedOf);

            var snapshot = ProjectManager.GetSnapshot(HostProject);
            var text = await snapshot.GetDocument(Documents[0].FilePath).GetTextAsync();
            Assert.Same(SourceText, text);

            Assert.True(ProjectManager.IsDocumentOpen(Documents[0].FilePath));
        }

        [ForegroundFact]
        public async Task DocumentClosed_UpdatesDocument()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);
            ProjectManager.DocumentOpened(HostProject.FilePath, Documents[0].FilePath, SourceText);
            ProjectManager.Reset();

            var expected = SourceText.From("Hi");
            var textAndVersion = TextAndVersion.Create(expected, VersionStamp.Create());

            Assert.True(ProjectManager.IsDocumentOpen(Documents[0].FilePath));

            // Act
            ProjectManager.DocumentClosed(HostProject.FilePath, Documents[0].FilePath, TextLoader.From(textAndVersion));

            // Assert
            Assert.Equal(ProjectChangeKind.DocumentChanged, ProjectManager.ListenersNotifiedOf);

            var snapshot = ProjectManager.GetSnapshot(HostProject);
            var text = await snapshot.GetDocument(Documents[0].FilePath).GetTextAsync();
            Assert.Same(expected, text);
            Assert.False(ProjectManager.IsDocumentOpen(Documents[0].FilePath));
        }
       

        [ForegroundFact]
        public async Task DocumentClosed_AcceptsChange()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);
            ProjectManager.Reset();

            var expected = SourceText.From("Hi");
            var textAndVersion = TextAndVersion.Create(expected, VersionStamp.Create());

            // Act
            ProjectManager.DocumentClosed(HostProject.FilePath, Documents[0].FilePath, TextLoader.From(textAndVersion));

            // Assert
            Assert.Equal(ProjectChangeKind.DocumentChanged, ProjectManager.ListenersNotifiedOf);

            var snapshot = ProjectManager.GetSnapshot(HostProject);
            var text = await snapshot.GetDocument(Documents[0].FilePath).GetTextAsync();
            Assert.Same(expected, text);
        }

        [ForegroundFact]
        public async Task DocumentChanged_Snapshot_UpdatesDocument()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);
            ProjectManager.DocumentOpened(HostProject.FilePath, Documents[0].FilePath, SourceText);
            ProjectManager.Reset();

            var expected = SourceText.From("Hi");

            // Act
            ProjectManager.DocumentChanged(HostProject.FilePath, Documents[0].FilePath, expected);

            // Assert
            Assert.Equal(ProjectChangeKind.DocumentChanged, ProjectManager.ListenersNotifiedOf);

            var snapshot = ProjectManager.GetSnapshot(HostProject);
            var text = await snapshot.GetDocument(Documents[0].FilePath).GetTextAsync();
            Assert.Same(expected, text);
        }

        [ForegroundFact]
        public async Task DocumentChanged_Loader_UpdatesDocument()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.DocumentAdded(HostProject, Documents[0], null);
            ProjectManager.DocumentOpened(HostProject.FilePath, Documents[0].FilePath, SourceText);
            ProjectManager.Reset();

            var expected = SourceText.From("Hi");
            var textAndVersion = TextAndVersion.Create(expected, VersionStamp.Create());

            // Act
            ProjectManager.DocumentChanged(HostProject.FilePath, Documents[0].FilePath, TextLoader.From(textAndVersion));

            // Assert
            Assert.Equal(ProjectChangeKind.DocumentChanged, ProjectManager.ListenersNotifiedOf);

            var snapshot = ProjectManager.GetSnapshot(HostProject);
            var text = await snapshot.GetDocument(Documents[0].FilePath).GetTextAsync();
            Assert.Same(expected, text);
        }

        [ForegroundFact]
        public void HostProjectAdded_WithoutWorkspaceProject_NotifiesListeners()
        {
            // Arrange

            // Act
            ProjectManager.HostProjectAdded(HostProject);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.False(snapshot.IsInitialized);

            Assert.Equal(ProjectChangeKind.ProjectAdded, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void HostProjectAdded_FindsWorkspaceProject_NotifiesListeners()
        {
            // Arrange
            Assert.True(Workspace.TryApplyChanges(WorkspaceProject.Solution));

            // Act
            ProjectManager.HostProjectAdded(HostProject);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProject);
            Assert.True(snapshot.IsInitialized);

            Assert.Equal(ProjectChangeKind.ProjectAdded, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void HostProjectChanged_ConfigurationChange_WithoutWorkspaceProject_NotifiesListeners()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.HostProjectChanged(HostProjectWithConfigurationChange);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProjectWithConfigurationChange);
            Assert.False(snapshot.IsInitialized);

            Assert.Equal(ProjectChangeKind.ProjectChanged, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void HostProjectChanged_ConfigurationChange_WithWorkspaceProject_NotifiesListeners()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.HostProjectChanged(HostProjectWithConfigurationChange);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(HostProjectWithConfigurationChange);
            Assert.True(snapshot.IsInitialized);

            Assert.Equal(ProjectChangeKind.ProjectChanged, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void HostProjectChanged_ConfigurationChange_DoesNotCacheProjectEngine()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            var snapshot = ProjectManager.GetSnapshot(HostProject);
            var projectEngine = snapshot.GetProjectEngine();

            // Act
            ProjectManager.HostProjectChanged(HostProjectWithConfigurationChange);

            // Assert
            snapshot = ProjectManager.GetSnapshot(HostProjectWithConfigurationChange);
            Assert.NotSame(projectEngine, snapshot.GetProjectEngine());
        }

        [ForegroundFact]
        public async Task HostProjectChanged_ConfigurationChange_DoesNotCacheComputedState()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            var snapshot = ProjectManager.GetSnapshot(HostProject);
            ProjectManager.Reset();

            // Adding some computed state
            await snapshot.GetTagHelpersAsync();

            // Act
            ProjectManager.HostProjectChanged(HostProjectWithConfigurationChange);

            // Assert
            snapshot = ProjectManager.GetSnapshot(HostProjectWithConfigurationChange);
            Assert.False(snapshot.TryGetTagHelpers(out var _));
        }

        [ForegroundFact]
        public void HostProjectChanged_IgnoresUnknownProject()
        {
            // Arrange

            // Act
            ProjectManager.HostProjectChanged(HostProject);

            // Assert
            Assert.Empty(ProjectManager.Projects);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void HostProjectRemoved_RemovesProject_NotifiesListeners()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.HostProjectRemoved(HostProject);

            // Assert
            Assert.Empty(ProjectManager.Projects);

            Assert.Equal(ProjectChangeKind.ProjectRemoved, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectAdded_WithoutHostProject_IgnoresWorkspaceProject()
        {
            // Arrange

            // Act
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);

            // Assert
            Assert.Empty(ProjectManager.Projects);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectAdded_IgnoresNonCSharpProject()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectAdded(VBWorkspaceProject);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.False(snapshot.IsInitialized);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectAdded_IgnoresSecondProjectWithSameFilePath()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectAdded(WorkspaceProjectWithDifferentTfm);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.Same(WorkspaceProject, snapshot.WorkspaceProject);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectAdded_IgnoresProjectWithoutFilePath()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectAdded(WorkspaceProjectWithoutFilePath);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.False(snapshot.IsInitialized);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectAdded_WithHostProject_NotifiesListenters()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.True(snapshot.IsInitialized);

            Assert.Equal(ProjectChangeKind.ProjectChanged, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectChanged_WithHostProject_NotifiesListenters()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectChanged(WorkspaceProject.WithAssemblyName("Test1"));

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.True(snapshot.IsInitialized);

            Assert.Equal(ProjectChangeKind.ProjectChanged, ProjectManager.ListenersNotifiedOf);
        }

        // We always update the snapshot when someone calls WorkspaceProjectChanged. This is how we deal
        // with changes to source code, which wouldn't result in a new project.
        [ForegroundFact]
        public void WorkspaceProjectChanged_WithHostProject_NotifiesListeners()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectChanged(WorkspaceProject);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.True(snapshot.IsInitialized);

            Assert.Equal(ProjectChangeKind.ProjectChanged, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectChanged_WithHostProject_CanNoOpForSecondProject()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectChanged(WorkspaceProjectWithDifferentTfm);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.True(snapshot.IsInitialized);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectChanged_WithoutHostProject_IgnoresWorkspaceProject()
        {
            // Arrange
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            var project = WorkspaceProject.WithAssemblyName("Test1"); // Simulate a project change

            // Act
            ProjectManager.WorkspaceProjectChanged(project);

            // Assert
            Assert.Empty(ProjectManager.Projects);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectChanged_IgnoresNonCSharpProject()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(VBWorkspaceProject);
            ProjectManager.Reset();

            var project = VBWorkspaceProject.WithAssemblyName("Test1"); // Simulate a project change

            // Act
            ProjectManager.WorkspaceProjectChanged(project);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.False(snapshot.IsInitialized);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectChanged_IgnoresProjectWithoutFilePath()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProjectWithoutFilePath);
            ProjectManager.Reset();

            var project = WorkspaceProjectWithoutFilePath.WithAssemblyName("Test1"); // Simulate a project change

            // Act
            ProjectManager.WorkspaceProjectChanged(project);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.False(snapshot.IsInitialized);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectChanged_IgnoresSecondProjectWithSameFilePath()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectChanged(WorkspaceProjectWithDifferentTfm);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.Same(WorkspaceProject, snapshot.WorkspaceProject);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public async Task WorkspaceProjectRemoved_DoesNotRemoveProject_RemovesTagHelpers()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            var snapshot = ProjectManager.GetSnapshot(HostProject);

            // Adding some computed state
            await snapshot.GetTagHelpersAsync();

            // Act
            ProjectManager.WorkspaceProjectRemoved(WorkspaceProject);

            // Assert
            snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.False(snapshot.IsInitialized);
            Assert.False(snapshot.TryGetTagHelpers(out var _));

            Assert.Equal(ProjectChangeKind.ProjectChanged, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public async Task WorkspaceProjectRemoved_FallsBackToSecondProject()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            var snapshot = ProjectManager.GetSnapshot(HostProject);

            // Adding some computed state
            await snapshot.GetTagHelpersAsync();

            // Sets up a solution where the which has WorkspaceProjectWithDifferentTfm but not WorkspaceProject
            // This will enable us to fall back and find the WorkspaceProjectWithDifferentTfm 
            Assert.True(Workspace.TryApplyChanges(WorkspaceProjectWithDifferentTfm.Solution));

            // Act
            ProjectManager.WorkspaceProjectRemoved(WorkspaceProject);

            // Assert
            snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.True(snapshot.IsInitialized);
            Assert.Equal(WorkspaceProjectWithDifferentTfm.Id, snapshot.WorkspaceProject.Id);
            Assert.False(snapshot.TryGetTagHelpers(out var _));

            Assert.Equal(ProjectChangeKind.ProjectChanged, ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectRemoved_IgnoresSecondProjectWithSameFilePath()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectRemoved(WorkspaceProjectWithDifferentTfm);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.Same(WorkspaceProject, snapshot.WorkspaceProject);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectRemoved_IgnoresNonCSharpProject()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(VBWorkspaceProject);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectRemoved(VBWorkspaceProject);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.False(snapshot.IsInitialized);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectRemoved_IgnoresProjectWithoutFilePath()
        {
            // Arrange
            ProjectManager.HostProjectAdded(HostProject);
            ProjectManager.WorkspaceProjectAdded(WorkspaceProjectWithoutFilePath);
            ProjectManager.Reset();

            // Act
            ProjectManager.WorkspaceProjectRemoved(WorkspaceProjectWithoutFilePath);

            // Assert
            var snapshot = ProjectManager.GetSnapshot(WorkspaceProject);
            Assert.False(snapshot.IsInitialized);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        [ForegroundFact]
        public void WorkspaceProjectRemoved_IgnoresUnknownProject()
        {
            // Arrange

            // Act
            ProjectManager.WorkspaceProjectRemoved(WorkspaceProject);

            // Assert
            Assert.Empty(ProjectManager.Projects);

            Assert.Null(ProjectManager.ListenersNotifiedOf);
        }

        private class TestProjectSnapshotManager : DefaultProjectSnapshotManager
        {
            public TestProjectSnapshotManager(ForegroundDispatcher dispatcher, IEnumerable<ProjectSnapshotChangeTrigger> triggers, Workspace workspace)
                : base(dispatcher, Mock.Of<ErrorReporter>(), triggers, workspace)
            {
            }

            public ProjectChangeKind? ListenersNotifiedOf { get; private set; }

            public DefaultProjectSnapshot GetSnapshot(HostProject hostProject)
            {
                return Projects.Cast<DefaultProjectSnapshot>().FirstOrDefault(s => s.FilePath == hostProject.FilePath);
            }

            public DefaultProjectSnapshot GetSnapshot(Project workspaceProject)
            {
                return Projects.Cast<DefaultProjectSnapshot>().FirstOrDefault(s => s.FilePath == workspaceProject.FilePath);
            }

            public void Reset()
            {
                ListenersNotifiedOf = null;
            }

            protected override void NotifyListeners(ProjectChangeEventArgs e)
            {
                ListenersNotifiedOf = e.Kind;
            }
        }
    }
}
