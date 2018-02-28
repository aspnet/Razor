// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Design.IntegrationTests
{
    public class BuildIntrospectionTest : MSBuildIntegrationTestBase
    {
        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task RazorSdk_AddsCshtmlFilesToUpToDateCheckInput()
        {
            var result = await DotnetMSBuild("_IntrospectUpToDateCheckInput", "/p:RazorCompileOnBuild=true");

            Assert.BuildPassed(result);
            Assert.BuildOutputContainsLine(result, $"UpToDateCheckInput: {Path.Combine("Views", "Home", "Index.cshtml")}");
            Assert.BuildOutputContainsLine(result, $"UpToDateCheckInput: {Path.Combine("Views", "_ViewStart.cshtml")}");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task RazorSdk_SetsPreserveCompilationContextForProjectsWithCshtmlFiles()
        {
            var result = await DotnetMSBuild("Build", "/t:_IntrospectPreserveCompilationContext");

            Assert.BuildPassed(result);
            Assert.BuildOutputContainsLine(result, "PreserveCompilationContext: true");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task RazorSdk_DoesNotSetPreserveCompilationContextForWebApplicationsWithoutCshtmlFiles()
        {
            Directory.Delete(Path.Combine(Project.DirectoryPath, "Views"), recursive: true);

            var result = await DotnetMSBuild("Build", "/t:_IntrospectPreserveCompilationContext");

            Assert.BuildPassed(result);
            Assert.BuildOutputContainsLine(result, "PreserveCompilationContext: false");
        }

        [Fact]
        [InitializeTestProject("ClassLibrary")]
        public async Task RazorSdk_DoesNotSetPreserveCompilationContextForClassLibraryProjects()
        {
            var result = await DotnetMSBuild("Build", "/t:_IntrospectPreserveCompilationContext");

            Assert.BuildPassed(result);
            Assert.BuildOutputContainsLine(result, "PreserveCompilationContext: false");
        }
    }
}
