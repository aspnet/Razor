// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Generator.Compiler.CSharp;

namespace Microsoft.AspNet.Razor.TagHelpers
{
    /// <summary>
    /// Renders code for tag helper property initialization.
    /// </summary>
    public class TagHelperAttributeCodeRenderer
    {
        /// <summary>
        /// Called during Razor's code generation process to generate code that instantiates the value of the tag 
        /// helper's property. Last value written should not be or end with a semicolon.
        /// </summary>
        /// <param name="attributeDescriptor">The <see cref="TagHelperAttributeDescriptor"/> to generate code for.</param>
        /// <param name="writer">The <see cref="CSharpCodeWriter"/> that's used to write code.</param>
        /// <param name="context">A <see cref="CodeGeneratorContext"/> instance that contains information about 
        /// the current code generation process.</param>
        /// <param name="renderAttributeValue"><see cref="Action"/> that renders the raw value of the HTML attribute. Will be null
        /// if there is no attribute value. Example: If the HTML attribute value is '3' and we want to new up an object that takes
        /// the attribute value, we'd write "new MyObjectType(" then call into <paramref name="renderAttributeValue"/> and finally 
        /// write the ending ")".</param>
        /// <remarks>
        /// Writes the string: "new MyPropertyType(...)" to the output where the "..." is rendered by calling the
        /// <paramref name="renderAttributeValue"/> <see cref="Action"/>.
        /// </remarks>
        public virtual void RenderAttribute([NotNull] TagHelperAttributeDescriptor attributeDescriptor,
                                            [NotNull] CSharpCodeWriter writer,
                                            [NotNull] CodeGeneratorContext context,
                                            [NotNull] Action<CSharpCodeWriter> renderAttributeValue)
        {
            renderAttributeValue(writer);
        }
    }
}