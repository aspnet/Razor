﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal abstract class DocumentSnapshot
    {
        public abstract bool IsEphemeral { get; }

        public abstract string SourceFilePath { get; }

        public abstract string OutputFilePath { get; }
    }
}
