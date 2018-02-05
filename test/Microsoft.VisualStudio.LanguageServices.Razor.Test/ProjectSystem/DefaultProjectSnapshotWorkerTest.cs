// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Moq;
using Xunit;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    public class DefaultProjectSnapshotWorkerTest : ForegroundDispatcherTestBase
    {
        public DefaultProjectSnapshotWorkerTest()
        {
            HostProject = new HostProject("Test1.csproj", FallbackRazorConfiguration.MVC_1_0);

            WorkspaceProject = new AdhocWorkspace().AddProject("Test1", LanguageNames.CSharp);
        }

        private HostProject HostProject { get; }

        private Project WorkspaceProject { get; }


        [ForegroundFact]
        public async Task ProcessUpdateAsync_DoesntBlockForegroundThread()
        {
            // Arrange
            var worker = new TestProjectSnapshotWorker(Dispatcher);

            var context = new ProjectSnapshotUpdateContext(HostProject.FilePath, HostProject, WorkspaceProject, VersionStamp.Default);

            // Act 1 -- We want to verify that this doesn't block the main thread
            var task = worker.ProcessUpdateAsync(context);

            // Assert 1
            //
            // The background task has started when this event is set.
            worker.ProcessingStarted.Wait(50);
            Assert.Equal(TaskStatus.Running, task.Status);

            // Act 2 - Ok let's go
            worker.ProcessingCompleted.Set();
            await task;
        }

        private class TestProjectSnapshotWorker : DefaultProjectSnapshotWorker
        {
            public TestProjectSnapshotWorker(ForegroundDispatcher dispatcher)
                : base(dispatcher)
            {
            }

            public ManualResetEventSlim ProcessingStarted { get; } = new ManualResetEventSlim(initialState: false);

            public ManualResetEventSlim ProcessingCompleted { get; } = new ManualResetEventSlim(initialState: false);

            protected override void OnProcessingUpdate()
            {
                ProcessingStarted.Set();

                base.OnProcessingUpdate();

                ProcessingCompleted.Wait();
            }
        }
    }
}
