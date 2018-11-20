// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.ComponentModel.Composition;
using Microsoft.CodeAnalysis;
using Microsoft.VisualStudio.Cascade.Contracts;
using Microsoft.VisualStudio.Editor.Razor;
using Microsoft.VisualStudio.Text;

namespace Microsoft.VisualStudio.LiveShare.Razor.Guest
{
    [Export(typeof(LiveShareWorkspaceProvider))]
    internal class GuestWorkspaceProvider : LiveShareWorkspaceProvider
    {
        private readonly IVsRemoteWorkspaceManager _remoteWorkspaceManager;
        private readonly IRemoteLanguageServiceWorkspaceHost _remoteLanguageServiceWorkspaceHost;

        [ImportingConstructor]
        public GuestWorkspaceProvider(IVsRemoteWorkspaceManager remoteWorkspaceManager, IRemoteLanguageServiceWorkspaceHost remoteLanguageServiceWorkspaceHost)
        {
            if (remoteWorkspaceManager == null)
            {
                throw new ArgumentNullException(nameof(remoteWorkspaceManager));
            }

            if (remoteLanguageServiceWorkspaceHost == null)
            {
                throw new ArgumentNullException(nameof(remoteLanguageServiceWorkspaceHost));
            }

            _remoteWorkspaceManager = remoteWorkspaceManager;
            _remoteLanguageServiceWorkspaceHost = remoteLanguageServiceWorkspaceHost;
        }

        public override bool TryGetWorkspace(ITextBuffer textBuffer, out Workspace workspace)
        {
            if (_remoteWorkspaceManager.IsRemoteSession)
            {
                workspace = _remoteLanguageServiceWorkspaceHost.Workspace;
                return true;
            }

            workspace = null;
            return false;
        }
    }
}
