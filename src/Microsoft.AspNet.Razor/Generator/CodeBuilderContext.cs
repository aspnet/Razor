// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.TagHelpers;

namespace Microsoft.AspNet.Razor.Generator
{
    /// <summary>
    /// Context object with information used to generate a Razor page.
    /// </summary>
    public class CodeBuilderContext : CodeGeneratorContext
    {
        /// <summary>
        /// Instantiates a new instance of the <see cref="CodeBuilderContext"/> object.
        /// </summary>
        /// <param name="generatorContext">A <see cref="CodeGeneratorContext"/> to copy information from.</param>
        /// <param name="tagHelperProvider">The <see cref="TagHelperProvider"/> that can be queried for
        /// <see cref="TagHelperDescriptor"/>s.</param>
        public CodeBuilderContext(CodeGeneratorContext generatorContext, TagHelperProvider tagHelperProvider)
            : base(generatorContext)
        {
            ExpressionRenderingMode = ExpressionRenderingMode.WriteToOutput;
            TagHelperProvider = tagHelperProvider;
        }

        // Internal for testing.
        internal CodeBuilderContext(RazorEngineHost host,
                                    string className,
                                    string rootNamespace,
                                    string sourceFile,
                                    bool shouldGenerateLinePragmas,
                                    TagHelperProvider tagHelperProvider)
            : base(host, className, rootNamespace, sourceFile, shouldGenerateLinePragmas)
        {
            ExpressionRenderingMode = ExpressionRenderingMode.WriteToOutput;
            TagHelperProvider = tagHelperProvider;
        }


        /// <summary>
        /// The current C# rendering mode.
        /// </summary>
        /// <remarks>
        /// <see cref="ExpressionRenderingMode.WriteToOutput"/> forces C# generation to write 
        /// <see cref="Compiler.Chunk"/>s to the output page, i.e. WriteLiteral("Hello World").
        /// <see cref="ExpressionRenderingMode.InjectCode"/> writes <see cref="Compiler.Chunk"/> values in their
        /// rawest form, i.e. "Hello World".
        /// </remarks>
        public ExpressionRenderingMode ExpressionRenderingMode { get; set; }

        /// <summary>
        /// The C# writer to write <see cref="Compiler.Chunk"/> information to.
        /// </summary>
        /// <remarks>
        /// If <see cref="TargetWriterName"/> is <c>null</c> values will be written using a default write method
        /// i.e. WriteLiteral("Hello World").
        /// If <see cref="TargetWriterName"/> is not <c>null</c> values will be written to the given 
        /// <see cref="TargetWriterName"/>, i.e. WriteLiteralTo("Hello World", myWriter).
        /// </remarks>
        public string TargetWriterName { get; set; }

        /// <summary>
        /// The <see cref="TagHelperDescriptor"/> registration system used to lookup 
        /// <see cref="TagHelperDescriptor"/>s.
        /// </summary>
        public TagHelperProvider TagHelperProvider { get; set; }
    }
}