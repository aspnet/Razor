// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Parser.SyntaxTree;

namespace Microsoft.AspNet.Razor.Generator
{
    public class StatementCodeGenerator : SpanCodeGenerator
    {
        private static readonly int TypeHashCode = typeof(StatementCodeGenerator).GetHashCode();

        public override void GenerateCode(Span target, CodeGeneratorContext context)
        {
            context.CodeTreeBuilder.AddStatementChunk(target.Content, target);
        }

        public override string ToString()
        {
            return "Stmt";
        }

        public override bool Equals(object obj)
        {
            return obj is StatementCodeGenerator;
        }

        public override int GetHashCode()
        {
            return TypeHashCode;
        }
    }
}
