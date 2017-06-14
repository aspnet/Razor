﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Extensions;
using Microsoft.AspNetCore.Razor.Language.Intermediate;
using Microsoft.AspNetCore.Razor.Language.Legacy;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultRazorIRLoweringPhase : RazorEnginePhaseBase, IRazorIRLoweringPhase
    {
        private IRazorCodeGenerationOptionsFeature[] _optionsCallbacks;

        protected override void OnIntialized()
        {
            _optionsCallbacks = Engine.Features.OfType<IRazorCodeGenerationOptionsFeature>().OrderBy(f => f.Order).ToArray();
        }

        protected override void ExecuteCore(RazorCodeDocument codeDocument)
        {
            var syntaxTree = codeDocument.GetSyntaxTree();
            ThrowForMissingDocumentDependency(syntaxTree);

            // This might not have been set if there are no tag helpers.
            var tagHelperContext = codeDocument.GetTagHelperContext();

            var document = new DocumentIRNode();
            var builder = RazorIRBuilder.Create(document);

            document.Options = CreateCodeGenerationOptions();

            var checksum = ChecksumIRNode.Create(codeDocument.Source);
            builder.Insert(0, checksum);

            var namespaces = new Dictionary<string, SourceSpan?>(StringComparer.Ordinal);

            // The import documents should be inserted logically before the main document.
            var imports = codeDocument.GetImportSyntaxTrees();
            if (imports != null)
            {
                var importsVisitor = new ImportsVisitor(document, builder, namespaces);

                for (var j = 0; j < imports.Count; j++)
                {
                    var import = imports[j];

                    importsVisitor.FilePath = import.Source.FilePath;
                    importsVisitor.VisitBlock(import.Root);
                }
            }

            var tagHelperPrefix = tagHelperContext?.Prefix;
            var visitor = new MainSourceVisitor(document, builder, namespaces, tagHelperPrefix)
            {
                FilePath = syntaxTree.Source.FilePath,
            };

            visitor.VisitBlock(syntaxTree.Root);

            // In each lowering piece above, namespaces were tracked. We render them here to ensure every
            // lowering action has a chance to add a source location to a namespace. Ultimately, closest wins.

            var i = builder.Current.Children.IndexOf(checksum) + 1;
            foreach (var @namespace in namespaces)
            {
                var @using = new UsingStatementIRNode()
                {
                    Content = @namespace.Key,
                    Source = @namespace.Value,
                };

                builder.Insert(i++, @using);
            }

            // The document should contain all errors that currently exist in the system. This involves
            // adding the errors from the primary and imported syntax trees.

            for (i = 0; i < syntaxTree.Diagnostics.Count; i++)
            {
                document.Diagnostics.Add(syntaxTree.Diagnostics[i]);
            }

            if (imports != null)
            {
                for (i = 0; i < imports.Count; i++)
                {
                    var import = imports[i];
                    for (var j = 0; j < import.Diagnostics.Count; j++)
                    {
                        document.Diagnostics.Add(import.Diagnostics[j]);
                    }
                }
            }

            codeDocument.SetIRDocument(document);
        }

        private RazorCodeGenerationOptions CreateCodeGenerationOptions()
        {
            var builder = new DefaultRazorCodeGenerationOptionsBuilder();
            for (var i = 0; i < _optionsCallbacks.Length; i++)
            {
                _optionsCallbacks[i].Configure(builder);
            }

            return builder.Build();
        }

        private class LoweringVisitor : ParserVisitor
        {
            protected readonly RazorIRBuilder _builder;
            protected readonly DocumentIRNode _document;
            protected readonly Dictionary<string, SourceSpan?> _namespaces;

            public LoweringVisitor(DocumentIRNode document, RazorIRBuilder builder, Dictionary<string, SourceSpan?> namespaces)
            {
                _document = document;
                _builder = builder;
                _namespaces = namespaces;
            }

            public string FilePath { get; set; }

            public override void VisitImportSpan(AddImportChunkGenerator chunkGenerator, Span span)
            {
                var namespaceImport = chunkGenerator.Namespace.Trim();
                var namespaceSpan = BuildSourceSpanFromNode(span);
                _namespaces[namespaceImport] = namespaceSpan;
            }

            public override void VisitAddTagHelperSpan(AddTagHelperChunkGenerator chunkGenerator, Span span)
            {
                _builder.Push(new DirectiveIRNode()
                {
                    Name = CSharpCodeParser.AddTagHelperDirectiveDescriptor.Directive,
                    Descriptor = CSharpCodeParser.AddTagHelperDirectiveDescriptor,
                    Source = BuildSourceSpanFromNode(span),
                });

                _builder.Add(new DirectiveTokenIRNode()
                {
                    Content = chunkGenerator.LookupText,
                    Descriptor = CSharpCodeParser.AddTagHelperDirectiveDescriptor.Tokens.First(),
                    Source = BuildSourceSpanFromNode(span),
                });

                _builder.Pop();
            }

            public override void VisitRemoveTagHelperSpan(RemoveTagHelperChunkGenerator chunkGenerator, Span span)
            {
                _builder.Push(new DirectiveIRNode()
                {
                    Name = CSharpCodeParser.RemoveTagHelperDirectiveDescriptor.Directive,
                    Descriptor = CSharpCodeParser.RemoveTagHelperDirectiveDescriptor,
                    Source = BuildSourceSpanFromNode(span),
                });

                _builder.Add(new DirectiveTokenIRNode()
                {
                    Content = chunkGenerator.LookupText,
                    Descriptor = CSharpCodeParser.RemoveTagHelperDirectiveDescriptor.Tokens.First(),
                    Source = BuildSourceSpanFromNode(span),
                });

                _builder.Pop();
            }

            public override void VisitTagHelperPrefixDirectiveSpan(TagHelperPrefixDirectiveChunkGenerator chunkGenerator, Span span)
            {
                _builder.Push(new DirectiveIRNode()
                {
                    Name = CSharpCodeParser.TagHelperPrefixDirectiveDescriptor.Directive,
                    Descriptor = CSharpCodeParser.TagHelperPrefixDirectiveDescriptor,
                    Source = BuildSourceSpanFromNode(span),
                });

                _builder.Add(new DirectiveTokenIRNode()
                {
                    Content = chunkGenerator.Prefix,
                    Descriptor = CSharpCodeParser.TagHelperPrefixDirectiveDescriptor.Tokens.First(),
                    Source = BuildSourceSpanFromNode(span),
                });

                _builder.Pop();
            }

            protected SourceSpan? BuildSourceSpanFromNode(SyntaxTreeNode node)
            {
                var location = node.Start;
                if (location == SourceLocation.Undefined)
                {
                    return null;
                }

                var span = new SourceSpan(
                    node.Start.FilePath ?? FilePath,
                    node.Start.AbsoluteIndex,
                    node.Start.LineIndex,
                    node.Start.CharacterIndex,
                    node.Length);
                return span;
            }
        }

        private class ImportsVisitor : LoweringVisitor
        {
            // Imports only supports usings and single-line directives. We only want to include directive tokens
            // when we're inside a single line directive. Also single line directives can't nest which makes
            // this simple.
            private bool _insideLineDirective;

            public ImportsVisitor(DocumentIRNode document, RazorIRBuilder builder, Dictionary<string, SourceSpan?> namespaces)
                : base(document, builder, namespaces)
            {
            }

            public override void VisitDirectiveToken(DirectiveTokenChunkGenerator chunkGenerator, Span span)
            {
                if (_insideLineDirective)
                {
                    _builder.Add(new DirectiveTokenIRNode()
                    {
                        Content = span.Content,
                        Descriptor = chunkGenerator.Descriptor,
                        Source = BuildSourceSpanFromNode(span),
                    });
                }
            }

            public override void VisitDirectiveBlock(DirectiveChunkGenerator chunkGenerator, Block block)
            {
                if (chunkGenerator.Descriptor.Kind == DirectiveKind.SingleLine)
                {
                    _insideLineDirective = true;

                    _builder.Push(new DirectiveIRNode()
                    {
                        Name = chunkGenerator.Descriptor.Directive,
                        Descriptor = chunkGenerator.Descriptor,
                        Source = BuildSourceSpanFromNode(block),
                    });

                    base.VisitDirectiveBlock(chunkGenerator, block);

                    _builder.Pop();

                    _insideLineDirective = false;
                }
            }
        }

        private class MainSourceVisitor : LoweringVisitor
        {
            private RazorIRNodeReference? _tagHelperFieldDeclaration;
            private HashSet<string> _usedTagHelperTypeNames;
            private readonly string _tagHelperPrefix;

            public MainSourceVisitor(DocumentIRNode document, RazorIRBuilder builder, Dictionary<string, SourceSpan?> namespaces, string tagHelperPrefix)
                : base(document, builder, namespaces)
            {
                _tagHelperPrefix = tagHelperPrefix;
            }

            public override void VisitDirectiveToken(DirectiveTokenChunkGenerator chunkGenerator, Span span)
            {
                _builder.Add(new DirectiveTokenIRNode()
                {
                    Content = span.Content,
                    Descriptor = chunkGenerator.Descriptor,
                    Source = BuildSourceSpanFromNode(span),
                });
            }

            public override void VisitDirectiveBlock(DirectiveChunkGenerator chunkGenerator, Block block)
            {
                _builder.Push(new DirectiveIRNode()
                {
                    Name = chunkGenerator.Descriptor.Directive,
                    Descriptor = chunkGenerator.Descriptor,
                    Source = BuildSourceSpanFromNode(block),
                });

                VisitDefault(block);

                _builder.Pop();
            }

            // Example
            // <input` checked="hello-world @false"`/>
            //  Name=checked
            //  Prefix= checked="
            //  Suffix="
            public override void VisitAttributeBlock(AttributeBlockChunkGenerator chunkGenerator, Block block)
            {
                _builder.Push(new HtmlAttributeIRNode()
                {
                    Name = chunkGenerator.Name,
                    Prefix = chunkGenerator.Prefix,
                    Suffix = chunkGenerator.Suffix,
                    Source = BuildSourceSpanFromNode(block),
                });

                VisitDefault(block);

                _builder.Pop();
            }

            // Example
            // <input checked="hello-world `@false`"/>
            //  Prefix= (space)
            //  Children will contain a token for @false.
            public override void VisitDynamicAttributeBlock(DynamicAttributeBlockChunkGenerator chunkGenerator, Block block)
            {
                var firstChild = block.Children.FirstOrDefault(c => c.IsBlock) as Block;
                if (firstChild == null || firstChild.Type == BlockKindInternal.Expression)
                {
                    _builder.Push(new CSharpExpressionAttributeValueIRNode()
                    {
                        Prefix = chunkGenerator.Prefix,
                        Source = BuildSourceSpanFromNode(block),
                    });
                }
                else
                {
                    _builder.Push(new CSharpCodeAttributeValueIRNode()
                    {
                        Prefix = chunkGenerator.Prefix,
                        Source = BuildSourceSpanFromNode(block),
                    });
                }

                VisitDefault(block);

                _builder.Pop();
            }

            public override void VisitLiteralAttributeSpan(LiteralAttributeChunkGenerator chunkGenerator, Span span)
            {
                _builder.Push(new HtmlAttributeValueIRNode()
                {
                    Prefix = chunkGenerator.Prefix,
                    Source = BuildSourceSpanFromNode(span),
                });

                var location = chunkGenerator.Value.Location;
                SourceSpan? valueSpan = null;
                if (location != SourceLocation.Undefined)
                {
                    valueSpan = new SourceSpan(
                        location.FilePath ?? FilePath,
                        location.AbsoluteIndex,
                        location.LineIndex,
                        location.CharacterIndex,
                        chunkGenerator.Value.Value.Length);
                }

                _builder.Add(new RazorIRToken()
                {
                    Content = chunkGenerator.Value,
                    Kind = RazorIRToken.TokenKind.Html,
                    Source = valueSpan
                });

                _builder.Pop();
            }

            public override void VisitTemplateBlock(TemplateBlockChunkGenerator chunkGenerator, Block block)
            {
                var templateNode = new TemplateIRNode();
                _builder.Push(templateNode);

                VisitDefault(block);

                _builder.Pop();

                if (templateNode.Children.Count > 0)
                {
                    var sourceRangeStart = templateNode
                        .Children
                        .FirstOrDefault(child => child.Source != null)
                        ?.Source;

                    if (sourceRangeStart != null)
                    {
                        var contentLength = templateNode.Children.Sum(child => child.Source?.Length ?? 0);

                        templateNode.Source = new SourceSpan(
                            sourceRangeStart.Value.FilePath ?? FilePath,
                            sourceRangeStart.Value.AbsoluteIndex,
                            sourceRangeStart.Value.LineIndex,
                            sourceRangeStart.Value.CharacterIndex,
                            contentLength);
                    }
                }
            }

            // CSharp expressions are broken up into blocks and spans because Razor allows Razor comments
            // inside an expression.
            // Ex:
            //      @DateTime.@*This is a comment*@Now
            //
            // We need to capture this in the IR so that we can give each piece the correct source mappings
            public override void VisitExpressionBlock(ExpressionChunkGenerator chunkGenerator, Block block)
            {
                if (_builder.Current is CSharpExpressionAttributeValueIRNode)
                {
                    VisitDefault(block);
                    return;
                }

                var expressionNode = new CSharpExpressionIRNode();

                _builder.Push(expressionNode);

                VisitDefault(block);

                _builder.Pop();

                if (expressionNode.Children.Count > 0)
                {
                    var sourceRangeStart = expressionNode
                        .Children
                        .FirstOrDefault(child => child.Source != null)
                        ?.Source;

                    if (sourceRangeStart != null)
                    {
                        var contentLength = expressionNode.Children.Sum(child => child.Source?.Length ?? 0);

                        expressionNode.Source = new SourceSpan(
                            sourceRangeStart.Value.FilePath ?? FilePath,
                            sourceRangeStart.Value.AbsoluteIndex,
                            sourceRangeStart.Value.LineIndex,
                            sourceRangeStart.Value.CharacterIndex,
                            contentLength);
                    }
                }
            }

            public override void VisitExpressionSpan(ExpressionChunkGenerator chunkGenerator, Span span)
            {
                _builder.Add(new RazorIRToken()
                {
                    Content = span.Content,
                    Kind = RazorIRToken.TokenKind.CSharp,
                    Source = BuildSourceSpanFromNode(span),
                });
            }

            public override void VisitStatementSpan(StatementChunkGenerator chunkGenerator, Span span)
            {
                var isAttributeValue = _builder.Current is CSharpCodeAttributeValueIRNode;

                if (!isAttributeValue)
                {
                    var statementNode = new CSharpCodeIRNode()
                    {
                        Source = BuildSourceSpanFromNode(span)
                    };
                    _builder.Push(statementNode);
                }

                _builder.Add(new RazorIRToken()
                {
                    Content = span.Content,
                    Kind = RazorIRToken.TokenKind.CSharp,
                    Source = BuildSourceSpanFromNode(span),
                });

                if (!isAttributeValue)
                {
                    _builder.Pop();
                }
            }

            public override void VisitMarkupSpan(MarkupChunkGenerator chunkGenerator, Span span)
            {
                if (span.Symbols.Count == 1)
                {
                    var symbol = span.Symbols[0] as HtmlSymbol;
                    if (symbol != null &&
                        symbol.Type == HtmlSymbolType.Unknown &&
                        symbol.Content.Length == 0)
                    {
                        // We don't want to create IR nodes for marker symbols.
                        return;
                    }
                }

                var source = BuildSourceSpanFromNode(span);
                var currentChildren = _builder.Current.Children;
                if (currentChildren.Count > 0 && currentChildren[currentChildren.Count - 1] is HtmlContentIRNode)
                {
                    var existingHtmlContent = (HtmlContentIRNode)currentChildren[currentChildren.Count - 1];

                    if (existingHtmlContent.Source == null && source == null)
                    {
                        Combine(existingHtmlContent, span);
                        return;
                    }

                    if (source != null &&
                        existingHtmlContent.Source != null &&
                        existingHtmlContent.Source.Value.FilePath == source.Value.FilePath &&
                        existingHtmlContent.Source.Value.AbsoluteIndex + existingHtmlContent.Source.Value.Length == source.Value.AbsoluteIndex)
                    {
                        Combine(existingHtmlContent, span);
                        return;
                    }
                }

                var contentNode = new HtmlContentIRNode()
                {
                    Source = source
                };
                _builder.Push(contentNode);

                _builder.Add(new RazorIRToken()
                {
                    Content = span.Content,
                    Kind = RazorIRToken.TokenKind.Html,
                    Source = source,
                });

                _builder.Pop();
            }

            public override void VisitTagHelperBlock(TagHelperChunkGenerator chunkGenerator, Block block)
            {
                var tagHelperBlock = block as TagHelperBlock;
                if (tagHelperBlock == null)
                {
                    return;
                }

                DeclareTagHelperFields(tagHelperBlock);

                var tagName = tagHelperBlock.TagName;
                if (_tagHelperPrefix != null)
                {
                    tagName = tagName.Substring(_tagHelperPrefix.Length);
                }

                _builder.Push(new TagHelperIRNode()
                {
                    TagName = tagName,
                    TagMode = tagHelperBlock.TagMode,
                    Source = BuildSourceSpanFromNode(block)
                });

                _builder.Push(new TagHelperBodyIRNode());

                VisitDefault(block);

                _builder.Pop(); // Pop InitializeTagHelperStructureIRNode

                AddTagHelperCreation(tagHelperBlock.Binding);
                AddTagHelperAttributes(tagHelperBlock.Attributes, tagHelperBlock.Binding);

                _builder.Pop(); // Pop TagHelperIRNode
            }

            private void Combine(HtmlContentIRNode node, Span span)
            {
                node.Children.Add(new RazorIRToken()
                {
                    Content = span.Content,
                    Kind = RazorIRToken.TokenKind.Html,
                    Source = BuildSourceSpanFromNode(span),
                });

                if (node.Source != null)
                {
                    Debug.Assert(node.Source.Value.FilePath != null);

                    node.Source = new SourceSpan(
                        node.Source.Value.FilePath,
                        node.Source.Value.AbsoluteIndex,
                        node.Source.Value.LineIndex,
                        node.Source.Value.CharacterIndex,
                        node.Source.Value.Length + span.Content.Length);
                }
            }

            private void DeclareTagHelperFields(TagHelperBlock block)
            {
                if (_tagHelperFieldDeclaration == null)
                {
                    var node = new FieldDeclarationIRNode();
                    node.Annotations[CommonAnnotations.InitializeTagHelperVariables] = CommonAnnotations.InitializeTagHelperVariables;
                    _document.Children.Add(node);

                    _tagHelperFieldDeclaration = new RazorIRNodeReference(_document, node);
                    _usedTagHelperTypeNames = new HashSet<string>(StringComparer.Ordinal);
                }

                foreach (var descriptor in block.Binding.Descriptors)
                {
                    var typeName = descriptor.GetTypeName();
                    if (_usedTagHelperTypeNames.Contains(typeName))
                    {
                        continue;
                    }

                    _usedTagHelperTypeNames.Add(typeName);

                    var tagHelperField = new FieldDeclarationIRNode()
                    {
                        AccessModifier = "private",
                        Name = GetTagHelperVariableName(typeName),
                        Type = "global::" + typeName
                    };
                    tagHelperField.Annotations[CommonAnnotations.TagHelperField] = CommonAnnotations.TagHelperField;
                    _tagHelperFieldDeclaration = _tagHelperFieldDeclaration.Value.InsertBefore(tagHelperField);
                }
            }

            private static string GetTagHelperVariableName(string tagHelperTypeName) => "__" + tagHelperTypeName.Replace('.', '_');

            private void AddTagHelperCreation(TagHelperBinding tagHelperBinding)
            {
                var descriptors = tagHelperBinding.Descriptors;
                foreach (var descriptor in descriptors)
                {
                    var typeName = descriptor.GetTypeName();
                    var createTagHelper = new CreateTagHelperIRNode()
                    {
                        TagHelperTypeName = typeName,
                        Descriptor = descriptor
                    };

                    _builder.Add(createTagHelper);
                }
            }

            private void AddTagHelperAttributes(IList<TagHelperAttributeNode> attributes, TagHelperBinding tagHelperBinding)
            {
                var descriptors = tagHelperBinding.Descriptors;
                var renderedBoundAttributeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                foreach (var attribute in attributes)
                {
                    var attributeValueNode = attribute.Value;
                    var associatedDescriptors = descriptors.Where(descriptor =>
                        descriptor.BoundAttributes.Any(attributeDescriptor => TagHelperMatchingConventions.CanSatisfyBoundAttribute(attribute.Name, attributeDescriptor)));

                    if (associatedDescriptors.Any() && renderedBoundAttributeNames.Add(attribute.Name))
                    {
                        if (attributeValueNode == null)
                        {
                            // Minimized attributes are not valid for bound attributes. TagHelperBlockRewriter has already
                            // logged an error if it was a bound attribute; so we can skip.
                            continue;
                        }

                        foreach (var associatedDescriptor in associatedDescriptors)
                        {
                            var associatedAttributeDescriptor = associatedDescriptor.BoundAttributes.First(
                                attributeDescriptor => TagHelperMatchingConventions.CanSatisfyBoundAttribute(attribute.Name, attributeDescriptor));
                            var tagHelperTypeName = associatedDescriptor.GetTypeName();
                            var attributePropertyName = associatedAttributeDescriptor.GetPropertyName();

                            var setTagHelperProperty = new SetTagHelperPropertyIRNode()
                            {
                                PropertyName = attributePropertyName,
                                AttributeName = attribute.Name,
                                TagHelperTypeName = tagHelperTypeName,
                                Descriptor = associatedAttributeDescriptor,
                                Binding = tagHelperBinding,
                                ValueStyle = attribute.ValueStyle,
                                Source = BuildSourceSpanFromNode(attributeValueNode),
                                IsIndexerNameMatch = TagHelperMatchingConventions.SatisfiesBoundAttributeIndexer(attribute.Name, associatedAttributeDescriptor),
                            };

                            _builder.Push(setTagHelperProperty);
                            attributeValueNode.Accept(this);
                            _builder.Pop();
                        }
                    }
                    else
                    {
                        var addHtmlAttribute = new AddTagHelperHtmlAttributeIRNode()
                        {
                            Name = attribute.Name,
                            ValueStyle = attribute.ValueStyle
                        };

                        _builder.Push(addHtmlAttribute);
                        if (attributeValueNode != null)
                        {
                            attributeValueNode.Accept(this);
                        }
                        _builder.Pop();
                    }
                }
            }
        }
    }
}
