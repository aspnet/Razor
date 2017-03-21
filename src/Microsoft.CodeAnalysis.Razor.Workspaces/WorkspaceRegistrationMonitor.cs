// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.CodeAnalysis.Razor
{
    public class WorkspaceRegistrationMonitor
    {
        private readonly WorkspaceRegistration _registration;
        private Workspace _workspace;

        public WorkspaceRegistrationMonitor(WorkspaceRegistration registration)
        {
            _registration = registration;

            _registration.WorkspaceChanged += OnWorkspaceRegistrationChanged;
        }

        public event Action<Workspace> ConnectedToWorkspace;

        public event Action<Workspace> DisconnectedFromWorkspace;

        private void OnWorkspaceRegistrationChanged(object sender, EventArgs e)
        {
            if (_workspace != null)
            {
                DisconnectedFromWorkspace?.Invoke(_workspace);
            }

            if (_registration.Workspace != null)
            {
                _workspace = _registration.Workspace;
                ConnectedToWorkspace?.Invoke(_workspace);
            }
        }
    }
}
