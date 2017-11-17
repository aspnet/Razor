// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.VisualStudio.Editor.Razor;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    internal abstract class VisualStudioOpenDocumentManager
    {
        public abstract IReadOnlyList<VisualStudioDocumentTracker> OpenDocuments { get; }

        public abstract void AddDocument(VisualStudioDocumentTracker tracker);

        public abstract void RemoveDocument(VisualStudioDocumentTracker tracker);
    }
}
