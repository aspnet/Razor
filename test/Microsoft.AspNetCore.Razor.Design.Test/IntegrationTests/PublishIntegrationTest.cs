﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Design.IntegrationTests
{
    public class PublishIntegrationTest : MSBuildIntegrationTestBase, IClassFixture<BuildServerTestFixture>
    {
        public PublishIntegrationTest(BuildServerTestFixture buildServer)
            : base(buildServer)
        {
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_RazorCompileOnPublish_IsDefault()
        {
            var result = await DotnetMSBuild("Publish");

            Assert.BuildPassed(result);

            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.pdb");

            // By default refs and .cshtml files will not be copied on publish
            // Skipped until we get a new Sdk. See https://github.com/aspnet/Razor/issues/2259
            // Assert.FileCountEquals(result, 0, Path.Combine(PublishOutputPath, "refs"), "*.dll");
            Assert.FileCountEquals(result, 0, Path.Combine(PublishOutputPath, "Views"), "*.cshtml");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_PublishesAssembly()
        {
            var result = await DotnetMSBuild("Publish");

            Assert.BuildPassed(result);

            Assert.FileExists(result, OutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc.pdb");
            Assert.FileExists(result, OutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc.Views.pdb");

            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.pdb");

            // By default refs and .cshtml files will not be copied on publish
            // Skipped until we get a new Sdk. See https://github.com/aspnet/Razor/issues/2259
            // Assert.FileCountEquals(result, 0, Path.Combine(PublishOutputPath, "refs"), "*.dll");
            Assert.FileCountEquals(result, 0, Path.Combine(PublishOutputPath, "Views"), "*.cshtml");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_WithRazorCompileOnPublish_PublishesAssembly()
        {
            var result = await DotnetMSBuild("Publish");

            Assert.BuildPassed(result);

            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.pdb");

            // By default refs and .cshtml files will not be copied on publish
            // Skipped until we get a new Sdk. See https://github.com/aspnet/Razor/issues/2259
            // Assert.FileCountEquals(result, 0, Path.Combine(PublishOutputPath, "refs"), "*.dll");
            Assert.FileCountEquals(result, 0, Path.Combine(PublishOutputPath, "Views"), "*.cshtml");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_WithRazorCompileOnBuildFalse_PublishesAssembly()
        {
            // RazorCompileOnBuild is turned off, but RazorCompileOnPublish should still be enabled
            var result = await DotnetMSBuild("Publish", "/p:RazorCompileOnBuild=false");

            Assert.BuildPassed(result);

            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.Views.pdb");

            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.pdb");

            // By default refs and .cshtml files will not be copied on publish
            // Skipped until we get a new Sdk. See https://github.com/aspnet/Razor/issues/2259
            // Assert.FileCountEquals(result, 0, Path.Combine(PublishOutputPath, "refs"), "*.dll");
            Assert.FileCountEquals(result, 0, Path.Combine(PublishOutputPath, "Views"), "*.cshtml");
        }

        [Fact] // This will use the old precompilation tool, RazorSDK shouldn't get involved.
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_WithMvcRazorCompileOnPublish_Noops()
        {
            var result = await DotnetMSBuild("Publish", "/p:MvcRazorCompileOnPublish=true");

            Assert.BuildPassed(result);

            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact] // This is an override to force the new toolset
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_WithMvcRazorCompileOnPublish_AndRazorSDK_PublishesAssembly()
        {
            var result = await DotnetMSBuild("Publish", "/p:MvcRazorCompileOnPublish=true /p:ResolvedRazorCompileToolset=RazorSDK");

            Assert.BuildPassed(result);

            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.pdb");

            // By default refs and .cshtml files will not be copied on publish
            // Skipped until we get a new Sdk. See https://github.com/aspnet/Razor/issues/2259
            // Assert.FileCountEquals(result, 0, Path.Combine(PublishOutputPath, "refs"), "*.dll");
            Assert.FileCountEquals(result, 0, Path.Combine(PublishOutputPath, "Views"), "*.cshtml");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_NoopsWithNoFiles()
        {
            Directory.Delete(Path.Combine(Project.DirectoryPath, "Views"), recursive: true);

            var result = await DotnetMSBuild("Publish");

            Assert.BuildPassed(result);

            // Everything we do should noop - including building the app. 
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_NoopsWithMvcRazorCompileOnPublish_False()
        {
            var result = await DotnetMSBuild("Publish", "/p:MvcRazorCompileOnPublish=false");

            Assert.BuildPassed(result);

            // Everything we do should noop - including building the app. 
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_NoopsWith_RazorCompileOnPublishFalse()
        {
            Directory.Delete(Path.Combine(Project.DirectoryPath, "Views"), recursive: true);

            var result = await DotnetMSBuild("Publish", "/p:RazorCompileOnPublish=false");

            Assert.BuildPassed(result);

            // Everything we do should noop - including building the app. 
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_SkipsCopyingBinariesToOutputDirectory_IfCopyBuildOutputToOutputDirectory_IsUnset()
        {
            var result = await DotnetMSBuild("Publish", "/p:CopyBuildOutputToPublishDirectory=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.dll");

            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.dll");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_SkipsCopyingBinariesToOutputDirectory_IfCopyOutputSymbolsToOutputDirectory_IsUnset()
        {
            var result = await DotnetMSBuild("Publish", "/p:CopyOutputSymbolsToPublishDirectory=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");

            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_Works_WhenSymbolsAreNotGenerated()
        {
            var result = await DotnetMSBuild("Publish", "/p:DebugType=none");

            Assert.BuildPassed(result);

            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");

            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, PublishOutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_IncludeCshtmlAndRefAssemblies_CopiesFiles()
        {
            var result = await DotnetMSBuild("Publish", "/p:CopyRazorGenerateFilesToPublishDirectory=true /p:CopyRefAssembliesToPublishDirectory=true");

            Assert.BuildPassed(result);

            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.pdb");

            // By default refs and .cshtml files will not be copied on publish
            Assert.FileExists(result, PublishOutputPath, "refs", "mscorlib.dll");
            Assert.FileCountEquals(result, 8, Path.Combine(PublishOutputPath, "Views"), "*.cshtml");
        }

        [Fact] // Tests old MvcPrecompilation behavior that we support for compat.
        [InitializeTestProject("SimpleMvc")]
        public async Task Publish_MvcRazorExcludeFilesFromPublish_False_CopiesFiles()
        {
            var result = await DotnetMSBuild("Publish", "/p:MvcRazorExcludeViewFilesFromPublish=false /p:MvcRazorExcludeRefAssembliesFromPublish=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.pdb");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "SimpleMvc.Views.pdb");

            // By default refs and .cshtml files will not be copied on publish
            Assert.FileExists(result, PublishOutputPath, "refs", "mscorlib.dll");
            Assert.FileCountEquals(result, 8, Path.Combine(PublishOutputPath, "Views"), "*.cshtml");
        }

        [Fact]
        [InitializeTestProject("AppWithP2PReference", "ClassLibrary")]
        public async Task Publish_WithP2P_AndRazorCompileOnBuild_CopiesRazorAssembly()
        {
            var result = await DotnetMSBuild("Publish");

            Assert.BuildPassed(result);

            Assert.FileExists(result, PublishOutputPath, "AppWithP2PReference.dll");
            Assert.FileExists(result, PublishOutputPath, "AppWithP2PReference.pdb");
            Assert.FileExists(result, PublishOutputPath, "AppWithP2PReference.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "AppWithP2PReference.Views.pdb");
            Assert.FileExists(result, PublishOutputPath, "ClassLibrary.dll");
            Assert.FileExists(result, PublishOutputPath, "ClassLibrary.pdb");
            Assert.FileExists(result, PublishOutputPath, "ClassLibrary.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "ClassLibrary.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("AppWithP2PReference", "ClassLibrary")]
        public async Task Publish_WithP2P_AndRazorCompileOnPublish_CopiesRazorAssembly()
        {
            var result = await DotnetMSBuild("Publish");

            Assert.BuildPassed(result);

            Assert.FileExists(result, PublishOutputPath, "AppWithP2PReference.dll");
            Assert.FileExists(result, PublishOutputPath, "AppWithP2PReference.pdb");
            Assert.FileExists(result, PublishOutputPath, "AppWithP2PReference.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "AppWithP2PReference.Views.pdb");
            Assert.FileExists(result, PublishOutputPath, "ClassLibrary.dll");
            Assert.FileExists(result, PublishOutputPath, "ClassLibrary.pdb");
            Assert.FileExists(result, PublishOutputPath, "ClassLibrary.Views.dll");
            Assert.FileExists(result, PublishOutputPath, "ClassLibrary.Views.pdb");
        }
    }
}
