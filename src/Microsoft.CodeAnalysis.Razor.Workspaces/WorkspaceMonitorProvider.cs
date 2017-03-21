// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.CodeAnalysis.Razor
{
    public class WorkspaceMonitorProvider
    {
        private readonly List<WorkspaceMonitorEntry> _workspaceMonitorEntries;

        public WorkspaceMonitorProvider()
        {
            _workspaceMonitorEntries = new List<WorkspaceMonitorEntry>();
        }

        public WorkspaceMonitor GetWorkspaceMonitor(Workspace workspace)
        {
            var entry = GetEntry(workspace);

            entry.MonitorRequestCount++;

            return entry.Monitor;
        }

        public WorkspaceMonitor StopMonitoring(Workspace workspace)
        {
            var entry = GetEntry(workspace);

            entry.MonitorRequestCount--;

            if (entry.MonitorRequestCount == 0)
            {
                _workspaceMonitorEntries.Remove(entry);
            }

            return entry.Monitor;
        }

        private WorkspaceMonitorEntry GetEntry(Workspace workspace)
        {
            var entry = _workspaceMonitorEntries.FirstOrDefault(e => e.Workspace == workspace);
            if (entry == null)
            {
                entry = new WorkspaceMonitorEntry(workspace);
                _workspaceMonitorEntries.Add(entry);
            }

            return entry;
        }

        private class WorkspaceMonitorEntry
        {
            public WorkspaceMonitorEntry(Workspace workspace)
            {
                Workspace = workspace;
                Monitor = new WorkspaceMonitor(workspace);
            }

            public int MonitorRequestCount { get; set; }

            public Workspace Workspace { get; }

            public WorkspaceMonitor Monitor { get; }
        }
    }
}
