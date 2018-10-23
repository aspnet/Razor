﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.Text;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    public class GeneratedCodeContainerTest
    {
        [Fact]
        public void SetOutput_AcceptsSameVersionedDocuments()
        {
            // Arrange
            var csharpDocument = RazorCSharpDocument.Create("...", RazorCodeGenerationOptions.CreateDefault(), Enumerable.Empty<RazorDiagnostic>());
            var hostProject = new HostProject("C:/project.csproj", RazorConfiguration.Default);
            var services = TestWorkspace.Create().Services;
            var projectState = ProjectState.Create(services, hostProject);
            var project = new DefaultProjectSnapshot(projectState);
            var hostDocument = new HostDocument("C:/file.cshtml", "C:/file.cshtml");
            var text = SourceText.From("...");
            var textAndVersion = TextAndVersion.Create(text, VersionStamp.Default);
            var documentState = new DocumentState(services, hostDocument, text, VersionStamp.Default, () => Task.FromResult(textAndVersion));
            var document = new DefaultDocumentSnapshot(project, documentState);
            var newDocument = new DefaultDocumentSnapshot(project, documentState);
            var container = new GeneratedCodeContainer();
            container.SetOutput(csharpDocument, document);

            // Act
            container.SetOutput(csharpDocument, newDocument);

            // Assert
            Assert.Same(newDocument, container.LatestDocument);
        }

        [Fact]
        public void SetOutput_AcceptsInitialOutput()
        {
            // Arrange
            var csharpDocument = RazorCSharpDocument.Create("...", RazorCodeGenerationOptions.CreateDefault(), Enumerable.Empty<RazorDiagnostic>());
            var hostProject = new HostProject("C:/project.csproj", RazorConfiguration.Default);
            var services = TestWorkspace.Create().Services;
            var projectState = ProjectState.Create(services, hostProject);
            var project = new DefaultProjectSnapshot(projectState);
            var hostDocument = new HostDocument("C:/file.cshtml", "C:/file.cshtml");
            var text = SourceText.From("...");
            var textAndVersion = TextAndVersion.Create(text, VersionStamp.Default);
            var documentState = new DocumentState(services, hostDocument, text, VersionStamp.Default, () => Task.FromResult(textAndVersion));
            var document = new DefaultDocumentSnapshot(project, documentState);
            var container = new GeneratedCodeContainer();

            // Act
            container.SetOutput(csharpDocument, document);

            // Assert
            Assert.NotNull(container.LatestDocument);
        }

        private static RazorCodeDocument GetCodeDocument(string content)
        {
            var sourceProjectItem = new TestRazorProjectItem("test.cshtml")
            {
                Content = content,
            };

            var engine = RazorProjectEngine.Create();
            var codeDocument = engine.ProcessDesignTime(sourceProjectItem);
            return codeDocument;
        }
    }
}
