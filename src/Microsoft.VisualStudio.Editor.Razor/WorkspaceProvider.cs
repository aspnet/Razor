// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;

namespace Microsoft.VisualStudio.Editor.Razor
{
    internal abstract class WorkspaceProvider
    {
        public abstract Workspace GetWorkspace(ITextView textView);

        public abstract Workspace GetWorkspace(ITextBuffer textView);
    }
}
