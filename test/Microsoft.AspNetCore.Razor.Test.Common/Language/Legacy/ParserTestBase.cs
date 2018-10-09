
// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
#if NET46
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
#else
using System.Threading;
#endif
using System.Text;
using Xunit;
using Xunit.Sdk;
using System.Text.RegularExpressions;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    [IntializeTestFile]
    public abstract class ParserTestBase
    {
#if !NET46
        private static readonly AsyncLocal<string> _fileName = new AsyncLocal<string>();
        private static readonly AsyncLocal<bool> _isTheory = new AsyncLocal<bool>();
#endif

        internal ParserTestBase()
        {
            Factory = CreateSpanFactory();
            BlockFactory = CreateBlockFactory();
            TestProjectRoot = TestProject.GetProjectDirectory(GetType());
        }

        /// <summary>
        /// Set to true to autocorrect the locations of spans to appear in document order with no gaps.
        /// Use this when spans were not created in document order.
        /// </summary>
        protected bool FixupSpans { get; set; }

        internal SpanFactory Factory { get; private set; }

        internal BlockFactory BlockFactory { get; private set; }

#if GENERATE_BASELINES
        protected bool GenerateBaselines { get; set; } = true;
#else
        protected bool GenerateBaselines { get; set; } = false;
#endif

        protected string TestProjectRoot { get; }

        // Used by the test framework to set the 'base' name for test files.
        public static string FileName
        {
#if NET46
            get
            {
                var handle = (ObjectHandle)CallContext.LogicalGetData("ParserTestBase_FileName");
                return (string)handle.Unwrap();
            }
            set
            {
                CallContext.LogicalSetData("ParserTestBase_FileName", new ObjectHandle(value));
            }
#elif NETCOREAPP2_2
            get { return _fileName.Value; }
            set { _fileName.Value = value; }
#endif
        }

        public static bool IsTheory
        {
#if NET46
            get
            {
                var handle = (ObjectHandle)CallContext.LogicalGetData("ParserTestBase_IsTheory");
                return (bool)handle.Unwrap();
            }
            set
            {
                CallContext.LogicalSetData("ParserTestBase_IsTheory", new ObjectHandle(value));
            }
#elif NETCOREAPP2_2
            get { return _isTheory.Value; }
            set { _isTheory.Value = value; }
#endif
        }

        internal virtual void AssertSyntaxTreeNodeMatchesBaseline(RazorSyntaxTree syntaxTree)
        {
            AssertSyntaxTreeNodeMatchesBaseline(syntaxTree.LegacyRoot, syntaxTree.Source.FilePath, syntaxTree.Diagnostics.ToArray());
        }

        internal void AssertSyntaxTreeNodeMatchesBaseline(Block root, string filePath, params RazorDiagnostic[] diagnostics)
        {
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
                File.WriteAllText(baselineFullPath, SyntaxTreeNodeSerializer.Serialize(root));

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
                File.WriteAllText(classifiedSpansBaselineFullPath, ClassifiedSpanSerializer.Serialize(root, filePath));

                // Write tag helper spans baseline
                var tagHelperSpansBaselineFullPath = Path.Combine(TestProjectRoot, baselineTagHelperSpansFileName);
                var serializedTagHelperSpans = TagHelperSpanSerializer.Serialize(root, filePath);
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
            SyntaxTreeNodeVerifier.Verify(root, baseline);

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
                var classifiedSpanBaseline = classifiedSpanFile.ReadAllText().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                ClassifiedSpanVerifier.Verify(root, filePath, classifiedSpanBaseline);
            }

            // Verify tag helper spans
            var tagHelperSpanFile = TestFile.Create(baselineTagHelperSpansFileName, GetType().GetTypeInfo().Assembly);
            var tagHelperSpanBaseline = new string[0];
            if (tagHelperSpanFile.Exists())
            {
                tagHelperSpanBaseline = tagHelperSpanFile.ReadAllText().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }

            TagHelperSpanVerifier.Verify(root, filePath, tagHelperSpanBaseline);
        }

        protected static string SerializeDiagnostic(RazorDiagnostic diagnostic)
        {
            var content = RazorDiagnosticSerializer.Serialize(diagnostic);
            var normalized = NormalizeNewLines(content);

            return normalized;
        }

        private static string NormalizeNewLines(string content)
        {
            return Regex.Replace(content, "(?<!\r)\n", "\r\n", RegexOptions.None, TimeSpan.FromSeconds(10));
        }

        internal virtual void BaselineTest(RazorSyntaxTree syntaxTree, bool verifySyntaxTree = true)
        {
            if (verifySyntaxTree)
            {
                SyntaxTreeVerifier.Verify(syntaxTree);
            }

            AssertSyntaxTreeNodeMatchesBaseline(syntaxTree);
        }

        internal virtual void BaselineTest(Block root, string filePath = null, bool verifySyntaxTree = true, params RazorDiagnostic[] diagnostics)
        {
            if (verifySyntaxTree)
            {
                SyntaxTreeVerifier.Verify(root);
            }

            AssertSyntaxTreeNodeMatchesBaseline(root, filePath, diagnostics);
        }

        internal RazorSyntaxTree ParseBlock(string document, bool designTime)
        {
            return ParseBlock(RazorLanguageVersion.Latest, document, designTime);
        }

        internal RazorSyntaxTree ParseBlock(RazorLanguageVersion version, string document, bool designTime)
        {
            return ParseBlock(version, document, null, designTime);
        }

        internal RazorSyntaxTree ParseBlock(string document, IEnumerable<DirectiveDescriptor> directives, bool designTime)
        {
            return ParseBlock(RazorLanguageVersion.Latest, document, directives, designTime);
        }

        internal abstract RazorSyntaxTree ParseBlock(RazorLanguageVersion version, string document, IEnumerable<DirectiveDescriptor> directives, bool designTime);

        internal RazorSyntaxTree ParseDocument(string document, bool designTime = false, RazorParserFeatureFlags featureFlags = null)
        {
            return ParseDocument(RazorLanguageVersion.Latest, document, designTime, featureFlags);
        }

        internal RazorSyntaxTree ParseDocument(RazorLanguageVersion version, string document, bool designTime = false, RazorParserFeatureFlags featureFlags = null)
        {
            return ParseDocument(version, document, null, designTime, featureFlags);
        }

        internal RazorSyntaxTree ParseDocument(string document, IEnumerable<DirectiveDescriptor> directives, bool designTime = false, RazorParserFeatureFlags featureFlags = null)
        {
            return ParseDocument(RazorLanguageVersion.Latest, document, directives, designTime, featureFlags);
        }

        internal virtual RazorSyntaxTree ParseDocument(RazorLanguageVersion version, string document, IEnumerable<DirectiveDescriptor> directives, bool designTime = false, RazorParserFeatureFlags featureFlags = null)
        {
            directives = directives ?? Array.Empty<DirectiveDescriptor>();

            var source = TestRazorSourceDocument.Create(document, filePath: null, relativePath: null, normalizeNewLines: true);

            var options = CreateParserOptions(version, directives, designTime, featureFlags);
            var context = new ParserContext(source, options);

            var codeParser = new CSharpCodeParser(directives, context);
            var markupParser = new HtmlMarkupParser(context);

            codeParser.HtmlParser = markupParser;
            markupParser.CodeParser = codeParser;

            markupParser.ParseDocument1();

            var root = context.Builder.Build();
            var diagnostics = context.ErrorSink.Errors;

            var codeDocument = RazorCodeDocument.Create(source);

            var syntaxTree = RazorSyntaxTree.Create(root, source, diagnostics, options);
            codeDocument.SetSyntaxTree(syntaxTree);

            var defaultDirectivePass = new DefaultDirectiveSyntaxTreePass();
            syntaxTree = defaultDirectivePass.Execute(codeDocument, syntaxTree);

            return syntaxTree;
        }

        internal virtual RazorSyntaxTree ParseHtmlBlock(RazorLanguageVersion version, string document, IEnumerable<DirectiveDescriptor> directives, bool designTime = false)
        {
            directives = directives ?? Array.Empty<DirectiveDescriptor>();

            var source = TestRazorSourceDocument.Create(document, filePath: null, relativePath: null, normalizeNewLines: true);
            var options = CreateParserOptions(version, directives, designTime);
            var context = new ParserContext(source, options);

            var parser = new HtmlMarkupParser(context);
            parser.CodeParser = new CSharpCodeParser(directives, context)
            {
                HtmlParser = parser,
            };

            parser.ParseBlock1();

            var root = context.Builder.Build();
            var diagnostics = context.ErrorSink.Errors;

            return RazorSyntaxTree.Create(root, source, diagnostics, options);
        }

        internal RazorSyntaxTree ParseCodeBlock(string document, bool designTime = false)
        {
            return ParseCodeBlock(RazorLanguageVersion.Latest, document, Enumerable.Empty<DirectiveDescriptor>(), designTime);
        }

        internal virtual RazorSyntaxTree ParseCodeBlock(
            RazorLanguageVersion version,
            string document,
            IEnumerable<DirectiveDescriptor> directives,
            bool designTime)
        {
            directives = directives ?? Array.Empty<DirectiveDescriptor>();

            var source = TestRazorSourceDocument.Create(document, filePath: null, relativePath: null, normalizeNewLines: true);
            var options = CreateParserOptions(version, directives, designTime);
            var context = new ParserContext(source, options);

            var parser = new CSharpCodeParser(directives, context);
            parser.HtmlParser = new HtmlMarkupParser(context)
            {
                CodeParser = parser,
            };

            parser.ParseBlock1();

            var root = context.Builder.Build();
            var diagnostics = context.ErrorSink.Errors;

            return RazorSyntaxTree.Create(root, source, diagnostics, options);
        }

        internal SpanFactory CreateSpanFactory()
        {
            return new SpanFactory();
        }

        internal abstract BlockFactory CreateBlockFactory();

        internal virtual void ParseBlockTest(string document)
        {
            ParseBlockTest(document, null, false, new RazorDiagnostic[0]);
        }

        internal virtual void ParseBlockTest(string document, bool designTime)
        {
            ParseBlockTest(document, null, designTime, new RazorDiagnostic[0]);
        }

        internal virtual void ParseBlockTest(string document, IEnumerable<DirectiveDescriptor> directives)
        {
            ParseBlockTest(document, directives, null);
        }

        internal virtual void ParseBlockTest(string document, params RazorDiagnostic[] expectedErrors)
        {
            ParseBlockTest(document, false, expectedErrors);
        }

        internal virtual void ParseBlockTest(string document, bool designTime, params RazorDiagnostic[] expectedErrors)
        {
            ParseBlockTest(document, null, designTime, expectedErrors);
        }

        internal virtual void ParseBlockTest(RazorLanguageVersion version, string document)
        {
            ParseBlockTest(version, document, expectedRoot: null);
        }

        internal virtual void ParseBlockTest(RazorLanguageVersion version, string document, Block expectedRoot)
        {
            ParseBlockTest(version, document, expectedRoot, false, null);
        }

        internal virtual void ParseBlockTest(string document, Block expectedRoot)
        {
            ParseBlockTest(document, expectedRoot, false, null);
        }

        internal virtual void ParseBlockTest(string document, IEnumerable<DirectiveDescriptor> directives, Block expectedRoot)
        {
            ParseBlockTest(document, directives, expectedRoot, false, null);
        }

        internal virtual void ParseBlockTest(string document, Block expectedRoot, bool designTime)
        {
            ParseBlockTest(document, expectedRoot, designTime, null);
        }

        internal virtual void ParseBlockTest(string document, Block expectedRoot, params RazorDiagnostic[] expectedErrors)
        {
            ParseBlockTest(document, expectedRoot, false, expectedErrors);
        }

        internal virtual void ParseBlockTest(string document, IEnumerable<DirectiveDescriptor> directives, Block expectedRoot, params RazorDiagnostic[] expectedErrors)
        {
            ParseBlockTest(document, directives, expectedRoot, false, expectedErrors);
        }

        internal virtual void ParseBlockTest(string document, Block expected, bool designTime, params RazorDiagnostic[] expectedErrors)
        {
            ParseBlockTest(document, null, expected, designTime, expectedErrors);
        }

        internal virtual void ParseBlockTest(RazorLanguageVersion version, string document, Block expected, bool designTime, params RazorDiagnostic[] expectedErrors)
        {
            ParseBlockTest(version, document, null, expected, designTime, expectedErrors);
        }

        internal virtual void ParseBlockTest(string document, IEnumerable<DirectiveDescriptor> directives, Block expected, bool designTime, params RazorDiagnostic[] expectedErrors)
        {
            ParseBlockTest(RazorLanguageVersion.Latest, document, directives, expected, designTime, expectedErrors);
        }

        internal virtual void ParseBlockTest(RazorLanguageVersion version, string document, IEnumerable<DirectiveDescriptor> directives, Block expected, bool designTime, params RazorDiagnostic[] expectedErrors)
        {
            var result = ParseBlock(version, document, directives, designTime);

            BaselineTest(result);
        }

        internal virtual void SingleSpanBlockTest(string document)
        {
            SingleSpanBlockTest(document, default, default);
        }

        internal virtual void SingleSpanBlockTest(string document, BlockKindInternal blockKind, SpanKindInternal spanType, AcceptedCharactersInternal acceptedCharacters = AcceptedCharactersInternal.Any)
        {
            SingleSpanBlockTest(document, blockKind, spanType, acceptedCharacters, expectedError: null);
        }

        internal virtual void SingleSpanBlockTest(string document, string spanContent, BlockKindInternal blockKind, SpanKindInternal spanType, AcceptedCharactersInternal acceptedCharacters = AcceptedCharactersInternal.Any)
        {
            SingleSpanBlockTest(document, spanContent, blockKind, spanType, acceptedCharacters, expectedErrors: null);
        }

        internal virtual void SingleSpanBlockTest(string document, BlockKindInternal blockKind, SpanKindInternal spanType, params RazorDiagnostic[] expectedError)
        {
            SingleSpanBlockTest(document, document, blockKind, spanType, expectedError);
        }

        internal virtual void SingleSpanBlockTest(string document, string spanContent, BlockKindInternal blockKind, SpanKindInternal spanType, params RazorDiagnostic[] expectedErrors)
        {
            SingleSpanBlockTest(document, spanContent, blockKind, spanType, AcceptedCharactersInternal.Any, expectedErrors ?? new RazorDiagnostic[0]);
        }

        internal virtual void SingleSpanBlockTest(string document, BlockKindInternal blockKind, SpanKindInternal spanType, AcceptedCharactersInternal acceptedCharacters, params RazorDiagnostic[] expectedError)
        {
            SingleSpanBlockTest(document, document, blockKind, spanType, acceptedCharacters, expectedError);
        }

        internal virtual void SingleSpanBlockTest(string document, string spanContent, BlockKindInternal blockKind, SpanKindInternal spanType, AcceptedCharactersInternal acceptedCharacters, params RazorDiagnostic[] expectedErrors)
        {
            var result = ParseBlock(document, designTime: false);

            BaselineTest(result);
        }

        internal virtual void ParseDocumentTest(string document)
        {
            ParseDocumentTest(document, null, false);
        }

        internal virtual void ParseDocumentTest(string document, IEnumerable<DirectiveDescriptor> directives)
        {
            ParseDocumentTest(document, directives, expected: null);
        }

        internal virtual void ParseDocumentTest(string document, Block expectedRoot)
        {
            ParseDocumentTest(document, expectedRoot, false, null);
        }

        internal virtual void ParseDocumentTest(string document, Block expectedRoot, params RazorDiagnostic[] expectedErrors)
        {
            ParseDocumentTest(document, expectedRoot, false, expectedErrors);
        }

        internal virtual void ParseDocumentTest(string document, IEnumerable<DirectiveDescriptor> directives, Block expected, params RazorDiagnostic[] expectedErrors)
        {
            ParseDocumentTest(document, directives, expected, false, expectedErrors);
        }

        internal virtual void ParseDocumentTest(string document, bool designTime)
        {
            ParseDocumentTest(document, null, designTime);
        }

        internal virtual void ParseDocumentTest(string document, Block expectedRoot, bool designTime)
        {
            ParseDocumentTest(document, expectedRoot, designTime, null);
        }

        internal virtual void ParseDocumentTest(string document, Block expected, bool designTime, params RazorDiagnostic[] expectedErrors)
        {
            ParseDocumentTest(document, null, expected, designTime, expectedErrors);
        }

        internal virtual void ParseDocumentTest(string document, IEnumerable<DirectiveDescriptor> directives, Block expected, bool designTime, params RazorDiagnostic[] expectedErrors)
        {
            var result = ParseDocument(document, directives, designTime);

            BaselineTest(result);
        }

        internal static void EvaluateResults(RazorSyntaxTree result, Block expectedRoot)
        {
            EvaluateResults(result, expectedRoot, null);
        }

        internal static void EvaluateResults(RazorSyntaxTree result, Block expectedRoot, IList<RazorDiagnostic> expectedErrors)
        {
            EvaluateParseTree(result.LegacyRoot, expectedRoot);
            EvaluateRazorErrors(result.Diagnostics, expectedErrors);
        }

        internal static void EvaluateParseTree(Block actualRoot, Block expectedRoot)
        {
            // Evaluate the result
            var collector = new ErrorCollector();

            if (expectedRoot == null)
            {
                Assert.Null(actualRoot);
            }
            else
            {
                // Link all the nodes
                expectedRoot.LinkNodes();
                Assert.NotNull(actualRoot);
                EvaluateSyntaxTreeNode(collector, actualRoot, expectedRoot);
                if (collector.Success)
                {
                    WriteTraceLine("Parse Tree Validation Succeeded:" + Environment.NewLine + collector.Message);
                }
                else
                {
                    Assert.True(false, Environment.NewLine + collector.Message);
                }
            }
        }

        private static void EvaluateSyntaxTreeNode(ErrorCollector collector, SyntaxTreeNode actual, SyntaxTreeNode expected)
        {
            if (actual == null)
            {
                AddNullActualError(collector, actual, expected);
                return;
            }

            if (actual.IsBlock != expected.IsBlock)
            {
                AddMismatchError(collector, actual, expected);
            }
            else
            {
                if (expected.IsBlock)
                {
                    EvaluateBlock(collector, (Block)actual, (Block)expected);
                }
                else
                {
                    EvaluateSpan(collector, (Span)actual, (Span)expected);
                }
            }
        }

        private static void EvaluateSpan(ErrorCollector collector, Span actual, Span expected)
        {
            if (!Equals(expected, actual))
            {
                AddMismatchError(collector, actual, expected);
            }
            else
            {
                AddPassedMessage(collector, expected);
            }
        }

        private static void EvaluateBlock(ErrorCollector collector, Block actual, Block expected)
        {
            if (actual.Type != expected.Type || !expected.ChunkGenerator.Equals(actual.ChunkGenerator))
            {
                AddMismatchError(collector, actual, expected);
            }
            else
            {
                if (actual is TagHelperBlock)
                {
                    EvaluateTagHelperBlock(collector, actual as TagHelperBlock, expected as TagHelperBlock);
                }

                AddPassedMessage(collector, expected);
                using (collector.Indent())
                {
                    var expectedNodes = expected.Children.GetEnumerator();
                    var actualNodes = actual.Children.GetEnumerator();
                    while (expectedNodes.MoveNext())
                    {
                        if (!actualNodes.MoveNext())
                        {
                            collector.AddError("{0} - FAILED :: No more elements at this node", expectedNodes.Current);
                        }
                        else
                        {
                            EvaluateSyntaxTreeNode(collector, actualNodes.Current, expectedNodes.Current);
                        }
                    }
                    while (actualNodes.MoveNext())
                    {
                        collector.AddError("End of Node - FAILED :: Found Node: {0}", actualNodes.Current);
                    }
                }
            }
        }

        private static void EvaluateTagHelperBlock(ErrorCollector collector, TagHelperBlock actual, TagHelperBlock expected)
        {
            if (expected == null)
            {
                AddMismatchError(collector, actual, expected);
            }
            else
            {
                if (!string.Equals(expected.TagName, actual.TagName, StringComparison.Ordinal))
                {
                    collector.AddError(
                        "{0} - FAILED :: TagName mismatch for TagHelperBlock :: ACTUAL: {1}",
                        expected.TagName,
                        actual.TagName);
                }

                if (expected.TagMode != actual.TagMode)
                {
                    collector.AddError(
                        $"{expected.TagMode} - FAILED :: {nameof(TagMode)} for {nameof(TagHelperBlock)} " +
                        $"{actual.TagName} :: ACTUAL: {actual.TagMode}");
                }

                var expectedAttributes = expected.Attributes.GetEnumerator();
                var actualAttributes = actual.Attributes.GetEnumerator();

                while (expectedAttributes.MoveNext())
                {
                    if (!actualAttributes.MoveNext())
                    {
                        collector.AddError("{0} - FAILED :: No more attributes on this node", expectedAttributes.Current);
                    }
                    else
                    {
                        EvaluateTagHelperAttribute(collector, actualAttributes.Current, expectedAttributes.Current);
                    }
                }
                while (actualAttributes.MoveNext())
                {
                    collector.AddError("End of Attributes - FAILED :: Found Attribute: {0}", actualAttributes.Current.Name);
                }
            }
        }

        private static void EvaluateTagHelperAttribute(
            ErrorCollector collector,
            LegacyTagHelperAttributeNode actual,
            LegacyTagHelperAttributeNode expected)
        {
            if (actual.Name != expected.Name)
            {
                collector.AddError("{0} - FAILED :: Attribute names do not match", expected.Name);
            }
            else
            {
                collector.AddMessage("{0} - PASSED :: Attribute names match", expected.Name);
            }

            if (actual.AttributeStructure != expected.AttributeStructure)
            {
                collector.AddError("{0} - FAILED :: Attribute value styles do not match", expected.AttributeStructure.ToString());
            }
            else
            {
                collector.AddMessage("{0} - PASSED :: Attribute value style match", expected.AttributeStructure);
            }

            if (actual.AttributeStructure != AttributeStructure.Minimized)
            {
                EvaluateSyntaxTreeNode(collector, actual.Value, expected.Value);
            }
        }

        private static void AddPassedMessage(ErrorCollector collector, SyntaxTreeNode expected)
        {
            collector.AddMessage("{0} - PASSED", expected);
        }

        private static void AddMismatchError(ErrorCollector collector, SyntaxTreeNode actual, SyntaxTreeNode expected)
        {
            collector.AddError("{0} - FAILED :: Actual: {1}", expected, actual);
        }

        private static void AddNullActualError(ErrorCollector collector, SyntaxTreeNode actual, SyntaxTreeNode expected)
        {
            collector.AddError("{0} - FAILED :: Actual: << Null >>", expected);
        }

        internal static void EvaluateRazorErrors(IEnumerable<RazorDiagnostic> actualErrors, IList<RazorDiagnostic> expectedErrors)
        {
            var realCount = actualErrors.Count();

            // Evaluate the errors
            if (expectedErrors == null || expectedErrors.Count == 0)
            {
                Assert.True(
                    realCount == 0,
                    "Expected that no errors would be raised, but the following errors were:" + Environment.NewLine + FormatErrors(actualErrors));
            }
            else
            {
                Assert.True(
                    expectedErrors.Count == realCount,
                    $"Expected that {expectedErrors.Count} errors would be raised, but {realCount} errors were." +
                    $"{Environment.NewLine}Expected Errors: {Environment.NewLine}{FormatErrors(expectedErrors)}" +
                    $"{Environment.NewLine}Actual Errors: {Environment.NewLine}{FormatErrors(actualErrors)}");
                Assert.Equal(expectedErrors, actualErrors);
            }
            WriteTraceLine("Expected Errors were raised:" + Environment.NewLine + FormatErrors(expectedErrors));
        }

        internal static string FormatErrors(IEnumerable<RazorDiagnostic> errors)
        {
            if (errors == null)
            {
                return "\t<< No Errors >>";
            }

            var builder = new StringBuilder();
            foreach (var error in errors)
            {
                builder.AppendFormat("\t{0}", error);
                builder.AppendLine();
            }
            return builder.ToString();
        }

        [Conditional("PARSER_TRACE")]
        private static void WriteTraceLine(string format, params object[] args)
        {
            Trace.WriteLine(string.Format(format, args));
        }

        internal static RazorParserOptions CreateParserOptions(
            RazorLanguageVersion version, 
            IEnumerable<DirectiveDescriptor> directives, 
            bool designTime,
            RazorParserFeatureFlags featureFlags = null)
        {
            return new TestRazorParserOptions(
                directives.ToArray(),
                designTime,
                parseLeadingDirectives: false,
                version: version,
                featureFlags: featureFlags);
        }

        private class TestRazorParserOptions : RazorParserOptions
        {
            public TestRazorParserOptions(DirectiveDescriptor[] directives, bool designTime, bool parseLeadingDirectives, RazorLanguageVersion version, RazorParserFeatureFlags featureFlags = null)
            {
                if (directives == null)
                {
                    throw new ArgumentNullException(nameof(directives));
                }

                Directives = directives;
                DesignTime = designTime;
                ParseLeadingDirectives = parseLeadingDirectives;
                Version = version;
                FeatureFlags = featureFlags ?? RazorParserFeatureFlags.Create(Version);
            }

            public override bool DesignTime { get; }

            public override IReadOnlyCollection<DirectiveDescriptor> Directives { get; }

            public override bool ParseLeadingDirectives { get; }

            public override RazorLanguageVersion Version { get; }

            internal override RazorParserFeatureFlags FeatureFlags { get; }
        }
    }
}