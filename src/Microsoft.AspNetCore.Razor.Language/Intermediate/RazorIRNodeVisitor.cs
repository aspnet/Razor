﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public abstract class RazorIRNodeVisitor
    {
        public virtual void Visit(RazorIRNode node)
        {
            node.Accept(this);
        }

        public virtual void VisitDefault(RazorIRNode node)
        {
        }

        public virtual void VisitChecksum(ChecksumIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitToken(RazorIRToken node)
        {
            VisitDefault(node);
        }

        public virtual void VisitDirectiveToken(DirectiveTokenIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitDirective(DirectiveIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitExtension(ExtensionIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitCSharpCode(CSharpCodeIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitCSharpExpression(CSharpExpressionIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitHtmlAttributeValue(HtmlAttributeValueIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitCSharpExpressionAttributeValue(CSharpExpressionAttributeValueIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitCSharpCodeAttributeValue(CSharpCodeAttributeValueIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitHtmlAttribute(HtmlAttributeIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitClassDeclaration(ClassDeclarationIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitMethodDeclaration(MethodDeclarationIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitFieldDeclaration(FieldDeclarationIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitPropertyDeclaration(PropertyDeclarationIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitDocument(DocumentIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitHtml(HtmlContentIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitNamespaceDeclaration(NamespaceDeclarationIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitUsingStatement(UsingStatementIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitTagHelper(TagHelperIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitTagHelperBody(TagHelperBodyIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitCreateTagHelper(CreateTagHelperIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitSetTagHelperProperty(SetTagHelperPropertyIRNode node)
        {
            VisitDefault(node);
        }

        public virtual void VisitAddTagHelperHtmlAttribute(AddTagHelperHtmlAttributeIRNode node)
        {
            VisitDefault(node);
        }
    }
}
