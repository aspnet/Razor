// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal abstract class ProjectSnapshot
    {
        public abstract ProjectExtensibilityConfiguration Configuration { get; }

        public abstract IReadOnlyList<DocumentSnapshot> Documents { get; }

        public abstract string FilePath { get; }

        public abstract bool IsInitialized { get; }

        public abstract bool IsUnloaded { get; }
        
        public abstract Project WorkspaceProject { get; }
    }
}
