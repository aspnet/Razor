﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal class DefaultProjectSnapshotWorker : ProjectSnapshotWorker
    {
        private readonly ProjectExtensibilityConfigurationFactory _configurationFactory;
        private readonly ForegroundDispatcher _foregroundDispatcher;

        public DefaultProjectSnapshotWorker(
            ForegroundDispatcher foregroundDispatcher,
            ProjectExtensibilityConfigurationFactory configurationFactory)
        {
            if (foregroundDispatcher == null)
            {
                throw new ArgumentNullException(nameof(foregroundDispatcher));
            }

            if (configurationFactory == null)
            {
                throw new ArgumentNullException(nameof(configurationFactory));
            }

            _foregroundDispatcher = foregroundDispatcher;
            _configurationFactory = configurationFactory;
        }

        public override Task ProcessUpdateAsync(ProjectSnapshotUpdateContext update, CancellationToken cancellationToken = default(CancellationToken))
        {
            if (update == null)
            {
                throw new ArgumentNullException(nameof(update));
            }

            // Don't block the main thread
            if (_foregroundDispatcher.IsForegroundThread)
            {
                return Task.Factory.StartNew(ProjectUpdatesCoreAsync, update, CancellationToken.None, TaskCreationOptions.None, _foregroundDispatcher.BackgroundScheduler);
            }

            return ProjectUpdatesCoreAsync(update);
        }

        private async Task ProjectUpdatesCoreAsync(object state)
        {
            var update = (ProjectSnapshotUpdateContext)state;

            // We'll have more things to process here, but for now we're just hardcoding the configuration.

            var configuration = await _configurationFactory.GetConfigurationAsync(update.WorkspaceProject);
            update.Configuration = configuration;
        }
    }
}
