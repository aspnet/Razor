﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using Microsoft.AspNetCore.Testing;
using Moq;

namespace Microsoft.AspNetCore.Razor.Design.IntegrationTests
{
    internal class ProjectDirectory : IDisposable
    {
#if PRESERVE_WORKING_DIRECTORY
        public bool PreserveWorkingDirectory { get; set; } = true;
#else
        public bool PreserveWorkingDirectory { get; set; }
#endif

        public static ProjectDirectory Create(string projectName, string[] additionalProjects)
        {
            var destinationPath = Path.Combine(Path.GetTempPath(), "Razor", Path.GetRandomFileName());
            Directory.CreateDirectory(destinationPath);

            try
            {
                if (Directory.EnumerateFiles(destinationPath).Any())
                {
                    throw new InvalidOperationException($"{destinationPath} should be empty");
                }

                var solutionRoot = TestPathUtilities.GetSolutionRootDirectory("Razor");
                if (solutionRoot == null)
                {
                    throw new InvalidOperationException("Could not find solution root.");
                }

                var binariesRoot = Path.GetDirectoryName(typeof(ProjectDirectory).Assembly.Location);

                foreach (var project in new string[] { projectName, }.Concat(additionalProjects))
                {
                    var projectRoot = Path.Combine(solutionRoot, "test", "testapps", project);
                    if (!Directory.Exists(projectRoot))
                    {
                        throw new InvalidOperationException($"Could not find project at '{projectRoot}'");
                    }

                    var projectDestination = Path.Combine(destinationPath, project);
                    Directory.CreateDirectory(projectDestination);
                    CopyDirectory(new DirectoryInfo(projectRoot), new DirectoryInfo(projectDestination));
                    CreateDirectoryProps(projectRoot, binariesRoot, destinationPath);
                    CreateDirectoryTargets(destinationPath);
                }
                
                CopyGlobalJson(solutionRoot, destinationPath);

                return new ProjectDirectory(destinationPath, Path.Combine(destinationPath, projectName));
            }
            catch
            {
                CleanupDirectory(destinationPath);
                throw;
            }

            void CopyDirectory(DirectoryInfo source, DirectoryInfo destination)
            {
                foreach (var directory in source.EnumerateDirectories())
                {
                    if (directory.Name == "bin" || directory.Name == "obj")
                    {
                        // Just in case someone has opened the project in an IDE or built it. We don't want to copy
                        // these folders.
                        continue;
                    }

                    var created = destination.CreateSubdirectory(directory.Name);
                    CopyDirectory(directory, created);
                }

                foreach (var file in source.EnumerateFiles())
                {
                    file.CopyTo(Path.Combine(destination.FullName, file.Name));
                }
            }

            void CreateDirectoryProps(string originalProjectRoot, string binariesRoot, string projectRoot)
            {
#if DEBUG
                var configuration = "Debug";
#elif RELEASE
                var configuration = "Release";
#else
#error Unknown Configuration
#endif

                var text = $@"
<Project>
  <Import Project=""{originalProjectRoot}\..\..\..\src\Microsoft.AspNetCore.Razor.Sdk\SDK\Sdk.props""/>
  <PropertyGroup>
    <OriginalProjectRoot>{originalProjectRoot}</OriginalProjectRoot>
    <BinariesRoot>{binariesRoot}</BinariesRoot>
    <_RazorMSBuildRoot>$(OriginalProjectRoot)\..\..\..\src\Microsoft.AspNetCore.Razor.Design\bin\{configuration}\netstandard2.0\</_RazorMSBuildRoot>
  </PropertyGroup>
  <Import Project=""$(OriginalProjectRoot)\..\..\..\src\Microsoft.AspNetCore.Razor.Design\build\netstandard2.0\Microsoft.AspNetCore.Razor.Design.props""/>
</Project>
";
                File.WriteAllText(Path.Combine(projectRoot, "Directory.Build.props"), text);
            }

            void CreateDirectoryTargets(string projectRoot)
            {
                var text = $@"
<Project>
  <Import Project=""$(OriginalProjectRoot)\..\..\..\src\Microsoft.AspNetCore.Razor.Sdk\SDK\Sdk.targets""/>
  <Import Project=""$(OriginalProjectRoot)\..\..\..\src\Microsoft.AspNetCore.Razor.Design\build\netstandard2.0\Microsoft.AspNetCore.Razor.Design.targets""/>
</Project>
";
                File.WriteAllText(Path.Combine(projectRoot, "Directory.Build.targets"), text);
            }

            void CopyGlobalJson(string solutionRoot, string projectRoot)
            {
                var srcGlobalJson = Path.Combine(solutionRoot, "global.json");
                if (!File.Exists(srcGlobalJson))
                {
                    throw new InvalidOperationException("global.json at the root of the repository could not be found. Run './build /t:Noop' at the repository root and re-run these tests.");
                }

                var destinationGlobalJson = Path.Combine(projectRoot, "global.json");
                File.Copy(srcGlobalJson, destinationGlobalJson);
            }
        }

        private ProjectDirectory(string solutionPath, string directoryPath)
        {
            SolutionPath = solutionPath;
            DirectoryPath = directoryPath;
        }

        public string DirectoryPath { get; }

        public string SolutionPath { get; }

        public void Dispose()
        {
            if (PreserveWorkingDirectory)
            {
                Console.WriteLine($"Skipping deletion of working directory {SolutionPath}");
            }
            else
            {
                CleanupDirectory(SolutionPath);
            }
        }

        private static void CleanupDirectory(string filePath)
        {
            var tries = 5;
            var sleep = TimeSpan.FromSeconds(3);

            for (var i = 0; i < tries; i++)
            {
                try
                {
                    Directory.Delete(filePath, recursive: true);
                    return;
                }
                catch when (i < tries - 1)
                {
                    Console.WriteLine($"Failed to delete directory {filePath}, trying again.");
                    Thread.Sleep(sleep);
                }
            }
        }
    }
}
