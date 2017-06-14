﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.CodeGeneration;
using Microsoft.AspNetCore.Razor.Language.Intermediate;

namespace Microsoft.AspNetCore.Razor.Language
{
    public abstract class DocumentClassifierPassBase : RazorIRPassBase, IRazorDocumentClassifierPass
    {
        private static readonly ICodeTargetExtension[] EmptyExtensionArray = new ICodeTargetExtension[0];

        protected abstract string DocumentKind { get; }

        protected ICodeTargetExtension[] TargetExtensions { get; private set; }

        protected override void OnInitialized()
        {
            var feature = Engine.Features.OfType<IRazorTargetExtensionFeature>();
            TargetExtensions = feature.FirstOrDefault()?.TargetExtensions.ToArray() ?? EmptyExtensionArray;
        }

        protected sealed override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIRNode irDocument)
        {
            if (irDocument.DocumentKind != null)
            {
                return;
            }

            if (!IsMatch(codeDocument, irDocument))
            {
                return;
            }

            irDocument.DocumentKind = DocumentKind;
            irDocument.Target = CreateTarget(codeDocument, irDocument.Options);

            Rewrite(codeDocument, irDocument);
        }

        private void Rewrite(RazorCodeDocument codeDocument, DocumentIRNode irDocument)
        {
            // Rewrite the document from a flat structure to use a sensible default structure,
            // a namespace and class declaration with a single 'razor' method.
            var children = new List<RazorIRNode>(irDocument.Children);
            irDocument.Children.Clear();

            var @namespace = new NamespaceDeclarationIRNode();
            @namespace.Annotations[CommonAnnotations.PrimaryNamespace] = CommonAnnotations.PrimaryNamespace;

            var @class = new ClassDeclarationIRNode();
            @class.Annotations[CommonAnnotations.PrimaryClass] = CommonAnnotations.PrimaryClass;

            var method = new MethodDeclarationIRNode();
            method.Annotations[CommonAnnotations.PrimaryMethod] = CommonAnnotations.PrimaryMethod;

            var documentBuilder = RazorIRBuilder.Create(irDocument);

            var namespaceBuilder = RazorIRBuilder.Create(documentBuilder.Current);
            namespaceBuilder.Push(@namespace);

            var classBuilder = RazorIRBuilder.Create(namespaceBuilder.Current);
            classBuilder.Push(@class);

            var methodBuilder = RazorIRBuilder.Create(classBuilder.Current);
            methodBuilder.Push(method);

            var visitor = new Visitor(documentBuilder, namespaceBuilder, classBuilder, methodBuilder);

            for (var i = 0; i < children.Count; i++)
            {
                visitor.Visit(children[i]);
            }

            // Note that this is called at the *end* of rewriting so that user code can see the tree
            // and look at its content to make a decision.
            OnDocumentStructureCreated(codeDocument, @namespace, @class, method);
        }

        protected abstract bool IsMatch(RazorCodeDocument codeDocument, DocumentIRNode irDocument);

        private CodeTarget CreateTarget(RazorCodeDocument codeDocument, RazorCodeGenerationOptions options)
        {
            return CodeTarget.CreateDefault(codeDocument, options, (builder) =>
            {
                for (var i = 0; i < TargetExtensions.Length; i++)
                {
                    builder.TargetExtensions.Add(TargetExtensions[i]);
                }

                ConfigureTarget(builder);
            });
        }

        protected virtual void ConfigureTarget(ICodeTargetBuilder builder)
        {
            // Intentionally empty.
        }

        protected virtual void OnDocumentStructureCreated(
            RazorCodeDocument codeDocument,
            NamespaceDeclarationIRNode @namespace,
            ClassDeclarationIRNode @class,
            MethodDeclarationIRNode @method)
        {
            // Intentionally empty.
        }

        private class Visitor : RazorIRNodeVisitor
        {
            private readonly RazorIRBuilder _document;
            private readonly RazorIRBuilder _namespace;
            private readonly RazorIRBuilder _class;
            private readonly RazorIRBuilder _method;

            public Visitor(RazorIRBuilder document, RazorIRBuilder @namespace, RazorIRBuilder @class, RazorIRBuilder method)
            {
                _document = document;
                _namespace = @namespace;
                _class = @class;
                _method = method;
            }

            public override void VisitChecksum(ChecksumIRNode node)
            {
                _document.Insert(0, node);
            }

            public override void VisitUsingStatement(UsingStatementIRNode node)
            {
                var children = _namespace.Current.Children;
                var i = children.Count - 1;
                for (; i >= 0; i--)
                {
                    var child = children[i];
                    if (child is UsingStatementIRNode)
                    {
                        break;
                    }
                }

                _namespace.Insert(i + 1, node);
            }

            public override void VisitFieldDeclaration(FieldDeclarationIRNode node)
            {
                if (node.Annotations.ContainsKey(CommonAnnotations.InitializeTagHelperVariables) ||
                    node.Annotations.ContainsKey(CommonAnnotations.TagHelperField))
                {
                    _class.Insert(0, node);
                }
                else
                {
                    base.VisitFieldDeclaration(node);
                }
            }

            public override void VisitDefault(RazorIRNode node)
            {
                if (node is MemberDeclarationIRNode)
                {
                    _class.Add(node);
                    return;
                }

                _method.Add(node);
            }
        }
    }
}
