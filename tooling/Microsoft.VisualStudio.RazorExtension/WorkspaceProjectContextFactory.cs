using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.RazorExtension
{
    // see https://github.com/dotnet/roslyn/blob/master/src/VisualStudio/Core/Def/Implementation/ProjectSystem/CPS/IWorkspaceProjectContextFactory.cs
    internal class WorkspaceProjectContextFactory
    {
        private readonly object _inner;
        private readonly InterfaceMapping _interfaceMap;
        private readonly Type _interfaceType;

        public WorkspaceProjectContextFactory(object inner)
        {
            _inner = inner;

            _interfaceType = _inner.GetType().GetInterfaces().Where(i => i.Name == "IWorkspaceProjectContextFactory").First();
            _interfaceMap = _inner.GetType().GetInterfaceMap(_interfaceType);
        }

        /// <summary>
        /// Creates and initializes a new Workspace project and returns a <see cref="IWorkspaceProjectContext"/> to lazily initialize the properties and items for the project.
        /// This method can be invoked on a background thread and doesn't access any members of the given UI <paramref name="hierarchy"/>,
        /// allowing the UI hierarchy to be published lazily, as long as <see cref="VisualStudioWorkspaceImpl.GetProjectTrackerAndInitializeIfNecessary"/> has been called once.
        /// </summary>
        /// <param name="languageName">Project language.</param>
        /// <param name="projectDisplayName">Display name for the project.</param>
        /// <param name="projectFilePath">Full path to the project file for the project.</param>
        /// <param name="projectGuid">Project guid.</param>
        /// <param name="hierarchy"><see cref="IVsHierarchy"/> for the project, an be null in deferred project load cases.</param>
        /// <param name="binOutputPath">Initial project binary output path.</param>
        public WorkspaceProjectContext CreateProjectContext(string languageName, string projectDisplayName, string projectFilePath, Guid projectGuid, object hierarchy, string binOutputPath)
        {
            var index = 0;
            for (; index < _interfaceMap.InterfaceMethods.Length; index++)
            {
                if (_interfaceMap.InterfaceMethods[index].Name == "CreateProjectContext" &&
                    _interfaceMap.InterfaceMethods[index].GetParameters().Length == 6)
                {
                    break;
                }
            }

            var context = _interfaceMap.TargetMethods[index].Invoke(_inner, new object[] { languageName, projectDisplayName, projectFilePath, projectGuid, hierarchy, binOutputPath });
            return new WorkspaceProjectContext(context);
        }
    }
}
