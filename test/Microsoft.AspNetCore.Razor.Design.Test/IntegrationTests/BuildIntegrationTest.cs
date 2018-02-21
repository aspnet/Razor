// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Testing.xunit;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Design.IntegrationTests
{
    public class BuildIntegrationTest : MSBuildIntegrationTestBase
    {
        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public Task Build_SimpleMvc_UsingDotnetMSBuild_CanBuildSuccessfully()
            => Build_SimpleMvc_CanBuildSuccessfully(MSBuildProcessKind.Dotnet);

        [ConditionalFact]
        [OSSkipCondition(OperatingSystems.Linux)]
        [OSSkipCondition(OperatingSystems.MacOSX)]
        [InitializeTestProject("SimpleMvc")]
        public Task Build_SimpleMvc_UsingDesktopMSBuild_CanBuildSuccessfully()
            => Build_SimpleMvc_CanBuildSuccessfully(MSBuildProcessKind.Desktop);

        private async Task Build_SimpleMvc_CanBuildSuccessfully(MSBuildProcessKind msBuildProcessKind)
        {
            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=true /t:_IntrospectPreserveCompilationContext", msBuildProcessKind: msBuildProcessKind);

            Assert.BuildPassed(result);
            Assert.FileExists(result, OutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc.pdb");
            Assert.FileExists(result, OutputPath, "SimpleMvc.PrecompiledViews.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc.PrecompiledViews.pdb");
            Assert.BuildOutputContainsLine(result, "PreserveCompilationContext: true");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // GetFullPath on OSX doesn't work well in travis. We end up computing a different path than will
                // end up in the MSBuild logs.
                Assert.BuildOutputContainsLine(result, $"SimpleMvc -> {Path.Combine(Path.GetFullPath(Project.DirectoryPath), OutputPath, "SimpleMvc.PrecompiledViews.dll")}");
            }
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_SimpleMvc_NoopsWithNoFiles()
        {
            Directory.Delete(Path.Combine(Project.DirectoryPath, "Views"), recursive: true);

            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=true /t:_IntrospectPreserveCompilationContext");

            Assert.BuildPassed(result);
            Assert.FileExists(result, OutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.PrecompiledViews.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.PrecompiledViews.pdb");
            // This expectation needs to be updated once we get a CLI with an updated WebSdk
            Assert.BuildOutputContainsLine(result, "PreserveCompilationContext: true");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_SimpleMvc_NoopsWithRazorCompileOnPublish()
        {
            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnPublish=true");

            Assert.BuildPassed(result);
            Assert.FileExists(result, OutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.PrecompiledViews.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.PrecompiledViews.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_ErrorInGeneratedCode_ReportsMSBuildError()
        {
            // Introducing a C# semantic error
            ReplaceContent("@{ var foo = \"\".Substring(\"bleh\"); }", "Views", "Home", "Index.cshtml");

            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=true");

            Assert.BuildFailed(result);

            // Verifying that the error correctly gets mapped to the original source
            Assert.BuildError(result, "CS1503", location: Path.Combine("Views", "Home", "Index.cshtml") + "(1,27)");

            // Compilation failed without creating the views assembly
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.dll");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.PrecompiledViews.dll");
        }

        [Fact]
        [InitializeTestProject("SimplePages")]
        public async Task Build_Works_WhenFilesAtDifferentPathsHaveSameNamespaceHierarchy()
        {
            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=true");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimplePages.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimplePages.PrecompiledViews.dll");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_RazorOutputPath_SetToNonDefault()
        {
            var customOutputPath = Path.Combine("bin", Configuration, TargetFramework, "Razor");
            var result = await DotnetMSBuild("Build", $"/p:RazorCompileOnBuild=true /p:RazorOutputPath={customOutputPath}");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.PrecompiledViews.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.PrecompiledViews.pdb");

            Assert.FileExists(result, customOutputPath, "SimpleMvc.PrecompiledViews.dll");
            Assert.FileExists(result, customOutputPath, "SimpleMvc.PrecompiledViews.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_MvcRazorOutputPath_SetToNonDefault()
        {
            var customOutputPath = Path.Combine("bin", Configuration, TargetFramework, "Razor");
            var result = await DotnetMSBuild("Build", $"/p:RazorCompileOnBuild=true /p:MvcRazorOutputPath={customOutputPath}");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.PrecompiledViews.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.PrecompiledViews.pdb");

            Assert.FileExists(result, customOutputPath, "SimpleMvc.PrecompiledViews.dll");
            Assert.FileExists(result, customOutputPath, "SimpleMvc.PrecompiledViews.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_SkipsCopyingBinariesToOutputDirectory_IfCopyBuildOutputToOutputDirectory_IsUnset()
        {
            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=true /p:CopyBuildOutputToOutputDirectory=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.PrecompiledViews.dll");

            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.PrecompiledViews.dll");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_SkipsCopyingBinariesToOutputDirectory_IfCopyOutputSymbolsToOutputDirectory_IsUnset()
        {
            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=true /p:CopyOutputSymbolsToOutputDirectory=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.PrecompiledViews.pdb");

            Assert.FileExists(result, OutputPath, "SimpleMvc.PrecompiledViews.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.PrecompiledViews.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_Works_WhenSymbolsAreNotGenerated()
        {
            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=true /p:DebugType=none");

            Assert.BuildPassed(result);

            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.PrecompiledViews.pdb");

            Assert.FileExists(result, OutputPath, "SimpleMvc.PrecompiledViews.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.PrecompiledViews.pdb");
        }

        [Fact]
        [InitializeTestProject("AppWithP2PReference", "ClassLibrary")]
        public async Task Build_WithP2P_CopiesRazorAssembly()
        {
            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=true");

            Assert.BuildPassed(result);

            Assert.FileExists(result, OutputPath, "AppWithP2PReference.dll");
            Assert.FileExists(result, OutputPath, "AppWithP2PReference.pdb");
            Assert.FileExists(result, OutputPath, "AppWithP2PReference.PrecompiledViews.dll");
            Assert.FileExists(result, OutputPath, "AppWithP2PReference.PrecompiledViews.pdb");
            Assert.FileExists(result, OutputPath, "ClassLibrary.dll");
            Assert.FileExists(result, OutputPath, "ClassLibrary.pdb");
            Assert.FileExists(result, OutputPath, "ClassLibrary.PrecompiledViews.dll");
            Assert.FileExists(result, OutputPath, "ClassLibrary.PrecompiledViews.pdb");
        }

        [Fact]
        [InitializeTestProject("SimplePages", "LinkedDir")]
        public async Task Build_SetsUpEmbeddedResourcesWithLogicalName()
        {
            // Arrange
            var additionalProjectContent = @"
<ItemGroup>
  <Content Include=""..\LinkedDir\LinkedFile.cshtml"" Link=""LinkedFileOut\LinkedFile.cshtml"" />
</ItemGroup>
";
            AddProjectFileContent(additionalProjectContent);
            Directory.CreateDirectory(Path.Combine(Project.DirectoryPath, "..", "LinkedDir"));

            var result = await DotnetMSBuild("Build", "/t:_IntrospectRazorEmbeddedResources /p:RazorCompileOnBuild=true /p:EmbedRazorGenerateSources=true");

            Assert.BuildPassed(result);

            Assert.BuildOutputContainsLine(result, $@"CompileResource: {Path.Combine("Pages", "Index.cshtml")} /Pages/Index.cshtml");
            Assert.BuildOutputContainsLine(result, $@"CompileResource: {Path.Combine("Areas", "Products", "Pages", "_ViewStart.cshtml")} /Areas/Products/Pages/_ViewStart.cshtml");
            Assert.BuildOutputContainsLine(result, $@"CompileResource: {Path.Combine("..", "LinkedDir", "LinkedFile.cshtml")} /LinkedFileOut/LinkedFile.cshtml");
        }
    }
}
