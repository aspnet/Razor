﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Evolution.Intermediate
{
    public abstract class RazorIRNodeVisitor<TResult>
    {
        public virtual TResult Visit(RazorIRNode node)
        {
            return node.Accept(this);
        }

        public virtual TResult VisitDefault(RazorIRNode node)
        {
            return default(TResult);
        }

        public virtual TResult VisitDirectiveToken(DirectiveTokenIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitTemplate(TemplateIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitSection(SectionIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitCSharpStatement(CSharpStatementIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitCSharpExpression(CSharpExpressionIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitCSharpToken(CSharpTokenIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitHtmlAttributeValue(HtmlAttributeValueIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitCSharpAttributeValue(CSharpAttributeValueIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitHtmlAttribute(HtmlAttributeIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitClass(ClassDeclarationIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitSingleLineDirective(SingleLineDirectiveIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitBlockDirective(BlockDirectiveIRNode node)
        {
            return VisitDefault(node);
        }
        public virtual TResult VisitMethodDeclaration(MethodDeclarationIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitDocument(DocumentIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitHtml(HtmlContentIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitNamespace(NamespaceDeclarationIRNode node)
        {
            return VisitDefault(node);
        }

        public virtual TResult VisitUsingStatement(UsingStatementIRNode node)
        {
            return VisitDefault(node);
        }
    }
}
