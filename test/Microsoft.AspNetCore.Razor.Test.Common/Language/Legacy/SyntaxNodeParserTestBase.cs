// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Xunit;
using Xunit.Sdk;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    public abstract class SyntaxNodeParserTestBase : ParserTestBase
    {
        protected bool UseNewSyntaxTree { get; set; }

        internal override RazorSyntaxTree ParseDocument(RazorLanguageVersion version, string document, IEnumerable<DirectiveDescriptor> directives, bool designTime = false)
        {
            if (!UseNewSyntaxTree)
            {
                return base.ParseDocument(version, document, directives, designTime);
            }

            directives = directives ?? Array.Empty<DirectiveDescriptor>();

            var source = TestRazorSourceDocument.Create(document, filePath: null, relativePath: null, normalizeNewLines: true);

            var options = CreateParserOptions(version, directives, designTime);
            var context = new ParserContext(source, options);

            var codeParser = new CSharpCodeParser(directives, context);
            var markupParser = new HtmlMarkupParser(context);

            codeParser.HtmlParser = markupParser;
            markupParser.CodeParser = codeParser;

            var root = markupParser.ParseDocument().CreateRed();

            var diagnostics = context.ErrorSink.Errors;

            var codeDocument = RazorCodeDocument.Create(source);

            var syntaxTree = RazorSyntaxTree.Create(root, source, diagnostics, options);
            codeDocument.SetSyntaxTree(syntaxTree);

            var defaultDirectivePass = new DefaultDirectiveSyntaxTreePass();
            syntaxTree = defaultDirectivePass.Execute(codeDocument, syntaxTree);

            return syntaxTree;
        }

        internal override void AssertSyntaxTreeNodeMatchesBaseline(RazorSyntaxTree syntaxTree)
        {
            if (!UseNewSyntaxTree)
            {
                base.AssertSyntaxTreeNodeMatchesBaseline(syntaxTree);
                return;
            }

            var root = syntaxTree.NewRoot;
            var diagnostics = syntaxTree.Diagnostics;
            var filePath = syntaxTree.Source.FilePath;
            if (FileName == null)
            {
                var message = $"{nameof(AssertSyntaxTreeNodeMatchesBaseline)} should only be called from a parser test ({nameof(FileName)} is null).";
                throw new InvalidOperationException(message);
            }

            if (IsTheory)
            {
                var message = $"{nameof(AssertSyntaxTreeNodeMatchesBaseline)} should not be called from a [Theory] test.";
                throw new InvalidOperationException(message);
            }

            var baselineFileName = Path.ChangeExtension(FileName, ".stree.txt");
            var baselineDiagnosticsFileName = Path.ChangeExtension(FileName, ".diag.txt");
            var baselineClassifiedSpansFileName = Path.ChangeExtension(FileName, ".cspans.txt");
            var baselineTagHelperSpansFileName = Path.ChangeExtension(FileName, ".tspans.txt");

            if (GenerateBaselines)
            {
                // Write syntax tree baseline
                var baselineFullPath = Path.Combine(TestProjectRoot, baselineFileName);
                File.WriteAllText(baselineFullPath, SyntaxNodeSerializer.Serialize(root));

                // Write diagnostics baseline
                var baselineDiagnosticsFullPath = Path.Combine(TestProjectRoot, baselineDiagnosticsFileName);
                var lines = diagnostics.Select(SerializeDiagnostic).ToArray();
                if (lines.Any())
                {
                    File.WriteAllLines(baselineDiagnosticsFullPath, lines);
                }
                else if (File.Exists(baselineDiagnosticsFullPath))
                {
                    File.Delete(baselineDiagnosticsFullPath);
                }

                // Write classified spans baseline
                var classifiedSpansBaselineFullPath = Path.Combine(TestProjectRoot, baselineClassifiedSpansFileName);
                File.WriteAllText(classifiedSpansBaselineFullPath, ClassifiedSpanSerializer.Serialize(syntaxTree));

                // Write tag helper spans baseline
                var tagHelperSpansBaselineFullPath = Path.Combine(TestProjectRoot, baselineTagHelperSpansFileName);
                var serializedTagHelperSpans = TagHelperSpanSerializer.Serialize(syntaxTree);
                if (!string.IsNullOrEmpty(serializedTagHelperSpans))
                {
                    File.WriteAllText(tagHelperSpansBaselineFullPath, serializedTagHelperSpans);
                }
                else if (File.Exists(tagHelperSpansBaselineFullPath))
                {
                    File.Delete(tagHelperSpansBaselineFullPath);
                }

                return;
            }

            // Verify syntax tree
            var stFile = TestFile.Create(baselineFileName, GetType().GetTypeInfo().Assembly);
            if (!stFile.Exists())
            {
                throw new XunitException($"The resource {baselineFileName} was not found.");
            }

            var baseline = stFile.ReadAllText().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            SyntaxNodeVerifier.Verify(root, baseline);

            // Verify diagnostics
            var baselineDiagnostics = string.Empty;
            var diagnosticsFile = TestFile.Create(baselineDiagnosticsFileName, GetType().GetTypeInfo().Assembly);
            if (diagnosticsFile.Exists())
            {
                baselineDiagnostics = diagnosticsFile.ReadAllText();
            }

            var actualDiagnostics = string.Concat(diagnostics.Select(d => SerializeDiagnostic(d) + "\r\n"));
            Assert.Equal(baselineDiagnostics, actualDiagnostics);

            // Verify classified spans
            var classifiedSpanFile = TestFile.Create(baselineClassifiedSpansFileName, GetType().GetTypeInfo().Assembly);
            if (!classifiedSpanFile.Exists())
            {
                throw new XunitException($"The resource {baselineClassifiedSpansFileName} was not found.");
            }
            else
            {
                var classifiedSpanBaseline = new string[0];
                classifiedSpanBaseline = classifiedSpanFile.ReadAllText().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                ClassifiedSpanVerifier.Verify(syntaxTree, classifiedSpanBaseline);
            }

            // Verify tag helper spans
            var tagHelperSpanFile = TestFile.Create(baselineTagHelperSpansFileName, GetType().GetTypeInfo().Assembly);
            var tagHelperSpanBaseline = new string[0];
            if (tagHelperSpanFile.Exists())
            {
                tagHelperSpanBaseline = tagHelperSpanFile.ReadAllText().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

                // Temporary
                throw new NotImplementedException("Tag helpers don't use the new syntax tree yet.");
            }
        }
    }
}
