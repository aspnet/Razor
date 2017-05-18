// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.CodeAnalysis.Text;

namespace Microsoft.CodeAnalysis.Razor
{
    public class WorkspaceRegistrationMonitorProvider
    {
        public WorkspaceRegistrationMonitor GetRegistrationMonitor(SourceTextContainer documentsRoslynBuffer)
        {
            var workspaceRegistration = Workspace.GetWorkspaceRegistration(documentsRoslynBuffer);
            var registrationMonitor = new WorkspaceRegistrationMonitor(workspaceRegistration);

            return registrationMonitor;
        }
    }
}
