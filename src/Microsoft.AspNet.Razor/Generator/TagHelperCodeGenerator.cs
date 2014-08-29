// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.AspNet.Razor.Generator.Compiler;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Parser.TagHelpers;

namespace Microsoft.AspNet.Razor.Generator
{
    /// <summary>
    /// A <see cref="BlockCodeGenerator"/> that is responsible for generating valid <see cref="TagHelperChunk"/>'s.
    /// </summary>
    public class TagHelperCodeGenerator : BlockCodeGenerator
    {
        /// <summary>
        /// Starts the generation of a <see cref="TagHelperChunk"/>.
        /// </summary>
        /// <param name="target">The <see cref="Block"/> thats <see cref="Block.CodeGenerator"/> is
        /// responsible for this <see cref="TagHelperCodeGenerator"/>.</param>
        /// <param name="context">A <see cref="CodeGeneratorContext"/> instance that contains information about 
        /// the current code generation process.</param>
        public override void GenerateStartBlockCode(Block target, CodeGeneratorContext context)
        {
            Debug.Assert(target is TagHelperBlock);

            var tagHelperBlock = target as TagHelperBlock;
            var attributes = new Dictionary<string, Chunk>(StringComparer.OrdinalIgnoreCase);

            // We need to create a code generator to create chunks for each of the attributes.
            var codeGenerator = context.Host.CreateCodeGenerator(
                context.ClassName,
                context.RootNamespace,
                context.SourceFile);

            foreach (var attribute in tagHelperBlock.Attributes)
            {
                var attributeName = attribute.Key;
                var attributeValue = attribute.Value;

                // Populates the code tree with chunks associated with attributes
                attributeValue.Accept(codeGenerator);

                var chunks = codeGenerator.Context.CodeTreeBuilder.CodeTree.Chunks;

                attributes.Add(attributeName, new ChunkBlock
                {
                    Children = chunks
                });

                // Reset the code tree builder so we can build a new one for the next attribute
                codeGenerator.Context.CodeTreeBuilder = new CodeTreeBuilder();
            }

            context.CodeTreeBuilder.StartChunkBlock(
                new TagHelperChunk
                {
                    TagName = tagHelperBlock.TagName,
                    Attributes = attributes
                },
                target,
                topLevel: false);
        }

        /// <summary>
        /// Ends the generation of a <see cref="TagHelperChunk"/> capturing all previously visited children
        /// since the <see cref="GenerateStartBlockCode(Block, CodeGeneratorContext)"/> was called.
        /// </summary>
        /// <param name="target">The <see cref="Block"/> thats <see cref="Block.CodeGenerator"/> is
        /// responsible for this <see cref="TagHelperCodeGenerator"/>.</param>
        /// <param name="context">A <see cref="CodeGeneratorContext"/> instance that contains information about 
        /// the current code generation process.</param>
        public override void GenerateEndBlockCode(Block target, CodeGeneratorContext context)
        {
            context.CodeTreeBuilder.EndChunkBlock();
        }
    }
}