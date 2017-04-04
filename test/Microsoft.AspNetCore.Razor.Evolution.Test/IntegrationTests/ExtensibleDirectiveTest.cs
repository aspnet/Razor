// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Evolution.Intermediate;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Evolution.IntegrationTests
{
    // Extensible directives only have codegen for design time, so we're only testing that.
    public class ExtensibleDirectiveTest : IntegrationTestBase
    {
        [Fact]
        public void NamespaceToken()
        {
            // Arrange
            var engine = RazorEngine.CreateDesignTime(builder =>
            {
                builder.Features.Add(new ApiSetsIRTestAdapter());

                builder.AddDirective(DirectiveDescriptorBuilder.Create("custom").AddNamespace().Build());
            });

            var document = CreateCodeDocument();

            // Act
            engine.Process(document);

            // Assert
            AssertIRMatchesBaseline(document.GetIRDocument());
            AssertDesignTimeDocumentMatchBaseline(document);
        }

        private class ApiSetsIRTestAdapter : RazorIRPassBase, IRazorIROptimizationPass
        {
            public override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIRNode irDocument)
            {
                var walker = new ApiSetsIRWalker();
                walker.Visit(irDocument);
            }

            private class ApiSetsIRWalker : RazorIRNodeWalker
            {
                public override void VisitClass(ClassDeclarationIRNode node)
                {
                    node.Name = Filename.Replace('/', '_');
                    node.AccessModifier = "public";

                    VisitDefault(node);
                }

                public override void VisitNamespace(NamespaceDeclarationIRNode node)
                {
                    node.Content = typeof(CodeGenerationIntegrationTest).Namespace + ".TestFiles";

                    VisitDefault(node);
                }

                public override void VisitRazorMethodDeclaration(RazorMethodDeclarationIRNode node)
                {
                    node.AccessModifier = "public";
                    node.Modifiers = new[] { "async" };
                    node.ReturnType = typeof(Task).FullName;
                    node.Name = "ExecuteAsync";

                    VisitDefault(node);
                }
            }
        }
    }
}
