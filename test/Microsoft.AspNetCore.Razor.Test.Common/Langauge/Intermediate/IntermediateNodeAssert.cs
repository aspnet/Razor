﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.AspNetCore.Razor.Language.Intermediate
{
    public static class IntermediateNodeAssert
    {
        public static TNode SingleChild<TNode>(IntermediateNode node)
        {
            if (node.Children.Count == 0)
            {
                throw new IntermediateNodeAssertException(node, "The node has no children.");
            }
            else if (node.Children.Count > 1)
            {
                throw new IntermediateNodeAssertException(node, node.Children, "The node has multiple children");
            }

            var child = node.Children[0];
            return Assert.IsType<TNode>(child);
        }

        public static void NoChildren(IntermediateNode node)
        {
            if (node.Children.Count > 0)
            {
                throw new IntermediateNodeAssertException(node, node.Children, "The node has children.");
            }
        }

        public static void Children(IntermediateNode node, params Action<IntermediateNode>[] validators)
        {
            var i = 0;
            for (; i < validators.Length; i++)
            {
                if (node.Children.Count == i)
                {
                    throw new IntermediateNodeAssertException(node, node.Children, $"The node only has {node.Children.Count} children.");
                }

                try
                {
                    validators[i].Invoke(node.Children[i]);
                }
                catch (XunitException e)
                {
                    throw new IntermediateNodeAssertException(node, node.Children, $"Failed while validating node {node.Children[i]} at {i}.", e);
                }
            }

            if (i < node.Children.Count)
            {
                throw new IntermediateNodeAssertException(node, node.Children, $"The node has extra child {node.Children[i]} at {i}.");
            }
        }

        public static void AnnotationEquals(IntermediateNode node, object value)
        {
            AnnotationEquals(node, value, value);
        }

        public static void AnnotationEquals(IntermediateNode node, object key, object value)
        {
            try
            {
                Assert.NotNull(node);
                Assert.Equal(value, node.Annotations[key]);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(node, node.Children, e.Message, e);
            }
        }

        public static void HasAnnotation(IntermediateNode node, object key)
        {
            try
            {
                Assert.NotNull(node);
                Assert.NotNull(node.Annotations[key]);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(node, node.Children, e.Message, e);
            }
        }

        public static void Html(string expected, IntermediateNode node)
        {
            try
            {
                var html = Assert.IsType<HtmlContentIntermediateNode>(node);
                var content = new StringBuilder();
                for (var i = 0; i < html.Children.Count; i++)
                {
                    var token = Assert.IsType<IntermediateToken>(html.Children[i]);
                    Assert.Equal(IntermediateToken.TokenKind.Html, token.Kind);
                    content.Append(token.Content);
                }

                Assert.Equal(expected, content.ToString());
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(node, node.Children, e.Message, e);
            }
        }

        public static void CSharpCode(string expected, IntermediateNode node)
        {
            try
            {
                var statement = Assert.IsType<CSharpCodeIntermediateNode>(node);
                var content = new StringBuilder();
                for (var i = 0; i < statement.Children.Count; i++)
                {
                    var token = Assert.IsType<IntermediateToken>(statement.Children[i]);
                    Assert.Equal(IntermediateToken.TokenKind.CSharp, token.Kind);
                    content.Append(token.Content);
                }

                Assert.Equal(expected, content.ToString());
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(node, node.Children, e.Message, e);
            }
        }

        public static void Directive(string expectedName, IntermediateNode node, params Action<IntermediateNode>[] childValidators)
        {
            try
            {
                var directive = Assert.IsType<DirectiveIntermediateNode>(node);
                Assert.Equal(expectedName, directive.Name);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(node, node.Children, e.Message, e);
            }

            Children(node, childValidators);
        }

        public static void DirectiveToken(DirectiveTokenKind expectedKind, string expectedContent, IntermediateNode node)
        {
            try
            {
                var token = Assert.IsType<DirectiveTokenIntermediateNode>(node);
                Assert.Equal(expectedKind, token.Descriptor.Kind);
                Assert.Equal(expectedContent, token.Content);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(node, node.Children, e.Message, e);
            }
        }

        public static void Using(string expected, IntermediateNode node)
        {
            try
            {
                var @using = Assert.IsType<UsingDirectiveIntermediateNode>(node);
                Assert.Equal(expected, @using.Content);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(node, node.Children, e.Message, e);
            }
        }

        public static void ConditionalAttribute(
            string prefix,
            string name,
            string suffix,
            IntermediateNode node,
            params Action<IntermediateNode>[] valueValidators)
        {
            var attribute = Assert.IsType<HtmlAttributeIntermediateNode>(node);

            try
            {
                Assert.Equal(prefix, attribute.Prefix);
                Assert.Equal(name, attribute.Name);
                Assert.Equal(suffix, attribute.Suffix);

                Children(attribute, valueValidators);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(attribute, attribute.Children, e.Message, e);
            }
        }

        public static void CSharpExpressionAttributeValue(string prefix, string expected, IntermediateNode node)
        {
            var attributeValue = Assert.IsType<CSharpExpressionAttributeValueIntermediateNode>(node);

            try
            {
                var content = new StringBuilder();
                for (var i = 0; i < attributeValue.Children.Count; i++)
                {
                    var token = Assert.IsType<IntermediateToken>(attributeValue.Children[i]);
                    Assert.True(token.IsCSharp);
                    content.Append(token.Content);
                }

                Assert.Equal(prefix, attributeValue.Prefix);
                Assert.Equal(expected, content.ToString());
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(attributeValue, attributeValue.Children, e.Message, e);
            }
        }

        public static void LiteralAttributeValue(string prefix, string expected, IntermediateNode node)
        {
            var attributeValue = Assert.IsType<HtmlAttributeValueIntermediateNode>(node);
            
            try
            {
                var content = new StringBuilder();
                for (var i = 0; i < attributeValue.Children.Count; i++)
                {
                    var token = Assert.IsType<IntermediateToken>(attributeValue.Children[i]);
                    Assert.True(token.IsHtml);
                    content.Append(token.Content);
                }

                Assert.Equal(prefix, attributeValue.Prefix);
                Assert.Equal(expected, content.ToString());
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(attributeValue, e.Message);
            }
        }

        public static void CSharpExpression(string expected, IntermediateNode node)
        {
            try
            {
                var cSharp = Assert.IsType<CSharpExpressionIntermediateNode>(node);

                var content = new StringBuilder();
                for (var i = 0; i < cSharp.Children.Count; i++)
                {
                    var token = Assert.IsType<IntermediateToken>(cSharp.Children[i]);
                    Assert.Equal(IntermediateToken.TokenKind.CSharp, token.Kind);
                    content.Append(token.Content);
                }

                Assert.Equal(expected, content.ToString());
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(node, node.Children, e.Message, e);
            }
        }

        public static void BeginInstrumentation(string expected, IntermediateNode node)
        {
            try
            {
                var beginNode = Assert.IsType<CSharpCodeIntermediateNode>(node);
                var content = new StringBuilder();
                for (var i = 0; i < beginNode.Children.Count; i++)
                {
                    var token = Assert.IsType<IntermediateToken>(beginNode.Children[i]);
                    Assert.True(token.IsCSharp);
                    content.Append(token.Content);
                }

                Assert.Equal($"BeginContext({expected});", content.ToString());
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(node, node.Children, e.Message, e);
            }
        }

        public static void EndInstrumentation(IntermediateNode node)
        {
            try
            {
                var endNode = Assert.IsType<CSharpCodeIntermediateNode>(node);
                var content = new StringBuilder();
                for (var i = 0; i < endNode.Children.Count; i++)
                {
                    var token = Assert.IsType<IntermediateToken>(endNode.Children[i]);
                    Assert.Equal(IntermediateToken.TokenKind.CSharp, token.Kind);
                    content.Append(token.Content);
                }
                Assert.Equal("EndContext();", content.ToString());
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(node, node.Children, e.Message, e);
            }
        }

        internal static void TagHelperFieldDeclaration(IntermediateNode node, params string[] tagHelperTypes)
        {
            var declareTagHelperFields = Assert.IsType<DeclareTagHelperFieldsIntermediateNode>(node);

            try
            {
                Assert.Equal(tagHelperTypes, declareTagHelperFields.UsedTagHelperTypeNames);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(declareTagHelperFields, e.Message);
            }
        }

        internal static void DeclarePreallocatedTagHelperAttribute(
            IntermediateNode node,
            string attributeName,
            string value,
            AttributeStructure valueStyle)
        {
            var declarePreallocatedTagHelperAttribute = Assert.IsType<DeclarePreallocatedTagHelperAttributeIntermediateNode>(node);

            try
            {
                Assert.Equal(attributeName, declarePreallocatedTagHelperAttribute.Name);
                Assert.Equal(value, declarePreallocatedTagHelperAttribute.Value);
                Assert.Equal(valueStyle, declarePreallocatedTagHelperAttribute.AttributeStructure);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(declarePreallocatedTagHelperAttribute, e.Message);
            }
        }

        internal static void TagHelper(string tagName, TagMode tagMode, IEnumerable<TagHelperDescriptor> tagHelpers, IntermediateNode node, params Action<IntermediateNode>[] childValidators)
        {
            var tagHelperNode = Assert.IsType<TagHelperIntermediateNode>(node);

            try
            {
                Assert.Equal(tagName, tagHelperNode.TagName);
                Assert.Equal(tagMode, tagHelperNode.TagMode);

                Assert.Equal(tagHelpers, tagHelperNode.TagHelpers, TagHelperDescriptorComparer.CaseSensitive);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(tagHelperNode, e.Message);
            }

            Children(node, childValidators);
        }

        internal static void TagHelperBody(IEnumerable<TagHelperDescriptor> tagHelpers, IntermediateNode node)
        {
            var tagHelperBodyNode = Assert.IsType<TagHelperBodyIntermediateNode>(node);

            try
            {
                Assert.Equal(tagHelpers, tagHelperBodyNode.TagHelpers, TagHelperDescriptorComparer.CaseSensitive);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(tagHelperBodyNode, e.Message);
            }
        }

        internal static void TagHelperHtmlAttribute(
            string name,
            AttributeStructure valueStyle,
            IntermediateNode node,
            params Action<IntermediateNode>[] valueValidators)
        {
            var tagHelperHtmlAttribute = Assert.IsType<AddTagHelperHtmlAttributeIntermediateNode>(node);

            try
            {
                Assert.Equal(name, tagHelperHtmlAttribute.Name);
                Assert.Equal(valueStyle, tagHelperHtmlAttribute.AttributeStructure);
                Children(tagHelperHtmlAttribute, valueValidators);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(tagHelperHtmlAttribute, tagHelperHtmlAttribute.Children, e.Message, e);
            }
        }

        internal static void SetPreallocatedTagHelperProperty(IntermediateNode node, string attributeName, string propertyName)
        {
            var setPreallocatedTagHelperProperty = Assert.IsType<SetPreallocatedTagHelperPropertyIntermediateNode>(node);

            try
            {
                Assert.Equal(attributeName, setPreallocatedTagHelperProperty.AttributeName);
                Assert.Equal(propertyName, setPreallocatedTagHelperProperty.PropertyName);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(setPreallocatedTagHelperProperty, e.Message);
            }
        }

        internal static void SetTagHelperProperty(
            string name,
            string propertyName,
            AttributeStructure valueStyle,
            IntermediateNode node,
            params Action<IntermediateNode>[] valueValidators)
        {
            var tagHelperBoundAttribute = Assert.IsType<SetTagHelperPropertyIntermediateNode>(node);

            try
            {
                Assert.Equal(name, tagHelperBoundAttribute.AttributeName);
                Assert.Equal(propertyName, tagHelperBoundAttribute.PropertyName);
                Assert.Equal(valueStyle, tagHelperBoundAttribute.AttributeStructure);
                Children(tagHelperBoundAttribute, valueValidators);
            }
            catch (XunitException e)
            {
                throw new IntermediateNodeAssertException(tagHelperBoundAttribute, tagHelperBoundAttribute.Children, e.Message, e);
            }
        }

        private class IntermediateNodeAssertException : XunitException
        {
            public IntermediateNodeAssertException(IntermediateNode node, string userMessage)
                : base(Format(node, null, null, userMessage))
            {
                Node = node;
            }

            public IntermediateNodeAssertException(IntermediateNode node, IEnumerable<IntermediateNode> nodes, string userMessage)
                : base(Format(node, null, nodes, userMessage))
            {
                Node = node;
                Nodes = nodes;
            }

            public IntermediateNodeAssertException(
                IntermediateNode node,
                IEnumerable<IntermediateNode> nodes,
                string userMessage,
                Exception innerException)
                : base(Format(node, null, nodes, userMessage), innerException)
            {
            }

            public IntermediateNodeAssertException(
                IntermediateNode node,
                IntermediateNode[] ancestors,
                IEnumerable<IntermediateNode> nodes,
                string userMessage,
                Exception innerException)
                : base(Format(node, ancestors, nodes, userMessage), innerException)
            {
            }

            public IntermediateNode Node { get; }

            public IEnumerable<IntermediateNode> Nodes { get; }

            private static string Format(IntermediateNode node, IntermediateNode[] ancestors, IEnumerable<IntermediateNode> nodes, string userMessage)
            {
                var builder = new StringBuilder();
                builder.AppendLine(userMessage);
                builder.AppendLine();

                if (nodes != null)
                {
                    builder.AppendLine("Nodes:");

                    foreach (var n in nodes)
                    {
                        builder.AppendLine(n.ToString());
                    }

                    builder.AppendLine();
                }


                builder.AppendLine("Path:");

                if (ancestors != null)
                {
                    builder.AppendLine();
                    builder.AppendLine("Path:");

                    foreach (var ancestor in ancestors)
                    {
                        builder.AppendLine(ancestor.ToString());
                    }
                }

                return builder.ToString();
            }
        }
    }
}
