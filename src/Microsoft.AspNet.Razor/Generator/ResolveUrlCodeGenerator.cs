// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Parser.SyntaxTree;

namespace Microsoft.AspNet.Razor.Generator
{
    public class ResolveUrlCodeGenerator : SpanCodeGenerator
    {
        private static readonly int TypeHashCode = typeof(ResolveUrlCodeGenerator).GetHashCode();

        public override void GenerateCode(Span target, CodeGeneratorContext context)
        {
            // Check if the host supports it
            if (string.IsNullOrEmpty(context.Host.GeneratedClassContext.ResolveUrlMethodName))
            {
                // Nope, just use the default MarkupCodeGenerator behavior
                new MarkupCodeGenerator().GenerateCode(target, context);
                return;
            }

            context.CodeTreeBuilder.AddResolveUrlChunk(target.Content, target);
        }

        public override string ToString()
        {
            return "VirtualPath";
        }

        public override bool Equals(object obj)
        {
            return obj is ResolveUrlCodeGenerator;
        }

        public override int GetHashCode()
        {
            return TypeHashCode;
        }
    }
}
