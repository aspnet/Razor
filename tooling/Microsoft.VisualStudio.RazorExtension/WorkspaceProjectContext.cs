using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Microsoft.VisualStudio.RazorExtension
{
    internal class WorkspaceProjectContext
    {
        private readonly dynamic _inner;

        public WorkspaceProjectContext(object inner)
        {
            _inner = inner;
        }

        public void AddProjectReference(WorkspaceProjectContext project, MetadataReferenceProperties properties)
        {
            var method = _inner.GetType().GetMethod("AddProjectReference");
            method.Invoke(_inner, new object[] { project._inner, properties });
        }

        public void AddSourceFile(string filePath, bool isInCurrentContext = true, IEnumerable<string> folderNames = null, SourceCodeKind sourceCodeKind = SourceCodeKind.Regular)
        {
            var method = _inner.GetType().GetMethod("AddSourceFile");
            method.Invoke(_inner, new object[] { filePath, isInCurrentContext, folderNames, sourceCodeKind });
        }
    }
}