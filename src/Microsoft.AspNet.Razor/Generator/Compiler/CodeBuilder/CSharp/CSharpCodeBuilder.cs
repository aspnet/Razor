// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Generator.Compiler.CSharp
{
    public class CSharpCodeBuilder : CodeBuilder
    {
        private const int DisableAsyncWarning = 1998;

        public CSharpCodeBuilder(CodeBuilderContext context)
            : base(context)
        {
        }

        private CodeTree Tree { get { return Context.CodeTreeBuilder.CodeTree; } }
        public RazorEngineHost Host { get { return Context.Host; } }

        public override CodeBuilderResult Build()
        {
            var writer = new CSharpCodeWriter();

            using (writer.BuildNamespace(Context.RootNamespace))
            {
                // Write out using directives
                AddImports(Tree, writer, Host.NamespaceImports);
                // Separate the usings and the class
                writer.WriteLine();

                new CSharpClassAttributeVisitor(writer, Context).Accept(Tree.Chunks);

                using (BuildClassDeclaration(writer))
                {
                    if (Host.DesignTimeMode)
                    {
                        writer.WriteLine("private static object @__o;");
                    }

                    var csharpCodeVisitor = DecorateCSharpCodeVisitor(writer, 
                                                                      Context, 
                                                                      new CSharpCodeVisitor(writer, Context));

                    new CSharpHelperVisitor(csharpCodeVisitor, writer, Context).Accept(Tree.Chunks);
                    new CSharpTypeMemberVisitor(csharpCodeVisitor, writer, Context).Accept(Tree.Chunks);
                    new CSharpDesignTimeHelpersVisitor(writer, Context).AcceptTree(Tree);
                    new CSharpPropertyVisitor(writer, Context).Accept(Tree.Chunks);

                    BuildConstructor(writer);

                    // Add space inbetween constructor and method body
                    writer.WriteLine();

                    using (writer.BuildDisableWarningScope(DisableAsyncWarning))
                    {
                        using (writer.BuildMethodDeclaration("public override async", "Task", Host.GeneratedClassContext.ExecuteMethodName))
                        {
                            new CSharpTagHelperDeclarationVisitor(writer, Context).Accept(Tree.Chunks);

                            csharpCodeVisitor.Accept(Tree.Chunks);
                        }
                    }
                }
            }

            return new CodeBuilderResult(writer.GenerateCode(), writer.LineMappingManager.Mappings);
        }

        protected virtual CSharpCodeVisitor DecorateCSharpCodeVisitor([NotNull] CSharpCodeWriter writer,
                                                                      [NotNull] CodeBuilderContext context,
                                                                      [NotNull] CSharpCodeVisitor incomingVisitor)
        {
            return incomingVisitor;
        }

        protected virtual CSharpCodeWritingScope BuildClassDeclaration(CSharpCodeWriter writer)
        {
            var baseTypeVisitor = new CSharpBaseTypeVisitor(writer, Context);
            baseTypeVisitor.Accept(Tree.Chunks);

            var baseType = baseTypeVisitor.CurrentBaseType ?? Host.DefaultBaseClass;

            var baseTypes = string.IsNullOrEmpty(baseType) ? Enumerable.Empty<string>() : new string[] { baseType };

            return writer.BuildClassDeclaration("public", Context.ClassName, baseTypes);
        }

        protected virtual void BuildConstructor(CSharpCodeWriter writer)
        {
            writer.WriteLineHiddenDirective();
            using (writer.BuildConstructor(Context.ClassName))
            {
                // Any constructor based logic that we need to add?
            };
        }

        private void AddImports(CodeTree codeTree, CSharpCodeWriter writer, IEnumerable<string> defaultImports)
        {
            // Write out using directives
            var usingVisitor = new CSharpUsingVisitor(writer, Context);
            foreach (Chunk chunk in Tree.Chunks)
            {
                usingVisitor.Accept(chunk);
            }

            defaultImports = defaultImports.Except(usingVisitor.ImportedUsings);

            foreach (string import in defaultImports)
            {
                writer.WriteUsing(import);
            }

            string taskNamespace = typeof(Task).Namespace;

            // We need to add the task namespace but ONLY if it hasn't been added by the default imports or using imports yet.
            if(!defaultImports.Contains(taskNamespace) && !usingVisitor.ImportedUsings.Contains(taskNamespace))
            {
                writer.WriteUsing(taskNamespace);
            }
        }
    }
}
