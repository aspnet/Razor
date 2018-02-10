﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.CodeAnalysis.Razor.ProjectSystem;
using Mvc1_X = Microsoft.AspNetCore.Mvc.Razor.Extensions.Version1_X;
using MvcLatest = Microsoft.AspNetCore.Mvc.Razor.Extensions;

namespace Microsoft.VisualStudio.Editor.Razor
{
    internal class DefaultTemplateEngineFactoryService : RazorTemplateEngineFactoryService
    {
        private readonly static RazorConfiguration DefaultConfiguration = FallbackRazorConfiguration.MVC_2_0;

        private readonly ProjectSnapshotManager _projectManager;

        public DefaultTemplateEngineFactoryService(ProjectSnapshotManager projectManager)
        {
            if (projectManager == null)
            {
                throw new ArgumentNullException(nameof(projectManager));
            }

            _projectManager = projectManager;
        }

        public override RazorTemplateEngine Create(string projectPath, Action<IRazorEngineBuilder> configure)
        {
            if (projectPath == null)
            {
                throw new ArgumentNullException(nameof(projectPath));
            }

            // In 15.5 we expect projectPath to be a directory, NOT the path to the csproj.
            var project = FindProject(projectPath);
            var configuration = project?.Configuration ?? DefaultConfiguration;

            RazorEngine engine;
            if (configuration.LanguageVersion.Major == 1)
            {
                engine = RazorEngine.CreateCore(configuration, b =>
                {
                    configure?.Invoke(b);

                    Mvc1_X.RazorExtensions.Register(b);

                    if (configuration.LanguageVersion.Minor >= 1)
                    {
                        Mvc1_X.RazorExtensions.RegisterViewComponentTagHelpers(b);
                    }
                });

                var templateEngine = new Mvc1_X.MvcRazorTemplateEngine(engine, RazorProject.Create(projectPath));
                templateEngine.Options.ImportsFileName = "_ViewImports.cshtml";
                return templateEngine;
            }
            else
            {
                engine = RazorEngine.CreateCore(configuration, b =>
                {
                    configure?.Invoke(b);

                    MvcLatest.RazorExtensions.Register(b);
                });

                var templateEngine = new MvcLatest.MvcRazorTemplateEngine(engine, RazorProject.Create(projectPath));
                templateEngine.Options.ImportsFileName = "_ViewImports.cshtml";
                return templateEngine;
            }
        }

        private ProjectSnapshot FindProject(string directory)
        {
            directory = NormalizeDirectoryPath(directory);

            var projects = _projectManager.Projects;
            for (var i = 0; i < projects.Count; i++)
            {
                var project = projects[i];
                if (project.WorkspaceProject?.FilePath != null)
                {
                    if (string.Equals(directory, NormalizeDirectoryPath(Path.GetDirectoryName(project.WorkspaceProject.FilePath)), StringComparison.OrdinalIgnoreCase))
                    {
                        return project;
                    }
                }
            }

            return null;
        }

        private string NormalizeDirectoryPath(string path)
        {
            return path.Replace('\\', '/').TrimEnd('/');
        }
    }
}
