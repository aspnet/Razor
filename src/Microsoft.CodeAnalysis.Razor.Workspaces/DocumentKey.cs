// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;
using Microsoft.Extensions.Internal;

namespace Microsoft.CodeAnalysis.Razor
{
    public struct DocumentKey : IEquatable<DocumentKey>
    {
        public DocumentKey(ProjectId projectId, string documentFilePath)
        {
            ProjectId = projectId;
            DocumentFilePath = documentFilePath;
        }

        public ProjectId ProjectId { get; }

        public string DocumentFilePath { get; }

        public bool Equals(DocumentKey other)
        {
            return
                ProjectId == other.ProjectId &&
                FilePathComparer.Instance.Equals(DocumentFilePath, other.DocumentFilePath);
        }

        public override bool Equals(object obj)
        {
            return obj is DocumentKey key ? Equals(key) : false;
        }

        public override int GetHashCode()
        {
            var hash = new HashCodeCombiner();
            hash.Add(ProjectId);
            hash.Add(DocumentFilePath, FilePathComparer.Instance);
            return hash;
        }
    }
}
