// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Mvc.Razor.Extensions;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.CSharp;

namespace Microsoft.CodeAnalysis.Razor.ProjectSystem
{
    internal class DefaultProjectSnapshotUpdateListener : ProjectSnapshotUpdateListener
    {
        private readonly Workspace _workspace;

        public DefaultProjectSnapshotUpdateListener(Workspace workspace)
        {
            _workspace = workspace;
        }

        public override void OnProjectUpdated(ProjectSnapshot snapshot)
        {
            try
            {
                var templateEngineFactory = _workspace.Services.GetLanguageServices(RazorLanguage.Name).GetRequiredService<RazorTemplateEngineFactoryService>();

                var engine = RazorEngine.Create(b =>
                {
                    b.Features.Add(new CompilationTagHelperFeature()
                    {
                        Compilation = (CSharpCompilation)snapshot.WorkspaceProject.GetCompilationAsync().Result,
                    });

                    RazorExtensions.Register(b);

                    b.Features.Add(new Microsoft.CodeAnalysis.Razor.DefaultTagHelperDescriptorProvider());
                    b.Features.Add(new ViewComponentTagHelperDescriptorProvider());

                });

                var templateEngine = new MvcRazorTemplateEngine(engine, RazorProject.Create(Path.GetDirectoryName(snapshot.FilePath)));
                templateEngine.Options.ImportsFileName = "_ViewImports.cshtml";

                foreach (var document in snapshot.Documents)
                {
                    var path = "/" + document.SourceFilePath.Replace('\\', '/');

                    var codeDocument = templateEngine.CreateCodeDocument(path);
                    var generated = templateEngine.GenerateCode(codeDocument);

                    using (var stream = File.OpenWrite(Path.Combine(Path.GetDirectoryName(snapshot.FilePath), document.OutputFilePath)))
                    using (var writer = new StreamWriter(stream))
                    {
                        writer.Write(generated.GeneratedCode);
                    }
                }
            }
            catch (Exception)
            {

            }
        }
    }
}
