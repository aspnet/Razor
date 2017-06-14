﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Razor.Language;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Mvc.Razor.Extensions
{
    public class ViewComponentTagHelperPass : RazorIRPassBase, IRazorIROptimizationPass
    {
        protected override void ExecuteCore(RazorCodeDocument codeDocument, DocumentIRNode irDocument)
        {
            var visitor = new Visitor();
            visitor.Visit(irDocument);

            if (visitor.Class == null || visitor.TagHelpers.Count == 0)
            {
                // Nothing to do, bail.
                return;
            }

            foreach (var tagHelper in visitor.TagHelpers)
            {
                GenerateVCTHClass(visitor.Class, tagHelper.Value);

                var tagHelperTypeName = "global::" + tagHelper.Value.GetTypeName();
                var tagHelperField = visitor.TagHelperFields.SingleOrDefault(
                    n => string.Equals(n.Type, tagHelperTypeName, StringComparison.Ordinal));
                if (tagHelperField != null)
                {
                    var vcthFullTypeName = GetVCTHFullName(visitor.Namespace, visitor.Class, tagHelper.Value);
                    tagHelperField.Type = "global::" + vcthFullTypeName;
                    tagHelperField.Name = GetTagHelperVariableName(vcthFullTypeName);
                }
            }

            foreach (var (parent, node) in visitor.CreateTagHelpers)
            {
                RewriteCreateNode(visitor.Namespace, visitor.Class, (CreateTagHelperIRNode)node, parent);
            }
        }

        private static string GetTagHelperVariableName(string tagHelperTypeName) => "__" + tagHelperTypeName.Replace('.', '_');

        private void GenerateVCTHClass(ClassDeclarationIRNode @class, TagHelperDescriptor tagHelper)
        {
            var writer = new CSharpCodeWriter();
            WriteClass(writer, tagHelper);

            var statement = new CSharpCodeIRNode();
            RazorIRBuilder.Create(statement)
                .Add(new RazorIRToken()
                {
                    Kind = RazorIRToken.TokenKind.CSharp,
                    Content = writer.Builder.ToString()
                });

            @class.Children.Add(statement);
        }

        private void RewriteCreateNode(
            NamespaceDeclarationIRNode @namespace,
            ClassDeclarationIRNode @class,
            CreateTagHelperIRNode node,
            RazorIRNode parent)
        {
            var newTypeName = GetVCTHFullName(@namespace, @class, node.Descriptor);
            for (var i = 0; i < parent.Children.Count; i++)
            {
                if (parent.Children[i] is SetTagHelperPropertyIRNode setProperty &&
                    node.Descriptor.BoundAttributes.Contains(setProperty.Descriptor))
                {
                    setProperty.TagHelperTypeName = newTypeName;
                }
            }

            node.TagHelperTypeName = newTypeName;
        }

        private static string GetVCTHFullName(
            NamespaceDeclarationIRNode @namespace,
            ClassDeclarationIRNode @class,
            TagHelperDescriptor tagHelper)
        {
            var vcName = tagHelper.Metadata[ViewComponentTagHelperDescriptorConventions.ViewComponentNameKey];
            return $"{@namespace.Content}.{@class.Name}.__Generated__{vcName}ViewComponentTagHelper";
        }

        private static string GetVCTHClassName(
            TagHelperDescriptor tagHelper)
        {
            var vcName = tagHelper.Metadata[ViewComponentTagHelperDescriptorConventions.ViewComponentNameKey];
            return $"__Generated__{vcName}ViewComponentTagHelper";
        }

        private void WriteClass(CSharpCodeWriter writer, TagHelperDescriptor descriptor)
        {
            // Add target element.
            BuildTargetElementString(writer, descriptor);

            // Initialize declaration.
            var tagHelperTypeName = "Microsoft.AspNetCore.Razor.TagHelpers.TagHelper";
            var className = GetVCTHClassName(descriptor);

            using (writer.BuildClassDeclaration("public", className, new[] { tagHelperTypeName }))
            {
                // Add view component helper.
                writer.WriteVariableDeclaration(
                    $"private readonly global::Microsoft.AspNetCore.Mvc.IViewComponentHelper",
                    "_helper",
                    value: null);

                // Add constructor.
                BuildConstructorString(writer, className);

                // Add attributes.
                BuildAttributeDeclarations(writer, descriptor);

                // Add process method.
                BuildProcessMethodString(writer, descriptor);
            }
        }

        private void BuildConstructorString(CSharpCodeWriter writer, string className)
        {
            var helperPair = new KeyValuePair<string, string>(
                $"global::Microsoft.AspNetCore.Mvc.IViewComponentHelper",
                "helper");

            using (writer.BuildConstructor("public", className, new[] { helperPair }))
            {
                writer.WriteStartAssignment("_helper")
                    .Write("helper")
                    .WriteLine(";");
            }
        }

        private void BuildAttributeDeclarations(CSharpCodeWriter writer, TagHelperDescriptor descriptor)
        {
            writer.Write("[")
              .Write("Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeNotBoundAttribute")
              .WriteParameterSeparator()
              .Write($"global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewContextAttribute")
              .WriteLine("]");

            writer.WriteAutoPropertyDeclaration(
                "public",
                $"global::Microsoft.AspNetCore.Mvc.Rendering.ViewContext",
                "ViewContext");

            foreach (var attribute in descriptor.BoundAttributes)
            {
                writer.WriteAutoPropertyDeclaration(
                    "public", attribute.TypeName, attribute.GetPropertyName());

                if (attribute.IndexerTypeName != null)
                {
                    writer.Write(" = ")
                        .WriteStartNewObject(attribute.TypeName)
                        .WriteEndMethodInvocation();
                }
            }
        }

        private void BuildProcessMethodString(CSharpCodeWriter writer, TagHelperDescriptor descriptor)
        {
            var contextVariable = "context";
            var outputVariable = "output";

            using (writer.BuildMethodDeclaration(
                    $"public override async",
                    $"global::{typeof(Task).FullName}",
                    "ProcessAsync",
                    new Dictionary<string, string>()
                    {
                        { "Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext", contextVariable },
                        { "Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput", outputVariable }
                    }))
            {
                writer.WriteInstanceMethodInvocation(
                    $"(_helper as global::Microsoft.AspNetCore.Mvc.ViewFeatures.IViewContextAware)?",
                    "Contextualize",
                    new[] { "ViewContext" });

                var methodParameters = GetMethodParameters(descriptor);
                var contentVariable = "content";
                writer.Write("var ")
                    .WriteStartAssignment(contentVariable)
                    .WriteInstanceMethodInvocation($"await _helper", "InvokeAsync", methodParameters);
                writer.WriteStartAssignment($"{outputVariable}.TagName")
                    .WriteLine("null;");
                writer.WriteInstanceMethodInvocation(
                    $"{outputVariable}.Content",
                    "SetHtmlContent",
                    new[] { contentVariable });
            }
        }

        private string[] GetMethodParameters(TagHelperDescriptor descriptor)
        {
            var propertyNames = descriptor.BoundAttributes.Select(
                attribute => attribute.GetPropertyName());
            var joinedPropertyNames = string.Join(", ", propertyNames);
            var parametersString = $"new {{ { joinedPropertyNames } }}";

            var viewComponentName = descriptor.Metadata[
                ViewComponentTagHelperDescriptorConventions.ViewComponentNameKey];
            var methodParameters = new[] { $"\"{viewComponentName}\"", parametersString };
            return methodParameters;
        }

        private void BuildTargetElementString(CSharpCodeWriter writer, TagHelperDescriptor descriptor)
        {
            Debug.Assert(descriptor.TagMatchingRules.Count() == 1);

            var rule = descriptor.TagMatchingRules.First();

            writer.Write("[")
                .WriteStartMethodInvocation("Microsoft.AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute")
                .WriteStringLiteral(rule.TagName)
                .WriteLine(")]");
        }

        private class Visitor : RazorIRNodeWalker
        {
            public ClassDeclarationIRNode Class { get; private set; }

            public List<FieldDeclarationIRNode> TagHelperFields { get; } = new List<FieldDeclarationIRNode>();

            public NamespaceDeclarationIRNode Namespace { get; private set; }

            public List<RazorIRNodeReference> CreateTagHelpers { get; } = new List<RazorIRNodeReference>();

            public Dictionary<string, TagHelperDescriptor> TagHelpers { get; } = new Dictionary<string, TagHelperDescriptor>();

            public override void VisitCreateTagHelper(CreateTagHelperIRNode node)
            {
                var tagHelper = node.Descriptor;
                if (ViewComponentTagHelperDescriptorConventions.IsViewComponentDescriptor(tagHelper))
                {
                    // Capture all the VCTagHelpers (unique by type name) so we can generate a class for each one.
                    var vcName = tagHelper.Metadata[ViewComponentTagHelperDescriptorConventions.ViewComponentNameKey];
                    TagHelpers[vcName] = tagHelper;

                    CreateTagHelpers.Add(new RazorIRNodeReference(Parent, node));
                }
            }

            public override void VisitNamespaceDeclaration(NamespaceDeclarationIRNode node)
            {
                if (Namespace == null)
                {
                    Namespace = node;
                }

                base.VisitNamespaceDeclaration(node);
            }

            public override void VisitClassDeclaration(ClassDeclarationIRNode node)
            {
                if (Class == null)
                {
                    Class = node;
                }

                base.VisitClassDeclaration(node);
            }

            public override void VisitFieldDeclaration(FieldDeclarationIRNode node)
            {
                TagHelperFields.Add(node);

                base.VisitFieldDeclaration(node);
            }
        }
    }
}
