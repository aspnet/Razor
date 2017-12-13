﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Microsoft.VisualStudio.LanguageServices.Razor
{
    internal class DefaultTagHelperResolver : TagHelperResolver
    {
        private readonly ErrorReporter _errorReporter;
        private readonly Workspace _workspace;

        public DefaultTagHelperResolver(ErrorReporter errorReporter, Workspace workspace)
        {
            _errorReporter = errorReporter;
            _workspace = workspace;
        }

        public override async Task<TagHelperResolutionResult> GetTagHelpersAsync(Project project, CancellationToken cancellationToken)
        {
            if (project == null)
            {
                throw new ArgumentNullException(nameof(project));
            }

            try
            {
                TagHelperResolutionResult result;

                // We're being defensive here because the OOP host can return null for the client/session/operation
                // when it's disconnected (user stops the process).
                var client = await RazorLanguageServiceClientFactory.CreateAsync(_workspace, cancellationToken);
                if (client != null)
                {
                    using (var session = await client.CreateSessionAsync(project.Solution))
                    {
                        if (session != null)
                        {
                            var jsonObject = await session.InvokeAsync<JObject>(
                                "GetTagHelpersAsync",
                                new object[] { project.Id.Id, "Foo", },
                                cancellationToken).ConfigureAwait(false);

                            result = GetTagHelperResolutionResult(jsonObject);

                            if (result != null)
                            {
                                return result;
                            }
                        }
                    }
                }

                // The OOP host is turned off, so let's do this in process.
                var compilation = await project.GetCompilationAsync(cancellationToken).ConfigureAwait(false);
                result = GetTagHelpers(compilation);
                return result;
            }
            catch (Exception exception)
            {
                _errorReporter.ReportError(exception, project);

                throw new InvalidOperationException(
                    Resources.FormatUnexpectedException(
                        typeof(DefaultTagHelperResolver).FullName,
                        nameof(GetTagHelpersAsync)),
                    exception);
            }
        }

        public override TagHelperResolutionResult GetTagHelpers(Compilation compilation)
        {
            var descriptors = new List<TagHelperDescriptor>();

            var providers = new ITagHelperDescriptorProvider[]
            {
                new DefaultTagHelperDescriptorProvider() { DesignTime = true, },
                new ViewComponentTagHelperDescriptorProvider(),
            };

            var results = new List<TagHelperDescriptor>();
            var context = TagHelperDescriptorProviderContext.Create(results);
            context.SetCompilation(compilation);

            for (var i = 0; i < providers.Length; i++)
            {
                var provider = providers[i];
                provider.Execute(context);
            }

            var diagnostics = new List<RazorDiagnostic>();
            var resolutionResult = new TagHelperResolutionResult(results, diagnostics);

            return resolutionResult;
        }

        private TagHelperResolutionResult GetTagHelperResolutionResult(JObject jsonObject)
        {
            var serializer = new JsonSerializer();
            serializer.Converters.Add(TagHelperDescriptorJsonConverter.Instance);
            serializer.Converters.Add(RazorDiagnosticJsonConverter.Instance);

            using (var reader = jsonObject.CreateReader())
            {
                return serializer.Deserialize<TagHelperResolutionResult>(reader);
            }
        }
    }
}
