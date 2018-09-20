// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Legacy;
using Microsoft.AspNetCore.Razor.Language.Syntax;

namespace Microsoft.AspNetCore.Razor.Language
{
    internal class DefaultRazorTagHelperBinderPhase : RazorEnginePhaseBase, IRazorTagHelperBinderPhase
    {
        protected override void ExecuteCore(RazorCodeDocument codeDocument)
        {
            var syntaxTree = codeDocument.GetSyntaxTree();
            ThrowForMissingDocumentDependency(syntaxTree);

            if (syntaxTree is LegacyRazorSyntaxTree)
            {
                LegacyExecuteCore(codeDocument);
                return;
            }

            var descriptors = codeDocument.GetTagHelpers();
            if (descriptors == null)
            {
                var feature = Engine.Features.OfType<ITagHelperFeature>().FirstOrDefault();
                if (feature == null)
                {
                    // No feature, nothing to do.
                    return;
                }

                descriptors = feature.GetDescriptors();
            }

            // We need to find directives in all of the *imports* as well as in the main razor file
            //
            // The imports come logically before the main razor file and are in the order they
            // should be processed.
            var visitor = new DirectiveVisitor(descriptors);
            var imports = codeDocument.GetImportSyntaxTrees();
            if (imports != null)
            {
                for (var i = 0; i < imports.Count; i++)
                {
                    var import = imports[i];
                    visitor.Visit(import.Root);
                }
            }

            visitor.Visit(syntaxTree.Root);

            var tagHelperPrefix = visitor.TagHelperPrefix;
            descriptors = visitor.Matches.ToArray();

            var context = TagHelperDocumentContext.Create(tagHelperPrefix, descriptors);
            codeDocument.SetTagHelperContext(context);

            if (descriptors.Count == 0)
            {
                // No descriptors, no-op.
                return;
            }

            var errorSink = new ErrorSink();
            var rewriter = new TagHelperParseTreeRewriter(tagHelperPrefix, descriptors, syntaxTree.Options.FeatureFlags);

            var root = syntaxTree.Root;
            root = rewriter.Rewrite(root, errorSink);

            var errorList = new List<RazorDiagnostic>();
            errorList.AddRange(errorSink.Errors);

            errorList.AddRange(descriptors.SelectMany(d => d.GetAllDiagnostics()));

            var diagnostics = CombineErrors(syntaxTree.Diagnostics, errorList);

            var newSyntaxTree = RazorSyntaxTree.Create(root, syntaxTree.Source, diagnostics, syntaxTree.Options);
            codeDocument.SetSyntaxTree(newSyntaxTree);
        }

        private void LegacyExecuteCore(RazorCodeDocument codeDocument)
        {
            var syntaxTree = codeDocument.GetSyntaxTree();

            var descriptors = codeDocument.GetTagHelpers();
            if (descriptors == null)
            {
                var feature = Engine.Features.OfType<ITagHelperFeature>().FirstOrDefault();
                if (feature == null)
                {
                    // No feature, nothing to do.
                    return;
                }

                descriptors = feature.GetDescriptors();
            }

            // We need to find directives in all of the *imports* as well as in the main razor file
            //
            // The imports come logically before the main razor file and are in the order they
            // should be processed.
            var visitor = new LegacyDirectiveVisitor(descriptors);
            var imports = codeDocument.GetImportSyntaxTrees();
            if (imports != null)
            {
                for (var i = 0; i < imports.Count; i++)
                {
                    var import = imports[i];
                    visitor.VisitBlock(import.LegacyRoot);
                }
            }

            visitor.VisitBlock(syntaxTree.LegacyRoot);

            var tagHelperPrefix = visitor.TagHelperPrefix;
            descriptors = visitor.Matches.ToArray();

            var context = TagHelperDocumentContext.Create(tagHelperPrefix, descriptors);
            codeDocument.SetTagHelperContext(context);

            if (descriptors.Count == 0)
            {
                // No descriptors, no-op.
                return;
            }

            var errorSink = new ErrorSink();
            var rewriter = new LegacyTagHelperParseTreeRewriter(tagHelperPrefix, descriptors, syntaxTree.Options.FeatureFlags);

            var root = syntaxTree.LegacyRoot;
            root = rewriter.Rewrite(root, errorSink);

            var errorList = new List<RazorDiagnostic>();
            errorList.AddRange(errorSink.Errors);

            errorList.AddRange(descriptors.SelectMany(d => d.GetAllDiagnostics()));

            var diagnostics = CombineErrors(syntaxTree.Diagnostics, errorList);

            var newSyntaxTree = RazorSyntaxTree.Create(root, syntaxTree.Source, diagnostics, syntaxTree.Options);
            codeDocument.SetSyntaxTree(newSyntaxTree);
        }

        private static bool MatchesDirective(TagHelperDescriptor descriptor, string typePattern, string assemblyName)
        {
            if (!string.Equals(descriptor.AssemblyName, assemblyName, StringComparison.Ordinal))
            {
                return false;
            }

            if (typePattern.EndsWith("*", StringComparison.Ordinal))
            {
                if (typePattern.Length == 1)
                {
                    // TypePattern is "*".
                    return true;
                }

                var lookupTypeName = typePattern.Substring(0, typePattern.Length - 1);

                return descriptor.Name.StartsWith(lookupTypeName, StringComparison.Ordinal);
            }

            return string.Equals(descriptor.Name, typePattern, StringComparison.Ordinal);
        }

        private static int GetErrorLength(string directiveText)
        {
            var nonNullLength = directiveText == null ? 1 : directiveText.Length;
            var normalizeEmptyStringLength = Math.Max(nonNullLength, 1);

            return normalizeEmptyStringLength;
        }

        private IReadOnlyList<RazorDiagnostic> CombineErrors(IReadOnlyList<RazorDiagnostic> errors1, IReadOnlyList<RazorDiagnostic> errors2)
        {
            var combinedErrors = new List<RazorDiagnostic>(errors1.Count + errors2.Count);
            combinedErrors.AddRange(errors1);
            combinedErrors.AddRange(errors2);

            return combinedErrors;
        }

        internal class DirectiveVisitor : SyntaxRewriter
        {
            private IReadOnlyList<TagHelperDescriptor> _tagHelpers;

            public DirectiveVisitor(IReadOnlyList<TagHelperDescriptor> tagHelpers)
            {
                _tagHelpers = tagHelpers;
            }

            public string TagHelperPrefix { get; private set; }

            public HashSet<TagHelperDescriptor> Matches { get; } = new HashSet<TagHelperDescriptor>();

            public override SyntaxNode VisitRazorDirectiveBody(RazorDirectiveBodySyntax node)
            {
                var context = node.GetSpanContext();
                if (context.ChunkGenerator is AddTagHelperChunkGenerator addTagHelper)
                {
                    if (addTagHelper.AssemblyName == null)
                    {
                        // Skip this one, it's an error
                        return base.VisitRazorDirectiveBody(node);
                    }

                    if (!AssemblyContainsTagHelpers(addTagHelper.AssemblyName, _tagHelpers))
                    {
                        // No tag helpers in the assembly.
                        return base.VisitRazorDirectiveBody(node);
                    }

                    for (var i = 0; i < _tagHelpers.Count; i++)
                    {
                        var tagHelper = _tagHelpers[i];
                        if (MatchesDirective(tagHelper, addTagHelper.TypePattern, addTagHelper.AssemblyName))
                        {
                            Matches.Add(tagHelper);
                        }
                    }
                }
                else if (context.ChunkGenerator is RemoveTagHelperChunkGenerator removeTagHelper)
                {
                    if (removeTagHelper.AssemblyName == null)
                    {
                        // Skip this one, it's an error
                        return base.VisitRazorDirectiveBody(node);
                    }


                    if (!AssemblyContainsTagHelpers(removeTagHelper.AssemblyName, _tagHelpers))
                    {
                        // No tag helpers in the assembly.
                        return base.VisitRazorDirectiveBody(node);
                    }

                    for (var i = 0; i < _tagHelpers.Count; i++)
                    {
                        var tagHelper = _tagHelpers[i];
                        if (MatchesDirective(tagHelper, removeTagHelper.TypePattern, removeTagHelper.AssemblyName))
                        {
                            Matches.Remove(tagHelper);
                        }
                    }
                }
                else if (context.ChunkGenerator is TagHelperPrefixDirectiveChunkGenerator tagHelperPrefix)
                {
                    if (!string.IsNullOrEmpty(tagHelperPrefix.DirectiveText))
                    {
                        // We only expect to see a single one of these per file, but that's enforced at another level.
                        TagHelperPrefix = tagHelperPrefix.DirectiveText;
                    }
                }

                return base.VisitRazorDirectiveBody(node);
            }

            private bool AssemblyContainsTagHelpers(string assemblyName, IReadOnlyList<TagHelperDescriptor> tagHelpers)
            {
                for (var i = 0; i < tagHelpers.Count; i++)
                {
                    if (string.Equals(tagHelpers[i].AssemblyName, assemblyName, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        // Internal for testing.
        internal class LegacyDirectiveVisitor : ParserVisitor
        {
            private IReadOnlyList<TagHelperDescriptor> _tagHelpers;

            public LegacyDirectiveVisitor(IReadOnlyList<TagHelperDescriptor> tagHelpers)
            {
                _tagHelpers = tagHelpers;
            }

            public string TagHelperPrefix { get; private set; }

            public HashSet<TagHelperDescriptor> Matches { get; } = new HashSet<TagHelperDescriptor>();

            public override void VisitAddTagHelperSpan(AddTagHelperChunkGenerator chunkGenerator, Span span)
            {
                if (chunkGenerator.AssemblyName == null)
                {
                    // Skip this one, it's an error
                    return;
                }

                if (!AssemblyContainsTagHelpers(chunkGenerator.AssemblyName, _tagHelpers))
                {
                    // No tag helpers in the assembly.
                    return;
                }

                for (var i = 0; i < _tagHelpers.Count; i++)
                {
                    var tagHelper = _tagHelpers[i];
                    if (MatchesDirective(tagHelper, chunkGenerator.TypePattern, chunkGenerator.AssemblyName))
                    {
                        Matches.Add(tagHelper);
                    }
                }
            }

            public override void VisitRemoveTagHelperSpan(RemoveTagHelperChunkGenerator chunkGenerator, Span span)
            {
                if (chunkGenerator.AssemblyName == null)
                {
                    // Skip this one, it's an error
                    return;
                }


                if (!AssemblyContainsTagHelpers(chunkGenerator.AssemblyName, _tagHelpers))
                {
                    // No tag helpers in the assembly.
                    return;
                }

                for (var i = 0; i < _tagHelpers.Count; i++)
                {
                    var tagHelper = _tagHelpers[i];
                    if (MatchesDirective(tagHelper, chunkGenerator.TypePattern, chunkGenerator.AssemblyName))
                    {
                        Matches.Remove(tagHelper);
                    }
                }
            }

            public override void VisitTagHelperPrefixDirectiveSpan(TagHelperPrefixDirectiveChunkGenerator chunkGenerator, Span span)
            {
                if (!string.IsNullOrEmpty(chunkGenerator.DirectiveText))
                {
                    // We only expect to see a single one of these per file, but that's enforced at another level.
                    TagHelperPrefix = chunkGenerator.DirectiveText;
                }
            }

            private bool AssemblyContainsTagHelpers(string assemblyName, IReadOnlyList<TagHelperDescriptor> tagHelpers)
            {
                for (var i = 0; i < tagHelpers.Count; i++)
                {
                    if (string.Equals(tagHelpers[i].AssemblyName, assemblyName, StringComparison.Ordinal))
                    {
                        return true;
                    }
                }

                return false;
            }
        }
    }
}
