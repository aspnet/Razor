// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class VirtualRazorProjectFileSystem : RazorProjectFileSystem
    {
        private readonly DirectoryNode Root = new DirectoryNode("/", "/");

        public override IEnumerable<RazorProjectItem> EnumerateItems(string basePath)
        {
            basePath = NormalizeAndEnsureValidPath(basePath);
            var directory = Root.GetDirectory(basePath);
            return directory?.EnumerateItems() ?? Enumerable.Empty<RazorProjectItem>();
        }

        public override RazorProjectItem GetItem(string path)
        {
            path = NormalizeAndEnsureValidPath(path);
            return Root.GetItem(path) ?? new NotFoundProjectItem(string.Empty, path);
        }

        public void Add(RazorProjectItem projectItem)
        {
            var filePath = NormalizeAndEnsureValidPath(projectItem.FilePath);
            Root.AddItem(new FileNode(filePath, projectItem));
        }

        private class DirectoryNode
        {
            public DirectoryNode(string path, string name)
            {
                Path = path;
                Name = name;
            }

            public string Path { get; }

            public string Name { get; }

            public List<DirectoryNode> Directories { get; } = new List<DirectoryNode>();

            public List<FileNode> Files { get; } = new List<FileNode>();

            public void AddItem(FileNode fileNode)
            {
                var filePath = fileNode.FilePath;
                if (!filePath.StartsWith(Path, StringComparison.OrdinalIgnoreCase))
                {
                    var message = Resources.FormatVirtualFileSystem_FileDoesNotBelongToDirectory(fileNode.FilePath, Path);
                    throw new InvalidOperationException(message);
                }

                // Look for the first / that appears in the path after the current directory path.
                var directoryPath = GetDirectoryPath(filePath);
                var directory = GetOrAddDirectory(this, directoryPath, createIfNotExists: true);
                Debug.Assert(directory != null);
                directory.Files.Add(fileNode);
            }

            public DirectoryNode GetDirectory(string path)
            {
                if (!path.StartsWith(Path, StringComparison.OrdinalIgnoreCase))
                {
                    var message = Resources.FormatVirtualFileSystem_FileDoesNotBelongToDirectory(path, Path);
                    throw new InvalidOperationException(message);
                }

                return GetOrAddDirectory(this, path);
            }

            public IEnumerable<RazorProjectItem> EnumerateItems()
            {
                foreach (var file in Files)
                {
                    yield return file.ProjectItem;
                }

                foreach (var directory in Directories)
                {
                    foreach (var file in directory.EnumerateItems())
                    {
                        yield return file;
                    }
                }
            }

            public RazorProjectItem GetItem(string path)
            {
                if (!path.StartsWith(Path, StringComparison.OrdinalIgnoreCase))
                {
                    throw new InvalidOperationException(Resources.FormatVirtualFileSystem_FileDoesNotBelongToDirectory(path, Path));
                }

                var directoryPath = GetDirectoryPath(path);
                var directory = GetOrAddDirectory(this, directoryPath);
                if (directory == null)
                {
                    return null;
                }

                foreach (var file in directory.Files)
                {
                    var filePath = file.FilePath;
                    var directoryLength = directory.Path.Length;

                    if (string.Compare(path, directoryLength, filePath, directoryLength, path.Length - directoryLength, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return file.ProjectItem;
                    }
                }

                return null;
            }

            private string GetDirectoryPath(string path)
            {
                // /dir1/dir2/file.cshtml -> /dir1/dir2/
                var fileNameIndex = path.LastIndexOf('/');
                if (fileNameIndex == -1)
                {
                    return path;
                }

                return path.Substring(0, fileNameIndex + 1);
            }

            private static DirectoryNode GetOrAddDirectory(
                DirectoryNode directory,
                string path,
                bool createIfNotExists = false)
            {
                Debug.Assert(!string.IsNullOrEmpty(path));
                if (path[path.Length - 1] != '/')
                {
                    path += '/';
                }

                int index;
                while ((index = path.IndexOf('/', directory.Path.Length)) != -1 && index != path.Length)
                {
                    var subDirectory = FindSubDirectory(directory, path);

                    if (subDirectory == null)
                    {
                        if (createIfNotExists)
                        {
                            var directoryPath = path.Substring(0, index + 1); // + 1 to include trailing slash
                            var directoryName = directoryPath.Substring(directory.Path.Length, directoryPath.Length - directory.Path.Length - 1);
                            subDirectory = new DirectoryNode(directoryPath, directoryName);
                            directory.Directories.Add(subDirectory);

                        }
                        else
                        {
                            return null;
                        }
                    }

                    directory = subDirectory;
                }

                return directory;
            }

            private static DirectoryNode FindSubDirectory(DirectoryNode parentDirectory, string path)
            {
                for (var i = 0; i < parentDirectory.Directories.Count; i++)
                {
                    var currentDirectory = parentDirectory.Directories[i];
                    var directoryName = currentDirectory.Name;
                    if (string.Compare(path, parentDirectory.Path.Length, directoryName, 0, directoryName.Length, StringComparison.OrdinalIgnoreCase) == 0)
                    {
                        return currentDirectory;
                    }
                }

                return null;
            }
        }

        private struct FileNode
        {
            public FileNode(string filePath, RazorProjectItem projectItem)
            {
                FilePath = filePath;
                ProjectItem = projectItem;
            }

            public string FilePath { get; }

            public RazorProjectItem ProjectItem { get; }
        }
    }
}
