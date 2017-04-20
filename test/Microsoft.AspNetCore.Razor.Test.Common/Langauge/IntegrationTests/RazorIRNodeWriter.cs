﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language.IntegrationTests
{
    // Serializes single IR nodes (shallow).
    public class RazorIRNodeWriter : RazorIRNodeVisitor
    {
        private readonly TextWriter _writer;

        public RazorIRNodeWriter(TextWriter writer)
        {
            _writer = writer;
        }

        public int Depth { get; set; }

        public override void VisitDefault(RazorIRNode node)
        {
            WriteBasicNode(node);
        }

        public override void VisitClass(ClassDeclarationIRNode node)
        {
            WriteContentNode(node, node.AccessModifier, node.Name, node.BaseType, string.Join(", ", node.Interfaces ?? new List<string>()));
        }

        public override void VisitCSharpAttributeValue(CSharpAttributeValueIRNode node)
        {
            WriteContentNode(node, node.Prefix);
        }

        public override void VisitToken(RazorIRToken node)
        {
            WriteContentNode(node, node.Kind.ToString(), node.Content);
        }

        public override void VisitDirective(DirectiveIRNode node)
        {
            WriteContentNode(node, node.Name);
        }

        public override void VisitDirectiveToken(DirectiveTokenIRNode node)
        {
            WriteContentNode(node, node.Content);
        }

        public override void VisitHtmlAttribute(HtmlAttributeIRNode node)
        {
            WriteContentNode(node, node.Prefix, node.Suffix);
        }

        public override void VisitHtmlAttributeValue(HtmlAttributeValueIRNode node)
        {
            WriteContentNode(node, node.Prefix, node.Content);
        }

        public override void VisitNamespace(NamespaceDeclarationIRNode node)
        {
            WriteContentNode(node, node.Content);
        }

        public override void VisitRazorMethodDeclaration(RazorMethodDeclarationIRNode node)
        {
            WriteContentNode(node, node.AccessModifier, string.Join(", ", node.Modifiers ?? new List<string>()), node.ReturnType, node.Name);
        }

        public override void VisitUsingStatement(UsingStatementIRNode node)
        {
            WriteContentNode(node, node.Content);
        }

        public override void VisitDeclareTagHelperFields(DeclareTagHelperFieldsIRNode node)
        {
            WriteContentNode(node, node.UsedTagHelperTypeNames.ToArray());
        }

        public override void VisitInitializeTagHelperStructure(InitializeTagHelperStructureIRNode node)
        {
            WriteContentNode(node, node.TagName, string.Format("{0}.{1}", nameof(TagMode), node.TagMode));
        }

        public override void VisitCreateTagHelper(CreateTagHelperIRNode node)
        {
            WriteContentNode(node, node.TagHelperTypeName);
        }

        public override void VisitSetTagHelperProperty(SetTagHelperPropertyIRNode node)
        {
            WriteContentNode(node, node.AttributeName, node.PropertyName, string.Format("HtmlAttributeValueStyle.{0}", node.ValueStyle));
        }

        public override void VisitAddTagHelperHtmlAttribute(AddTagHelperHtmlAttributeIRNode node)
        {
            WriteContentNode(node, node.Name, string.Format("{0}.{1}", nameof(HtmlAttributeValueStyle), node.ValueStyle));
        }

        public override void VisitExtension(ExtensionIRNode node)
        {
            switch (node)
            {
                case DeclarePreallocatedTagHelperHtmlAttributeIRNode n:
                    WriteContentNode(n, n.VariableName, n.Name, n.Value, string.Format("{0}.{1}", nameof(HtmlAttributeValueStyle), n.ValueStyle));
                    break;
                case AddPreallocatedTagHelperHtmlAttributeIRNode n:
                    WriteContentNode(n, n.VariableName);
                    break;
                case DeclarePreallocatedTagHelperAttributeIRNode n:
                    WriteContentNode(n, n.VariableName, n.Name, n.Value, string.Format("HtmlAttributeValueStyle.{0}", n.ValueStyle));
                    break;
                case SetPreallocatedTagHelperPropertyIRNode n:
                    WriteContentNode(n, n.VariableName, n.AttributeName, n.PropertyName);
                    break;
                default:
                    base.VisitExtension(node);
                    break;
            }
        }

        protected void WriteBasicNode(RazorIRNode node)
        {
            WriteIndent();
            WriteName(node);
            WriteSeparator();
            WriteSourceRange(node);
        }

        protected void WriteContentNode(RazorIRNode node, params string[] content)
        {
            WriteIndent();
            WriteName(node);
            WriteSeparator();
            WriteSourceRange(node);

            for (var i = 0; i < content.Length; i++)
            {
                WriteSeparator();
                WriteContent(content[i]);
            }
        }

        protected void WriteIndent()
        {
            for (var i = 0; i < Depth; i++)
            {
                for (var j = 0; j < 4; j++)
                {
                    _writer.Write(' ');
                }
            }
        }

        protected void WriteSeparator()
        {
            _writer.Write(" - ");
        }

        protected void WriteNewLine()
        {
            _writer.WriteLine();
        }

        protected void WriteName(RazorIRNode node)
        {
            var typeName = node.GetType().Name;
            if (typeName.EndsWith("IRNode"))
            {
                _writer.Write(typeName.Substring(0, typeName.Length - "IRNode".Length));
            }
            else
            {
                _writer.Write(typeName);
            }
        }

        protected void WriteSourceRange(RazorIRNode node)
        {
            if (node.Source != null)
            {
                var sourceRange = node.Source.Value;
                _writer.Write("(");
                _writer.Write(sourceRange.AbsoluteIndex);
                _writer.Write(":");
                _writer.Write(sourceRange.LineIndex);
                _writer.Write(",");
                _writer.Write(sourceRange.CharacterIndex);
                _writer.Write(" [");
                _writer.Write(sourceRange.Length);
                _writer.Write("] ");

                if (sourceRange.FilePath != null)
                {
                    var fileName = sourceRange.FilePath.Substring(sourceRange.FilePath.LastIndexOf('/') + 1);
                    _writer.Write(fileName);
                }

                _writer.Write(")");
            }
        }

        protected void WriteContent(string content)
        {
            if (content == null)
            {
                return;
            }

            // We explicitly escape newlines in node content so that the IR can be compared line-by-line. The escaped
            // newline cannot be platform specific so we need to drop the windows \r.
            // Also, escape our separator so we can search for ` - `to find delimiters.
            _writer.Write(content.Replace("\r", string.Empty).Replace("\n", "\\n").Replace(" - ", "\\-"));
        }
    }
}
