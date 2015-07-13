// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Microsoft.AspNet.Razor.Chunks;
using Microsoft.Framework.Internal;

namespace Microsoft.AspNet.Razor.CodeGenerators.Visitors
{
    public class CSharpTagHelperFieldDeclarationVisitor : CodeVisitor<CSharpCodeWriter>
    {
        private readonly HashSet<string> _declaredTagHelpers;
        private readonly GeneratedTagHelperContext _tagHelperContext;
        private bool _foundTagHelpers;

        public CSharpTagHelperFieldDeclarationVisitor([NotNull] CSharpCodeWriter writer,
                                                      [NotNull] CodeGeneratorContext context)
            : base(writer, context)
        {
            _declaredTagHelpers = new HashSet<string>(StringComparer.Ordinal);
            _tagHelperContext = Context.Host.GeneratedClassContext.GeneratedTagHelperContext;
        }

        protected override void Visit(TagHelperChunk chunk)
        {
            // We only want to setup tag helper manager fields if there are tag helpers, and only once
            if (!_foundTagHelpers)
            {
                _foundTagHelpers = true;

                // We want to hide declared TagHelper fields so they cannot be stepped over via a debugger.
                Writer.WriteLineHiddenDirective();

                // Runtime fields aren't useful during design time.
                if (!Context.Host.DesignTimeMode)
                {
                    // Need to disable the warning "X is assigned to but never used." for the value buffer since
                    // whether it's used depends on how a TagHelper is used.
                    Writer.WritePragma("warning disable 0414");
                    WritePrivateField(_tagHelperContext.TagHelperContentTypeName,
                                      CSharpTagHelperCodeRenderer.StringValueBufferVariableName,
                                      value: null);
                    Writer.WritePragma("warning restore 0414");

                    WritePrivateField(_tagHelperContext.ExecutionContextTypeName,
                                      CSharpTagHelperCodeRenderer.ExecutionContextVariableName,
                                      value: null);

                    Writer
                        .Write("private ")
                        .WriteVariableDeclaration(
                            _tagHelperContext.RunnerTypeName,
                            CSharpTagHelperCodeRenderer.RunnerVariableName,
                            value: null);

                    Writer.Write("private ")
                          .Write(_tagHelperContext.ScopeManagerTypeName)
                          .Write(" ")
                          .WriteStartAssignment(CSharpTagHelperCodeRenderer.ScopeManagerVariableName)
                          .WriteStartNewObject(_tagHelperContext.ScopeManagerTypeName)
                          .WriteEndMethodInvocation();

                    Writer
                        .Write("private ")
                        .WriteVariableDeclaration(
                            _tagHelperContext.UnchangedTagHelperAttributeValueBufferTypeName,
                            CSharpTagHelperCodeRenderer.OriginalTagHelperAttributeValueVariableName,
                            value: null);

                    Writer
                        .Write("private ")
                        .WriteVariableDeclaration(
                            "object",
                            CSharpTagHelperCodeRenderer.RawAttributeValueVariableName,
                            value: null);

                    Writer
                        .Write("private ")
                        .WriteVariableDeclaration(
                            "bool",
                            CSharpTagHelperCodeRenderer.ShouldRenderTagHelperAttributeVariableName,
                            value: "false");

                }
            }

            foreach (var descriptor in chunk.Descriptors)
            {
                if (!_declaredTagHelpers.Contains(descriptor.TypeName))
                {
                    _declaredTagHelpers.Add(descriptor.TypeName);

                    WritePrivateField(descriptor.TypeName,
                                      CSharpTagHelperCodeRenderer.GetVariableName(descriptor),
                                      value: null);
                }
            }

            // We need to dive deeper to ensure we pick up any nested tag helpers.
            Accept(chunk.Children);
        }

        public override void Accept(Chunk chunk)
        {
            var parentChunk = chunk as ParentChunk;

            // If we're any ParentChunk other than TagHelperChunk then we want to dive into its Children
            // to search for more TagHelperChunk chunks. This if-statement enables us to not override
            // each of the special ParentChunk types and then dive into their children.
            if (parentChunk != null && !(parentChunk is TagHelperChunk))
            {
                Accept(parentChunk.Children);
            }
            else
            {
                // If we're a TagHelperChunk or any other non ParentChunk we ".Accept" it. This ensures
                // that our overridden Visit(TagHelperChunk) method gets called and is not skipped over.
                // If we're a non ParentChunk or a TagHelperChunk then we want to just invoke the Visit
                // method for that given chunk (base.Accept indirectly calls the Visit method).
                base.Accept(chunk);
            }
        }

        private void WritePrivateField(string type, string name, string value)
        {
            Writer.Write("private ")
                  .WriteVariableDeclaration(type, name, value);
        }
    }
}