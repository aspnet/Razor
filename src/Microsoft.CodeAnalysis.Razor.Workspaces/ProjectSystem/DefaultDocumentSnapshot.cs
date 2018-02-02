// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal class DefaultDocumentSnapshot : DocumentSnapshot
    {
        private readonly HostDocument _inner;

        public DefaultDocumentSnapshot(HostDocument inner)
        {
            _inner = inner;
        }

        public override string SourceFilePath => _inner.FilePath;

        public override string OutputFilePath => _inner.GeneratedFilePath;

        public override bool IsEphemeral => true;
    }
}
