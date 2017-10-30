using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.VisualStudio.RazorExtension
{
    internal class VisualStudioProjectTracker
    {
        private readonly object _inner;

        public VisualStudioProjectTracker(object inner)
        {
            _inner = inner;
        }

        public WorkspaceProjectContext GetProject(ProjectId projectId)
        {
            var method = _inner.GetType().GetMethod("GetProject", BindingFlags.Instance | BindingFlags.NonPublic);
            return new WorkspaceProjectContext(method.Invoke(_inner, new object[] { projectId }));
        }
    }
}
