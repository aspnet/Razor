using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Razor;
using Microsoft.VisualStudio.ComponentModelHost;
using Microsoft.VisualStudio.LanguageServices;
using Microsoft.VisualStudio.ProjectSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using ThreadHelper = Microsoft.VisualStudio.Shell.ThreadHelper;

namespace Microsoft.VisualStudio.RazorExtension
{
    [Export(ExportContractNames.Scopes.UnconfiguredProject, typeof(IProjectDynamicLoadComponent))]
    [AppliesTo("RazorDotNetCore")]
    public class RazorProjectWatcher : IProjectDynamicLoadComponent
    {
        private readonly UnconfiguredProject _project;
        private readonly IServiceProvider _services;
        private readonly VisualStudioWorkspace _workspace;
        private RazorTemplateEngineFactoryService _templateEngineFactory;

        private string _filePath;
        private WorkspaceProjectContext _context;
        private Dictionary<ProjectId, ProjectId> _companionProject;

        [ImportingConstructor]
        public RazorProjectWatcher(
            UnconfiguredProject project,
            Microsoft.VisualStudio.Shell.SVsServiceProvider services,
            VisualStudioWorkspace workspace)
        {
            _project = project;
            _services = (IServiceProvider)services;
            _workspace = workspace;
            _companionProject = new Dictionary<ProjectId, ProjectId>();

            //SynchronizationContext.SetSynchronizationContext(new SynchronizationContext());
            

            _workspace.WorkspaceChanged += Workspace_WorkspaceChanged;
        }

        public Task LoadAsync()
        {
            _filePath = _project.FullPath;

            return Task.CompletedTask;
        }

        public Task UnloadAsync()
        {
            _workspace.WorkspaceChanged -= Workspace_WorkspaceChanged;
            return Task.CompletedTask;
        }

        private async Task CreateCompanionProject(Project project)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

            if (_templateEngineFactory == null)
            {
                var razorLanguageServices = _workspace.Services.GetLanguageServices(RazorLanguage.Name);
                _templateEngineFactory = razorLanguageServices.GetRequiredService<RazorTemplateEngineFactoryService>();
            }

            if (_context != null)
            {
                return;
            }

            var projectTracker = GetProjectTracker();

            var factory = GetProjectContextFactory();
            _context = factory.CreateProjectContext(LanguageNames.CSharp, project.Name + " (Razor)", null, Guid.Empty, null, null);

            var hostProject = projectTracker.GetProject(project.Id);
            _context.AddProjectReference(hostProject, new MetadataReferenceProperties());

        }

        private async void Workspace_WorkspaceChanged(object sender, WorkspaceChangeEventArgs e)
        {
            switch (e.Kind)
            {
                case WorkspaceChangeKind.SolutionAdded:
                {
                    foreach (var project in e.NewSolution.Projects)
                    {
                        if (_filePath == project.FilePath)
                        {
                            await CreateCompanionProject(project);
                            
                            foreach (var document in project.AdditionalDocuments)
                            {
                                AddToCompanionProject(document.Name, document.FilePath, project.Id);
                            }

                            string[] fileNames =
                            {
                                "About.cshtml",
                                "Contact.cshtml",
                                "Test.cshtml",
                                "TestRazorProjectSystem.cshtml"
                            };
                            
                            var directory = "C:\\Users\\ajbaaska\\Source\\Repos\\WebApplication5\\WebApplication5\\Views\\Home\\";
                            foreach (var file in fileNames)
                            {
                                AddToCompanionProject(file, directory + file, project.Id);
                            }
                        }
                    }

                    break;
                }

                case WorkspaceChangeKind.ProjectAdded:
                {
                    //var project = _workspace.CurrentSolution.GetProject(e.ProjectId);
                    //if (_filePath == project.FilePath)
                    //{
                    //    try
                    //    {
                    //        await CreateCompanionProject(project);
                    //    }
                    //    catch
                    //    {
                            
                    //    }
                        
                    //    foreach (var document in project.AdditionalDocuments)
                    //    {
                    //        AddToCompanionProject(document.Name, document.FilePath, e.ProjectId);
                    //    }

                    //    string[] fileNames =
                    //    {
                    //        "About.cshtml",
                    //        "Contact.cshtml",
                    //        "Test.cshtml",
                    //        "TestRazorProjectSystem.cshtml"
                    //    };
                    //    var directory = "C:\\Users\\ajbaaska\\Source\\Repos\\WebApplication5\\WebApplication5\\Views\\Home\\";
                    //    foreach (var file in fileNames)
                    //    {
                    //        AddToCompanionProject(file, directory + file, e.ProjectId);
                    //    }
                    //}

                    break;
                }
            }
        }

        private void AddToCompanionProject(string documentName, string filePath, ProjectId projectId)
        {
            if (documentName.EndsWith(".cshtml", StringComparison.OrdinalIgnoreCase))
            {
                var projectTracker = GetProjectTracker();
                var currentProject = _workspace.CurrentSolution.GetProject(projectId);
                var engine = _templateEngineFactory.Create(currentProject.FilePath, builder =>
                {
                });

                var codeDocument = engine.CreateCodeDocument(filePath);
                var cSharpDocument = engine.GenerateCode(codeDocument);

                WorkspaceProjectContext companionProject = null;
                foreach (var project in _workspace.CurrentSolution.Projects)
                {
                    if (project.Name.StartsWith(currentProject.Name) && project.Name.EndsWith("(Razor)"))
                    {
                        companionProject = projectTracker.GetProject(project.Id);
                        break;
                    }
                }

                var generatedFilePath = $"C:\\Users\\ajbaaska\\Desktop\\temp\\{documentName.Substring(0, documentName.Length - 7)}.cs";
                using (var writer = File.CreateText(generatedFilePath))
                {
                    writer.Write(cSharpDocument.GeneratedCode.Substring(20));
                }

                
                companionProject.AddSourceFile(generatedFilePath);
            }
        }

        private WorkspaceProjectContextFactory GetProjectContextFactory()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var componentModel = (IComponentModel)_services.GetService(typeof(SComponentModel));
            var assembly = Assembly.Load("Microsoft.VisualStudio.LanguageServices, Version=2.3.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35");
            var type = assembly.GetType("Microsoft.VisualStudio.LanguageServices.ProjectSystem.IWorkspaceProjectContextFactory");
            var method = componentModel.GetType().GetMethod("GetService").MakeGenericMethod(type);
            return new WorkspaceProjectContextFactory(method.Invoke(componentModel, null));
        }

        private VisualStudioProjectTracker GetProjectTracker()
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var property = _workspace.GetType().GetProperty("ProjectTracker", BindingFlags.Instance | BindingFlags.NonPublic);
            return new VisualStudioProjectTracker(property.GetValue(_workspace, null));
        }
    }
}
