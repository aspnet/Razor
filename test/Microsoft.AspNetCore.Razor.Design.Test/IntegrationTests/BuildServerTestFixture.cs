﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Threading;
using Microsoft.AspNetCore.Razor.Tools;
using Microsoft.CodeAnalysis;
using Moq;

namespace Microsoft.AspNetCore.Razor.Design.IntegrationTests
{
    public class BuildServerTestFixture : IDisposable
    {
        private static readonly TimeSpan _defaultShutdownTimeout = TimeSpan.FromSeconds(30);

        public BuildServerTestFixture()
        {
            PipeName = Guid.NewGuid().ToString();
            Console.Out.WriteLine($"Creating server with pipe {PipeName}.");

            if (!ServerConnection.TryCreateServerCore(Environment.CurrentDirectory, PipeName))
            {
                throw new InvalidOperationException($"Failed to start the build server at pipe {PipeName}.");
            }
        }

        public string PipeName { get; }

        public void Dispose()
        {
            // Shutdown the build server.
            using (var cts = new CancellationTokenSource(_defaultShutdownTimeout))
            {
                var writer = new StringWriter();

                cts.Token.Register(() =>
                {
                    var output = writer.ToString();
                    throw new TimeoutException($"Shutting down the build server at pipe {PipeName} took longer than expected.{Environment.NewLine}Output: {output}.");
                });

                var application = new Application(cts.Token, Mock.Of<ExtensionAssemblyLoader>(), Mock.Of<ExtensionDependencyChecker>(), (path, properties) => Mock.Of<PortableExecutableReference>())
                {
                    //Out = writer,
                    //Error = writer,
                };
                var exitCode = application.Execute("shutdown", "-w", "-p", PipeName);
                if (exitCode != 0)
                {
                    var output = writer.ToString();
                    throw new InvalidOperationException(
                        $"Build server at pipe {PipeName} failed to shutdown with exit code {exitCode}. Output: {output}");
                }
            }
        }

        private static string RecursiveFind(string path, string start)
        {
            var test = Path.Combine(start, path);
            if (File.Exists(test))
            {
                return start;
            }
            else
            {
                return RecursiveFind(path, new DirectoryInfo(start).Parent.FullName);
            }
        }
    }
}
