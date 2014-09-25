// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.Generator.Compiler.CSharp
{
    public class CSharpPropertyVisitor : CodeVisitor<CSharpCodeWriter>
    {
        private bool _foundTagHelpers;
        private GeneratedTagHelperContext _tagHelperContext;
        private string _activateAttributeName;

        public CSharpPropertyVisitor(CSharpCodeWriter writer, CodeBuilderContext context)
            : base(writer, context)
        {
            _tagHelperContext = Context.Host.GeneratedClassContext.GeneratedTagHelperContext;
            _activateAttributeName = Context.Host.GeneratedClassContext.ActivateAttributeName;
        }

        protected override void Visit(TagHelperChunk chunk)
        {
            if (!_foundTagHelpers)
            {
                _foundTagHelpers = true;

                WriteActivatedProperty(_tagHelperContext.TagHelperRunnerTypeName,
                                       CSharpTagHelperCodeRenderer.DefaultTagHelperRunnerVariableName);
                WriteActivatedProperty(_tagHelperContext.TagHelperScopeManagerTypeName,
                                       CSharpTagHelperCodeRenderer.DefaultTagHelperScopeManagerVariableName);
            }
        }

        private void WriteActivatedProperty(string typeName, string name)
        {
            if (_activateAttributeName != null)
            {
                Writer.Write("[")
                      .Write(_activateAttributeName)
                      .WriteLine("]");
            }

            Writer.Write("private ")
                  .Write(typeName)
                  .Write(" ")
                  .Write(name)
                  .WriteLine(" { get; set; }");
        }
    }
}