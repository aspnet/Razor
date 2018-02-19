﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;

namespace Microsoft.CodeAnalysis.Remote.Razor
{
    internal class RazorLanguageService : RazorServiceBase
    {
        public RazorLanguageService(Stream stream, IServiceProvider serviceProvider)
            : base(stream, serviceProvider)
        {
        }

        public async Task<TagHelperResolutionResult> GetTagHelpersAsync(ProjectSnapshotHandle projectHandle, string factoryTypeName, CancellationToken cancellationToken = default)
        {
            var project = await GetProjectSnapshotAsync(projectHandle, cancellationToken).ConfigureAwait(false);

            return await RazorServices.TagHelperResolver.GetTagHelpersAsync(project, factoryTypeName, cancellationToken);
        }

        public Task<IEnumerable<DirectiveDescriptor>> GetDirectivesAsync(Guid projectIdBytes, string projectDebugName, CancellationToken cancellationToken = default(CancellationToken))
        {
            var projectId = ProjectId.CreateFromSerialized(projectIdBytes, projectDebugName);

            var engine = RazorEngine.Create();
            var directives = engine.Features.OfType<IRazorDirectiveFeature>().FirstOrDefault()?.Directives;
            return Task.FromResult(directives ?? Enumerable.Empty<DirectiveDescriptor>());
        }

        public Task<GeneratedDocument> GenerateDocumentAsync(Guid projectIdBytes, string projectDebugName, string filePath, string text, CancellationToken cancellationToken = default(CancellationToken))
        {
            var projectId = ProjectId.CreateFromSerialized(projectIdBytes, projectDebugName);

            var engine = RazorEngine.Create();

            RazorSourceDocument source;
            using (var stream = new MemoryStream())
            {
                var bytes = Encoding.UTF8.GetBytes(text);
                stream.Write(bytes, 0, bytes.Length);

                stream.Seek(0L, SeekOrigin.Begin);
                source = RazorSourceDocument.ReadFrom(stream, filePath, Encoding.UTF8);
            }

            var code = RazorCodeDocument.Create(source);
            engine.Process(code);

            var csharp = code.GetCSharpDocument();
            if (csharp == null)
            {
                throw new InvalidOperationException();
            }

            return Task.FromResult(new GeneratedDocument() { Text = csharp.GeneratedCode, });
        }
    }
}
