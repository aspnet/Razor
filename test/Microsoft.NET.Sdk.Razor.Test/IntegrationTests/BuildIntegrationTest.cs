// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Testing.xunit;
using Microsoft.Extensions.DependencyModel;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Design.IntegrationTests
{
    public class BuildIntegrationTest : MSBuildIntegrationTestBase, IClassFixture<BuildServerTestFixture>
    {
        public BuildIntegrationTest(BuildServerTestFixture buildServer)
            : base(buildServer)
        {
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public Task Build_SimpleMvc_UsingDotnetMSBuildAndWithoutBuildServer_CanBuildSuccessfully()
            => Build_SimpleMvc_WithoutBuildServer_CanBuildSuccessfully(MSBuildProcessKind.Dotnet);

        [ConditionalFact]
        [OSSkipCondition(OperatingSystems.Linux | OperatingSystems.MacOSX)]
        [InitializeTestProject("SimpleMvc")]
        public Task Build_SimpleMvc_UsingDesktopMSBuildAndWithoutBuildServer_CanBuildSuccessfully()
            => Build_SimpleMvc_WithoutBuildServer_CanBuildSuccessfully(MSBuildProcessKind.Desktop);

        // This test is identical to the ones in BuildServerIntegrationTest except this one explicitly disables the Razor build server.
        private async Task Build_SimpleMvc_WithoutBuildServer_CanBuildSuccessfully(MSBuildProcessKind msBuildProcessKind)
        {
            var result = await DotnetMSBuild("Build",
                "/p:UseRazorBuildServer=false",
                suppressBuildServer: true,
                msBuildProcessKind: msBuildProcessKind);

            Assert.BuildPassed(result);
            Assert.FileExists(result, OutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc.pdb");
            Assert.FileExists(result, OutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc.Views.pdb");

            if (!RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                // GetFullPath on OSX doesn't work well in travis. We end up computing a different path than will
                // end up in the MSBuild logs.
                Assert.BuildOutputContainsLine(result, $"SimpleMvc -> {Path.Combine(Path.GetFullPath(Project.DirectoryPath), OutputPath, "SimpleMvc.Views.dll")}");
            }
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_SimpleMvc_NoopsWithNoFiles()
        {
            Directory.Delete(Path.Combine(Project.DirectoryPath, "Views"), recursive: true);

            var result = await DotnetMSBuild("Build");

            Assert.BuildPassed(result);
            Assert.FileExists(result, OutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.Views.pdb");

            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.RazorTargetAssemblyInfo.cs");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_SimpleMvc_NoopsWithRazorCompileOnBuild_False()
        {
            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=false");

            Assert.BuildPassed(result);
            Assert.FileExists(result, OutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_ErrorInGeneratedCode_ReportsMSBuildError()
        {
            // Introducing a C# semantic error
            ReplaceContent("@{ var foo = \"\".Substring(\"bleh\"); }", "Views", "Home", "Index.cshtml");

            var result = await DotnetMSBuild("Build");

            Assert.BuildFailed(result);

            // Verifying that the error correctly gets mapped to the original source
            Assert.BuildError(result, "CS1503", location: Path.Combine("Views", "Home", "Index.cshtml") + "(1,27)");

            // Compilation failed without creating the views assembly
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.dll");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
        }

        [Fact]
        [InitializeTestProject("SimplePages")]
        public async Task Build_Works_WhenFilesAtDifferentPathsHaveSameNamespaceHierarchy()
        {
            var result = await DotnetMSBuild("Build");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimplePages.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimplePages.Views.dll");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_RazorOutputPath_SetToNonDefault()
        {
            var customOutputPath = Path.Combine("bin", Configuration, TargetFramework, "Razor");
            var result = await DotnetMSBuild("Build", $"/p:RazorOutputPath={customOutputPath}");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");

            Assert.FileExists(result, customOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, customOutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_MvcRazorOutputPath_SetToNonDefault()
        {
            var customOutputPath = Path.Combine("bin", Configuration, TargetFramework, "Razor");
            var result = await DotnetMSBuild("Build", $"/p:MvcRazorOutputPath={customOutputPath}");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");

            Assert.FileExists(result, customOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, customOutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_SkipsCopyingBinariesToOutputDirectory_IfCopyBuildOutputToOutputDirectory_IsUnset()
        {
            var result = await DotnetMSBuild("Build", "/p:CopyBuildOutputToOutputDirectory=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.dll");

            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.Views.dll");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_SkipsCopyingBinariesToOutputDirectory_IfCopyOutputSymbolsToOutputDirectory_IsUnset()
        {
            var result = await DotnetMSBuild("Build", "/p:CopyOutputSymbolsToOutputDirectory=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");

            Assert.FileExists(result, OutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_Works_WhenSymbolsAreNotGenerated()
        {
            var result = await DotnetMSBuild("Build", "/p:DebugType=none");

            Assert.BuildPassed(result);

            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");

            Assert.FileExists(result, OutputPath, "SimpleMvc.Views.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.pdb");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("AppWithP2PReference", additionalProjects: "ClassLibrary")]
        public async Task Build_WithP2P_CopiesRazorAssembly()
        {
            var result = await DotnetMSBuild("Build");

            Assert.BuildPassed(result);

            Assert.FileExists(result, OutputPath, "AppWithP2PReference.dll");
            Assert.FileExists(result, OutputPath, "AppWithP2PReference.pdb");
            Assert.FileExists(result, OutputPath, "AppWithP2PReference.Views.dll");
            Assert.FileExists(result, OutputPath, "AppWithP2PReference.Views.pdb");
            Assert.FileExists(result, OutputPath, "ClassLibrary.dll");
            Assert.FileExists(result, OutputPath, "ClassLibrary.pdb");
            Assert.FileExists(result, OutputPath, "ClassLibrary.Views.dll");
            Assert.FileExists(result, OutputPath, "ClassLibrary.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimplePages", additionalProjects: "LinkedDir")]
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

            var result = await DotnetMSBuild("Build", "/t:_IntrospectRazorEmbeddedResources /p:EmbedRazorGenerateSources=true");

            Assert.BuildPassed(result);

            Assert.BuildOutputContainsLine(result, $@"CompileResource: {Path.Combine("Pages", "Index.cshtml")} /Pages/Index.cshtml");
            Assert.BuildOutputContainsLine(result, $@"CompileResource: {Path.Combine("Areas", "Products", "Pages", "_ViewStart.cshtml")} /Areas/Products/Pages/_ViewStart.cshtml");
            Assert.BuildOutputContainsLine(result, $@"CompileResource: {Path.Combine("..", "LinkedDir", "LinkedFile.cshtml")} /LinkedFileOut/LinkedFile.cshtml");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_WithViews_ProducesDepsFileWithCompilationContext()
        {
            var customDefine = "RazorSdkTest";
            var result = await DotnetMSBuild("Build", $"/p:DefineConstants={customDefine}");

            Assert.BuildPassed(result);

            Assert.FileExists(result, OutputPath, "SimpleMvc.deps.json");
            var depsFilePath = Path.Combine(Project.DirectoryPath, OutputPath, "SimpleMvc.deps.json");
            var dependencyContext = ReadDependencyContext(depsFilePath);
            // Pick a couple of libraries and ensure they have some compile references
            var packageReference = dependencyContext.CompileLibraries.First(l => l.Name == "Microsoft.AspNetCore.Html.Abstractions");
            Assert.NotEmpty(packageReference.Assemblies);

            var projectReference = dependencyContext.CompileLibraries.First(l => l.Name == "SimpleMvc");
            Assert.NotEmpty(packageReference.Assemblies);

            Assert.Contains(customDefine, dependencyContext.CompilationOptions.Defines);
        }

        [Fact]
        [InitializeTestProject("ClassLibrary")]
        public async Task Build_CodeGensAssemblyInfoUsingValuesFromProject()
        {
            var razorAssemblyInfoPath = Path.Combine(IntermediateOutputPath, "ClassLibrary.RazorTargetAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build");

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorAssemblyInfoPath);
            Assert.FileContains(result, razorAssemblyInfoPath, "[assembly: System.Reflection.AssemblyCopyrightAttribute(\"© Microsoft\")]");
            Assert.FileContains(result, razorAssemblyInfoPath, "[assembly: System.Reflection.AssemblyProductAttribute(\"Razor Test\")]");
            Assert.FileContains(result, razorAssemblyInfoPath, "[assembly: System.Reflection.AssemblyCompanyAttribute(\"Microsoft\")]");
            Assert.FileContains(result, razorAssemblyInfoPath, "[assembly: System.Reflection.AssemblyTitleAttribute(\"ClassLibrary.Views\")]");
            Assert.FileContains(result, razorAssemblyInfoPath, "[assembly: System.Reflection.AssemblyVersionAttribute(\"1.0.0.0\")]");
            Assert.FileContains(result, razorAssemblyInfoPath, "[assembly: System.Reflection.AssemblyFileVersionAttribute(\"1.0.0.0\")]");
            Assert.FileContains(result, razorAssemblyInfoPath, "[assembly: System.Reflection.AssemblyInformationalVersionAttribute(\"1.0.0\")]");
            Assert.FileContains(result, razorAssemblyInfoPath, "[assembly: System.Reflection.AssemblyDescriptionAttribute(\"ClassLibrary Description\")]");
        }

        [Fact]
        [InitializeTestProject("ClassLibrary")]
        public async Task Build_UsesRazorSpecificAssemblyProperties()
        {
            var razorTargetAssemblyInfo = Path.Combine(IntermediateOutputPath, "ClassLibrary.RazorTargetAssemblyInfo.cs");
            var buildArguments = "/p:RazorAssemblyFileVersion=2.0.0.100 /p:RazorAssemblyInformationalVersion=2.0.0-preview " +
                "/p:RazorAssemblyTitle=MyRazorViews /p:RazorAssemblyVersion=2.0.0 " +
                "/p:RazorAssemblyDescription=MyRazorDescription";
            var result = await DotnetMSBuild("Build", buildArguments);

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorTargetAssemblyInfo);
            Assert.FileContains(result, razorTargetAssemblyInfo, "[assembly: System.Reflection.AssemblyDescriptionAttribute(\"MyRazorDescription\")]");
            Assert.FileContains(result, razorTargetAssemblyInfo, "[assembly: System.Reflection.AssemblyTitleAttribute(\"MyRazorViews\")]");
            Assert.FileContains(result, razorTargetAssemblyInfo, "[assembly: System.Reflection.AssemblyVersionAttribute(\"2.0.0\")]");
            Assert.FileContains(result, razorTargetAssemblyInfo, "[assembly: System.Reflection.AssemblyFileVersionAttribute(\"2.0.0.100\")]");
            Assert.FileContains(result, razorTargetAssemblyInfo, "[assembly: System.Reflection.AssemblyInformationalVersionAttribute(\"2.0.0-preview\")]");
        }

        [Fact]
        [InitializeTestProject("ClassLibrary")]
        public async Task Build_DoesNotGenerateAssemblyInfo_IfGenerateRazorTargetAssemblyInfo_IsSetToFalse()
        {
            var result = await DotnetMSBuild("Build", "/p:GenerateRazorTargetAssemblyInfo=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "ClassLibrary.AssemblyInfo.cs");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "ClassLibrary.RazorTargetAssemblyInfo.cs");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_AddsApplicationPartAttributes()
        {
            var razorAssemblyInfoPath = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorAssemblyInfo.cs");
            var razorTargetAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorTargetAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build");

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorAssemblyInfoPath);
            Assert.FileContains(result, razorAssemblyInfoPath, "[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.RelatedAssemblyAttribute(\"SimpleMvc.Views\")]");

            Assert.FileExists(result, razorTargetAssemblyInfo);
            Assert.FileContains(result, razorTargetAssemblyInfo, "[assembly: System.Reflection.AssemblyTitleAttribute(\"SimpleMvc.Views\")]");
            Assert.FileContains(result, razorTargetAssemblyInfo, "[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.ProvideApplicationPartFactoryAttribute(\"Microsoft.AspNetCore.Mvc.ApplicationParts.CompiledRazorAssemblyApplicationPartFac\"");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_AddsApplicationPartAttributes_WhenEnableDefaultRazorTargetAssemblyInfoAttributes_IsFalse()
        {
            var razorTargetAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorTargetAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:EnableDefaultRazorTargetAssemblyInfoAttributes=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.AssemblyInfo.cs");
            Assert.FileExists(result, razorTargetAssemblyInfo);
            Assert.FileDoesNotContain(result, razorTargetAssemblyInfo, "[assembly: System.Reflection.AssemblyTitleAttribute");
            Assert.FileContains(result, razorTargetAssemblyInfo, "[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.ProvideApplicationPartFactoryAttribute(\"Microsoft.AspNetCore.Mvc.ApplicationParts.CompiledRazorAssemblyApplicationPartFac\"");
        }

        [Fact]
        [InitializeTestProject("ClassLibrary")]
        public async Task Build_UsesSpecifiedApplicationPartFactoryTypeName()
        {
            var razorTargetAssemblyInfo = Path.Combine(IntermediateOutputPath, "ClassLibrary.RazorTargetAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:ProvideApplicationPartFactoryAttributeTypeName=CustomFactory");

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorTargetAssemblyInfo);
            Assert.FileContains(result, razorTargetAssemblyInfo, "[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.ProvideApplicationPartFactoryAttribute(\"CustomFactory\"");
        }

        [Fact]
        [InitializeTestProject("ClassLibrary")]
        public async Task Build_DoesNotGenerateProvideApplicationPartFactoryAttribute_IfGenerateProvideApplicationPartFactoryAttributeIsUnset()
        {
            var razorTargetAssemblyInfo = Path.Combine(IntermediateOutputPath, "ClassLibrary.RazorTargetAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:GenerateProvideApplicationPartFactoryAttribute=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorTargetAssemblyInfo);
            Assert.FileDoesNotContain(result, razorTargetAssemblyInfo, "[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.ProvideApplicationPartFactoryAttribute");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_DoesNotAddRelatedAssemblyPart_IfToolSetIsNotRazorSdk()
        {
            var razorAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:RazorCompileToolSet=MvcPrecompilation");

            Assert.BuildPassed(result);

            Assert.FileDoesNotExist(result, razorAssemblyInfo);
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.RazorTargetAssemblyInfo.cs");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_DoesNotAddRelatedAssemblyPart_IfViewCompilationIsDisabled()
        {
            var razorAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=false /p:RazorCompileOnPublish=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorAssemblyInfo);
            Assert.FileDoesNotContain(result, razorAssemblyInfo, "RelatedAssemblyAttribute");

            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.RazorTargetAssemblyInfo.cs");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_AddsRelatedAssemblyPart_IfCompileOnPublishIsAllowed()
        {
            var razorAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:RazorCompileOnBuild=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorAssemblyInfo);
            Assert.FileContains(result, razorAssemblyInfo, "[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.RelatedAssemblyAttribute(\"SimpleMvc.Views\")]");

            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.RazorTargetAssemblyInfo.cs");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_AddsRelatedAssemblyPart_IfGenerateAssemblyInfoIsFalse()
        {
            var razorAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:GenerateAssemblyInfo=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorAssemblyInfo);
            Assert.FileContains(result, razorAssemblyInfo, "[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.RelatedAssemblyAttribute(\"SimpleMvc.Views\")]");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_WithGenerateRazorHostingAssemblyInfoFalse_DoesNotGenerateHostingAttributes()
        {
            var razorAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:GenerateRazorHostingAssemblyInfo=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorAssemblyInfo);
            Assert.FileDoesNotContain(result, razorAssemblyInfo, "Microsoft.AspNetCore.Razor.Hosting.RazorLanguageVersionAttribute");
            Assert.FileDoesNotContain(result, razorAssemblyInfo, "Microsoft.AspNetCore.Razor.Hosting.RazorConfigurationNameAttribute");
            Assert.FileDoesNotContain(result, razorAssemblyInfo, "Microsoft.AspNetCore.Razor.Hosting.RazorExtensionAssemblyNameAttribute");
            Assert.FileContains(result, razorAssemblyInfo, "[assembly: Microsoft.AspNetCore.Mvc.ApplicationParts.RelatedAssemblyAttribute(\"SimpleMvc.Views\")]");
        }

        [Fact]
        [InitializeTestProject("ClassLibrary")]
        public async Task Build_DoesNotGenerateHostingAttributes_InClassLibraryProjects()
        {
            var razorAssemblyInfo = Path.Combine(IntermediateOutputPath, "ClassLibrary.AssemblyInfo.cs");
            var result = await DotnetMSBuild("Build");

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorAssemblyInfo);
            Assert.FileDoesNotContain(result, razorAssemblyInfo, "Microsoft.AspNetCore.Razor.Hosting.RazorLanguageVersionAttribute");
            Assert.FileDoesNotContain(result, razorAssemblyInfo, "Microsoft.AspNetCore.Razor.Hosting.RazorConfigurationNameAttribute");
            Assert.FileDoesNotContain(result, razorAssemblyInfo, "Microsoft.AspNetCore.Razor.Hosting.RazorExtensionAssemblyNameAttribute");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_GeneratesHostingAttributesByDefault()
        {
            var razorAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.AssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:GenerateRazorHostingAssemblyInfo=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, razorAssemblyInfo);
            Assert.FileDoesNotContain(result, razorAssemblyInfo, "Microsoft.AspNetCore.Razor.Hosting.RazorLanguageVersionAttribute(\"2.1\")");
            Assert.FileDoesNotContain(result, razorAssemblyInfo, "Microsoft.AspNetCore.Razor.Hosting.RazorConfigurationNameAttribute(\"MVC-2-1\")");
        }

        [Fact]
        [InitializeTestProject("SimpleMvcFSharp", language: "F#")]
        public async Task Build_SimpleMvcFSharp_NoopsWithoutFailing()
        {
            var result = await DotnetMSBuild("Build");

            Assert.BuildPassed(result);

            Assert.FileExists(result, OutputPath, "SimpleMvcFSharp.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvcFSharp.pdb");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvcFSharp.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvcFSharp.pdb");

            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvcFSharp.Views.dll");
            Assert.FileDoesNotExist(result, OutputPath, "SimpleMvcFSharp.Views.pdb");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvcFSharp.RazorAssemblyInfo.cs");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvcFSharp.RazorAssemblyInfo.fs");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_WithGenerateRazorAssemblyInfo_False_DoesNotGenerateAssemblyInfo()
        {
            var razorAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:GenerateRazorAssemblyInfo=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");

            Assert.FileDoesNotExist(result, razorAssemblyInfo);
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_WithGenerateRazorTargetAssemblyInfo_False_DoesNotGenerateAssemblyInfo()
        {
            var razorAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorAssemblyInfo.cs");
            var razorTargetAssemblyInfo = Path.Combine(IntermediateOutputPath, "SimpleMvc.RazorTargetAssemblyInfo.cs");
            var result = await DotnetMSBuild("Build", "/p:GenerateRazorTargetAssemblyInfo=false");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");

            Assert.FileExists(result, razorAssemblyInfo);
            Assert.FileDoesNotExist(result, razorTargetAssemblyInfo);
        }

        [Fact]
        [InitializeTestProject("AppWithP2PReference", additionalProjects: new[] { "ClassLibrary", "ClassLibrary2" })]
        public async Task Build_WithP2P_WorksWhenBuildProjectReferencesIsDisabled()
        {
            // Simulates building the same way VS does by setting BuildProjectReferences=false.
            // With this flag, P2P references aren't resolved during GetCopyToOutputDirectoryItems. This test ensures that
            // no Razor work is done in such a scenario and the build succeeds.
            var additionalProjectContent = @"
<ItemGroup>
  <ProjectReference Include=""..\ClassLibrary2\ClassLibrary2.csproj"" />
</ItemGroup>
";
            AddProjectFileContent(additionalProjectContent);

            var result = await DotnetMSBuild(target: default);

            Assert.BuildPassed(result);

            Assert.FileExists(result, OutputPath, "AppWithP2PReference.dll");
            Assert.FileExists(result, OutputPath, "AppWithP2PReference.Views.dll");
            Assert.FileExists(result, OutputPath, "ClassLibrary.dll");
            Assert.FileExists(result, OutputPath, "ClassLibrary.Views.dll");
            Assert.FileExists(result, OutputPath, "ClassLibrary2.dll");
            Assert.FileExists(result, OutputPath, "ClassLibrary2.Views.dll");

            // Force a rebuild of ClassLibrary2 by changing a file
            var class2Path = Path.Combine(Project.SolutionPath, "ClassLibrary2", "Class2.cs");
            File.AppendAllText(class2Path, Environment.NewLine + "// Some changes");

            // dotnet msbuild /p:BuildProjectReferences=false
            result = await DotnetMSBuild(target: default, "/p:BuildProjectReferences=false", suppressRestore: true);

            Assert.BuildPassed(result);
        }

        [Fact]
        [InitializeTestProject("AppWithP2PReference", additionalProjects: new[] { "ClassLibrary", "ClassLibraryMvc21" })]
        public async Task Build_WithP2P_Referencing21Project_Works()
        {
            // Verifies building with different versions of Razor.Tasks works. Loosely modeled after the repro
            // scenario listed in https://github.com/Microsoft/msbuild/issues/3572
            var additionalProjectContent = @"
<ItemGroup>
  <ProjectReference Include=""..\ClassLibraryMvc21\ClassLibraryMvc21.csproj"" />
</ItemGroup>
";
            AddProjectFileContent(additionalProjectContent);

            var result = await DotnetMSBuild(target: default);

            Assert.BuildPassed(result);

            Assert.FileExists(result, OutputPath, "AppWithP2PReference.dll");
            Assert.FileExists(result, OutputPath, "AppWithP2PReference.Views.dll");
            Assert.FileExists(result, OutputPath, "ClassLibrary.dll");
            Assert.FileExists(result, OutputPath, "ClassLibrary.Views.dll");
            Assert.FileExists(result, OutputPath, "ClassLibraryMvc21.dll");
            Assert.FileExists(result, OutputPath, "ClassLibraryMvc21.Views.dll");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_WithStartupObjectSpecified_Works()
        {
            var result = await DotnetMSBuild("Build", $"/p:StartupObject=SimpleMvc.Program");

            Assert.BuildPassed(result);

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");

            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_WithDeterministicFlagSet_OutputsDeterministicViewsAssembly()
        {
            // Build 1
            var result = await DotnetMSBuild("Build", $"/p:Deterministic=true");

            Assert.BuildPassed(result);
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
            var filePath = Path.Combine(result.Project.DirectoryPath, IntermediateOutputPath, "SimpleMvc.Views.dll");
            var firstAssemblyBytes = File.ReadAllBytes(filePath);

            // Build 2
            result = await DotnetMSBuild("Rebuild", $"/p:Deterministic=true");

            Assert.BuildPassed(result);
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
            var secondAssemblyBytes = File.ReadAllBytes(filePath);

            Assert.Equal(firstAssemblyBytes, secondAssemblyBytes);
        }

        [Fact]
        [InitializeTestProject("SimpleMvc21")]
        public async Task Building_NETCoreApp21TargetingProject()
        {
            TargetFramework = "netcoreapp2.1";

            // Build
            var result = await DotnetMSBuild("Build");

            Assert.BuildPassed(result);
            Assert.FileExists(result, OutputPath, "SimpleMvc21.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc21.pdb");
            Assert.FileExists(result, OutputPath, "SimpleMvc21.Views.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc21.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc21")]
        public async Task Building_WorksWhenMultipleRazorConfigurationsArePresent()
        {
            TargetFramework = "netcoreapp2.1";
            AddProjectFileContent(@"
<ItemGroup>
    <RazorConfiguration Include=""MVC-2.1"">
      <Extensions>MVC-2.1;$(CustomRazorExtension)</Extensions>
    </RazorConfiguration>
</ItemGroup>");

            // Build
            var result = await DotnetMSBuild("Build");

            Assert.BuildPassed(result);
            Assert.FileExists(result, OutputPath, "SimpleMvc21.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc21.pdb");
            Assert.FileExists(result, OutputPath, "SimpleMvc21.Views.dll");
            Assert.FileExists(result, OutputPath, "SimpleMvc21.Views.pdb");
        }

        [Fact]
        [InitializeTestProject("SimpleMvc")]
        public async Task Build_WithoutServer_ErrorDuringBuild_DisplaysErrorInMsBuildOutput()
        {
            var result = await DotnetMSBuild(
                "Build",
                "/p:UseRazorBuildServer=false /p:RazorLangVersion=5.0",
                suppressBuildServer: true);

            Assert.BuildFailed(result);
            Assert.BuildOutputContainsLine(
                result,
                $"Invalid option 5.0 for Razor language version --version; must be Latest or a valid version in range {RazorLanguageVersion.Version_1_0} to {RazorLanguageVersion.Latest}.");

            // Compilation failed without creating the views assembly
            Assert.FileExists(result, IntermediateOutputPath, "SimpleMvc.dll");
            Assert.FileDoesNotExist(result, IntermediateOutputPath, "SimpleMvc.Views.dll");
        }

        private static DependencyContext ReadDependencyContext(string depsFilePath)
        {
            var reader = new DependencyContextJsonReader();
            using (var stream = File.OpenRead(depsFilePath))
            {
                return reader.Read(stream);
            }
        }
    }
}
