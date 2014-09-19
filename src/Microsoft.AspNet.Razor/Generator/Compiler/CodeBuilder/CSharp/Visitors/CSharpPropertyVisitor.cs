// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.Generator.Compiler.CSharp
{
    public class CSharpPropertyVisitor : CodeVisitor<CSharpCodeWriter>
    {
        private bool _foundTagHelpers;
        private GeneratedTagHelperRenderingContext _tagHelperContext;
        private string _activateAttributeName;

        public CSharpPropertyVisitor(CSharpCodeWriter writer, CodeBuilderContext context)
            : base(writer, context)
        {
            _tagHelperContext = Context.Host.GeneratedClassContext.GeneratedTagHelperRenderingContext;
            _activateAttributeName = Context.Host.GeneratedClassContext.ActivateAttributeName;
        }

        protected override void Visit(TagHelperChunk chunk)
        {
            if (!_foundTagHelpers)
            {
                _foundTagHelpers = true;

                if (_activateAttributeName != null)
                {
                    Writer.Write("[")
                          .Write(_activateAttributeName)
                          .WriteLine("]");
                }

                Writer.Write("private ")
                      .Write(_tagHelperContext.TagHelperManagerName)
                      .Write(" ")
                      .Write(CSharpTagHelperCodeRenderer.ManagerVariableName)
                      .WriteLine(" { get; set; }");
            }
        }
    }
}