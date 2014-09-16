// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Diagnostics;
using System.Globalization;

namespace Microsoft.AspNet.Razor.Generator.Compiler.CSharp
{
    public class CSharpDesignTimeHelpersVisitor : CodeVisitor<CSharpCodeWriter>
    {
        private const string TagHelperDirectiveSyntaxHelper = "__tagHelperDirectiveSyntaxHelper";
        internal const string InheritsHelper = "__inheritsHelper";
        internal const string DesignTimeHelperMethodName = "__RazorDesignTimeHelpers__";
        private static readonly string ObjectTypeString = typeof(object).ToString();

        private const int DisableVariableNamingWarnings = 219;

        private CSharpCodeVisitor _csharpCodeVisitor;
        private bool _initializedTagHelperDirectiveSyntaxHelper;

        public CSharpDesignTimeHelpersVisitor([NotNull] CSharpCodeVisitor csharpCodeVisitor,
                                              [NotNull] CSharpCodeWriter writer,
                                              [NotNull] CodeBuilderContext context)

            : base(writer, context)
        {
            _csharpCodeVisitor = csharpCodeVisitor;
        }

        public void AcceptTree(CodeTree tree)
        {
            if (Context.Host.DesignTimeMode)
            {
                using (Writer.BuildMethodDeclaration("private", "void", "@" + DesignTimeHelperMethodName))
                {
                    using (Writer.BuildDisableWarningScope(DisableVariableNamingWarnings))
                    {
                        Accept(tree.Chunks);
                    }
                }
            }
        }

        protected override void Visit(SetBaseTypeChunk chunk)
        {
            if (Context.Host.DesignTimeMode)
            {
                using (CSharpLineMappingWriter lineMappingWriter = Writer.BuildLineMapping(chunk.Start, chunk.TypeName.Length, Context.SourceFile))
                {
                    Writer.Indent(chunk.Start.CharacterIndex);

                    lineMappingWriter.MarkLineMappingStart();
                    Writer.Write(chunk.TypeName);
                    lineMappingWriter.MarkLineMappingEnd();

                    Writer.Write(" ").Write(InheritsHelper).Write(" = null;");
                }
            }
        }

        protected override void Visit(AddTagHelperChunk chunk)
        {
            // We should always be in design time mode because of the calling AcceptTree method verification.
            Debug.Assert(Context.Host.DesignTimeMode);

            if (!_initializedTagHelperDirectiveSyntaxHelper)
            {
                _initializedTagHelperDirectiveSyntaxHelper = true;
                Writer.WriteVariableDeclaration(ObjectTypeString, TagHelperDirectiveSyntaxHelper, "null");
            }

            Writer.WriteStartAssignment(TagHelperDirectiveSyntaxHelper);

            _csharpCodeVisitor.CreateExpressionCodeMapping(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "\"{0}\"", chunk.LookupText),
                chunk);

            Writer.WriteLine(";");
        }
    }
}
