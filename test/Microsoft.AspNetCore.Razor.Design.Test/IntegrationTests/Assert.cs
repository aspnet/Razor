// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Razor.Design.IntegrationTests
{
    internal class Assert : Xunit.Assert
    {
        // Matches `{filename}: error {code}: {message} [{project}]
        // See https://stackoverflow.com/questions/3441452/msbuild-and-ignorestandarderrorwarningformat/5180353#5180353
        private static readonly Regex ErrorRegex = new Regex(@"^(?'location'.+): error (?'errorcode'[A-Z0-9]+): (?'message'.+) \[(?'project'.+)\]$");

        public static void BuildPassed(MSBuildResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (result.ExitCode != 0)
            {
                throw new BuildFailedException(result);
            }
        }

        public static void BuildError(MSBuildResult result, string errorCode, string location = null)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            // We don't really need to search line by line, I'm doing this so that it's possible/easy to debug.
            var lines = result.Output.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i];
                var match = ErrorRegex.Match(line);
                if (match.Success)
                {
                    if (match.Groups["errorcode"].Value != errorCode)
                    {
                        continue;
                    }

                    if (location != null && match.Groups["location"].Value.Trim() != location)
                    {
                        continue;
                    }

                    // This is a match
                    return;
                }
            }

            throw new BuildErrorMissingException(result, errorCode, location);
        }

        public static void BuildFailed(MSBuildResult result)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            };

            if (result.ExitCode == 0)
            {
                throw new BuildPassedException(result);
            }
        }

        public static void BuildOutputContainsLine(MSBuildResult result, string match)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (match == null)
            {
                throw new ArgumentNullException(nameof(match));
            }

            // We don't really need to search line by line, I'm doing this so that it's possible/easy to debug.
            var lines = result.Output.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            for (var i = 0; i < lines.Length; i++)
            {
                var line = lines[i].Trim();
                if (line == match)
                {
                    return;
                }
            }

            throw new BuildOutputMissingException(result, match);
        }

        public static void FileExists(MSBuildResult result, params string[] paths)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var filePath = Path.Combine(result.Project.DirectoryPath, Path.Combine(paths));
            if (!File.Exists(filePath))
            {
                throw new FileMissingException(result, filePath);
            }
        }

        public static void FileCountEquals(MSBuildResult result, int expected, string directoryPath, string searchPattern)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (directoryPath == null)
            {
                throw new ArgumentNullException(nameof(directoryPath));
            }

            if (searchPattern == null)
            {
                throw new ArgumentNullException(nameof(searchPattern));
            }

            directoryPath = Path.Combine(result.Project.DirectoryPath, directoryPath);

            if (Directory.Exists(directoryPath))
            {
                var files = Directory.GetFiles(directoryPath, searchPattern, SearchOption.AllDirectories);
                if (files.Length != expected)
                {
                    throw new FileCountException(result, expected, directoryPath, searchPattern, files);
                }
            }
            else if (expected > 0)
            {
                // directory doesn't exist, that's OK if we expected to find nothing.
                throw new FileCountException(result, expected, directoryPath, searchPattern, Array.Empty<string>());
            }
        }

        public static void FileDoesNotExist(MSBuildResult result, params string[] paths)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            var filePath = Path.Combine(result.Project.DirectoryPath, Path.Combine(paths));
            if (File.Exists(filePath))
            {
                throw new FileFoundException(result, filePath);
            }
        }

        public static void NuspecContains(MSBuildResult result, string nuspecPath, string expected)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (nuspecPath == null)
            {
                throw new ArgumentNullException(nameof(nuspecPath));
            }

            if (expected == null)
            {
                throw new ArgumentNullException(nameof(expected));
            }

            nuspecPath = Path.Combine(result.Project.DirectoryPath, nuspecPath);
            FileExists(result, nuspecPath);

            var content = File.ReadAllText(nuspecPath);
            if (!content.Contains(expected))
            {
                throw new NuspecException(result, nuspecPath, content, expected);
            }
        }

        public static void NupkgContains(MSBuildResult result, string nupkgPath, string filePath)
        {
            if (result == null)
            {
                throw new ArgumentNullException(nameof(result));
            }

            if (nupkgPath == null)
            {
                throw new ArgumentNullException(nameof(nupkgPath));
            }

            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            nupkgPath = Path.Combine(result.Project.DirectoryPath, nupkgPath);
            FileExists(result, nupkgPath);

            var unzipped = Path.Combine(result.Project.DirectoryPath, "nupkg");
            ZipFile.ExtractToDirectory(nupkgPath, unzipped);

            if (!File.Exists(Path.Combine(unzipped, filePath)))
            {
                throw new NupkgFileMissingException(result, nupkgPath, filePath);
            }
        }

        private abstract class MSBuildXunitException : Xunit.Sdk.XunitException
        {
            protected MSBuildXunitException(MSBuildResult result)
            {
                Result = result;
            }

            protected abstract string Heading { get; }

            public MSBuildResult Result { get; }

            public override string Message
            {
                get
                {
                    var message = new StringBuilder();
                    message.AppendLine(Heading);
                    message.Append(Result.FileName);
                    message.Append(" ");
                    message.Append(Result.Arguments);
                    message.AppendLine();
                    message.AppendLine();
                    message.Append(Result.Output);
                    return message.ToString();
                }
            }
        }

        private class BuildErrorMissingException : MSBuildXunitException
        {
            public BuildErrorMissingException(MSBuildResult result, string errorCode, string location)
                : base(result)
            {
                ErrorCode = errorCode;
                Location = location;
            }

            public string ErrorCode { get; }

            public string Location { get; }

            protected override string Heading
            {
                get
                {
                    return
                        $"Error code '{ErrorCode}' was not found." + Environment.NewLine +
                        $"Looking for '{Location ?? ".*"}: error {ErrorCode}: .*'";
                }
            }
        }


        private class BuildFailedException : MSBuildXunitException
        {
            public BuildFailedException(MSBuildResult result)
                : base(result)
            {
            }

            protected override string Heading => "Build failed.";
        }

        private class BuildPassedException : MSBuildXunitException
        {
            public BuildPassedException(MSBuildResult result)
                : base(result)
            {
            }

            protected override string Heading => "Build should have failed, but it passed.";
        }

        private class BuildOutputMissingException : MSBuildXunitException
        {
            public BuildOutputMissingException(MSBuildResult result, string match)
                : base(result)
            {
                Match = match;
            }

            public string Match { get; }

            protected override string Heading => $"Build did not contain the line: '{Match}'.";
        }

        private class FileMissingException : MSBuildXunitException
        {
            public FileMissingException(MSBuildResult result, string filePath)
                : base(result)
            {
                FilePath = filePath;
            }

            public string FilePath { get; }

            protected override string Heading => $"File: '{FilePath}' was not found.";
        }

        private class FileCountException : MSBuildXunitException
        {
            public FileCountException(MSBuildResult result, int expected, string directoryPath, string searchPattern, string[] files)
                : base(result)
            {
                Expected = expected;
                DirectoryPath = directoryPath;
                SearchPattern = searchPattern;
                Files = files;
            }

            public string DirectoryPath { get; }

            public int Expected { get; }

            public string[] Files { get; }

            public string SearchPattern { get; }

            protected override string Heading
            {
                get
                {
                    var heading = new StringBuilder();
                    heading.AppendLine($"Expected {Expected} files matching {SearchPattern} in {DirectoryPath}, found {Files.Length}.");

                    if (Files.Length > 0)
                    {
                        heading.AppendLine("Files:");

                        foreach (var file in Files)
                        {
                            heading.Append("\t");
                            heading.Append(file);
                        }

                        heading.AppendLine();
                    }

                    return heading.ToString();
                }
            }
        }

        private class FileFoundException : MSBuildXunitException
        {
            public FileFoundException(MSBuildResult result, string filePath)
                : base(result)
            {
                FilePath = filePath;
            }

            public string FilePath { get; }

            protected override string Heading => $"File: '{FilePath}' was found, but should not exist.";
        }

        private class NuspecException : MSBuildXunitException
        {
            public NuspecException(MSBuildResult result, string filePath, string content, string expected)
                : base(result)
            {
                FilePath = filePath;
                Content = content;
                Expected = expected;
            }

            public string Content { get; }

            public string Expected { get; }

            public string FilePath { get; }

            protected override string Heading
            {
                get
                {
                    return 
                        $"nuspec: '{FilePath}' did not contain the expected content." + Environment.NewLine +
                        Environment.NewLine +
                        $"expected: {Expected}" + Environment.NewLine +
                        Environment.NewLine +
                        $"actual: {Content}";
                }
            }
        }

        private class NupkgFileMissingException : MSBuildXunitException
        {
            public NupkgFileMissingException(MSBuildResult result, string nupkgPath, string filePath)
                : base(result)
            {
                NupkgPath = nupkgPath;
                FilePath = filePath;
            }

            public string FilePath { get; }

            public string NupkgPath { get; }

            protected override string Heading => $"File: '{FilePath}' was not found was not found in {NupkgPath}.";
        }
    }
}
