// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

//#define PARSER_TRACE

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.AspNet.Razor.Generator;
using Microsoft.AspNet.Razor.Parser;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Text;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Framework
{
    public abstract class ParserTestBase
    {
        protected static Block IgnoreOutput = new IgnoreOutputBlock();

        public SpanFactory Factory { get; private set; }

        protected ParserTestBase()
        {
            Factory = CreateSpanFactory();
        }

        public abstract ParserBase CreateMarkupParser();
        public abstract ParserBase CreateCodeParser();

        protected abstract ParserBase SelectActiveParser(ParserBase codeParser, ParserBase markupParser);

        public virtual ParserContext CreateParserContext(ITextDocument input, ParserBase codeParser, ParserBase markupParser)
        {
            return new ParserContext(input, codeParser, markupParser, SelectActiveParser(codeParser, markupParser));
        }

        protected abstract SpanFactory CreateSpanFactory();

        protected virtual void ParseBlockTest(string document)
        {
            ParseBlockTest(document, null, false, new RazorError[0]);
        }

        protected virtual void ParseBlockTest(string document, bool designTimeParser)
        {
            ParseBlockTest(document, null, designTimeParser, new RazorError[0]);
        }

        protected virtual void ParseBlockTest(string document, params RazorError[] expectedErrors)
        {
            ParseBlockTest(document, false, expectedErrors);
        }

        protected virtual void ParseBlockTest(string document, bool designTimeParser, params RazorError[] expectedErrors)
        {
            ParseBlockTest(document, null, designTimeParser, expectedErrors);
        }

        protected virtual void ParseBlockTest(string document, Block expectedRoot)
        {
            ParseBlockTest(document, expectedRoot, false, null);
        }

        protected virtual void ParseBlockTest(string document, Block expectedRoot, bool designTimeParser)
        {
            ParseBlockTest(document, expectedRoot, designTimeParser, null);
        }

        protected virtual void ParseBlockTest(string document, Block expectedRoot, params RazorError[] expectedErrors)
        {
            ParseBlockTest(document, expectedRoot, false, expectedErrors);
        }

        protected virtual void ParseBlockTest(string document, Block expectedRoot, bool designTimeParser, params RazorError[] expectedErrors)
        {
            RunParseTest(document, parser => parser.ParseBlock, expectedRoot, (expectedErrors ?? new RazorError[0]).ToList(), designTimeParser);
        }

        protected virtual void SingleSpanBlockTest(string document, BlockType blockType, SpanKind spanType, AcceptedCharacters acceptedCharacters = AcceptedCharacters.Any)
        {
            SingleSpanBlockTest(document, blockType, spanType, acceptedCharacters, expectedError: null);
        }

        protected virtual void SingleSpanBlockTest(string document, string spanContent, BlockType blockType, SpanKind spanType, AcceptedCharacters acceptedCharacters = AcceptedCharacters.Any)
        {
            SingleSpanBlockTest(document, spanContent, blockType, spanType, acceptedCharacters, expectedErrors: null);
        }

        protected virtual void SingleSpanBlockTest(string document, BlockType blockType, SpanKind spanType, params RazorError[] expectedError)
        {
            SingleSpanBlockTest(document, document, blockType, spanType, expectedError);
        }

        protected virtual void SingleSpanBlockTest(string document, string spanContent, BlockType blockType, SpanKind spanType, params RazorError[] expectedErrors)
        {
            SingleSpanBlockTest(document, spanContent, blockType, spanType, AcceptedCharacters.Any, expectedErrors ?? new RazorError[0]);
        }

        protected virtual void SingleSpanBlockTest(string document, BlockType blockType, SpanKind spanType, AcceptedCharacters acceptedCharacters, params RazorError[] expectedError)
        {
            SingleSpanBlockTest(document, document, blockType, spanType, acceptedCharacters, expectedError);
        }

        protected virtual void SingleSpanBlockTest(string document, string spanContent, BlockType blockType, SpanKind spanType, AcceptedCharacters acceptedCharacters, params RazorError[] expectedErrors)
        {
            BlockBuilder builder = new BlockBuilder();
            builder.Type = blockType;
            ParseBlockTest(
                document,
                ConfigureAndAddSpanToBlock(
                    builder,
                    Factory.Span(spanType, spanContent, spanType == SpanKind.Markup)
                           .Accepts(acceptedCharacters)),
                expectedErrors ?? new RazorError[0]);
        }

        protected virtual void ParseDocumentTest(string document) {
            ParseDocumentTest(document, null, false);
        }

        protected virtual void ParseDocumentTest(string document, Block expectedRoot) {
            ParseDocumentTest(document, expectedRoot, false, null);
        }

        protected virtual void ParseDocumentTest(string document, Block expectedRoot, params RazorError[] expectedErrors) {
            ParseDocumentTest(document, expectedRoot, false, expectedErrors);
        }

        protected virtual void ParseDocumentTest(string document, bool designTimeParser) {
            ParseDocumentTest(document, null, designTimeParser);
        }

        protected virtual void ParseDocumentTest(string document, Block expectedRoot, bool designTimeParser) {
            ParseDocumentTest(document, expectedRoot, designTimeParser, null);
        }

        protected virtual void ParseDocumentTest(string document, Block expectedRoot, bool designTimeParser, params RazorError[] expectedErrors) {
            RunParseTest(document, parser => parser.ParseDocument, expectedRoot, expectedErrors, designTimeParser, parserSelector: c => c.MarkupParser);
        }

        protected virtual ParserResults ParseDocument(string document) {
            return ParseDocument(document, designTimeParser: false);
        }

        protected virtual ParserResults ParseDocument(string document, bool designTimeParser) {
            return RunParse(document, parser => parser.ParseDocument, designTimeParser, parserSelector: c => c.MarkupParser);
        }

        protected virtual ParserResults ParseBlock(string document) {
            return ParseBlock(document, designTimeParser: false);
        }

        protected virtual ParserResults ParseBlock(string document, bool designTimeParser) {
            return RunParse(document, parser => parser.ParseBlock, designTimeParser);
        }

        protected virtual ParserResults RunParse(string document, Func<ParserBase, Action> parserActionSelector, bool designTimeParser, Func<ParserContext, ParserBase> parserSelector = null)
        {
            parserSelector = parserSelector ?? (c => c.ActiveParser);

            // Create the source
            ParserResults results = null;
            using (SeekableTextReader reader = new SeekableTextReader(document))
            {
                try
                {
                    ParserBase codeParser = CreateCodeParser();
                    ParserBase markupParser = CreateMarkupParser();
                    ParserContext context = CreateParserContext(reader, codeParser, markupParser);
                    context.DesignTimeMode = designTimeParser;

                    codeParser.Context = context;
                    markupParser.Context = context;

                    // Run the parser
                    parserActionSelector(parserSelector(context))();
                    results = context.CompleteParse();
                }
                finally
                {
                    if (results != null && results.Document != null)
                    {
                        WriteTraceLine(String.Empty);
                        WriteTraceLine("Actual Parse Tree:");
                        WriteNode(0, results.Document);
                    }
                }
            }
            return results;
        }

        protected virtual void RunParseTest(string document, Func<ParserBase, Action> parserActionSelector, Block expectedRoot, IList<RazorError> expectedErrors, bool designTimeParser, Func<ParserContext, ParserBase> parserSelector = null)
        {
            // Create the source
            ParserResults results = RunParse(document, parserActionSelector, designTimeParser, parserSelector);

            // Evaluate the results
            if (!ReferenceEquals(expectedRoot, IgnoreOutput))
            {
                EvaluateResults(results, expectedRoot, expectedErrors);
            }
        }

        [Conditional("PARSER_TRACE")]
        private void WriteNode(int indent, SyntaxTreeNode node)
        {
            string content = node.ToString().Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("{", "{{")
                .Replace("}", "}}");
            if (indent > 0)
            {
                content = new String('.', indent * 2) + content;
            }
            WriteTraceLine(content);
            Block block = node as Block;
            if (block != null)
            {
                foreach (SyntaxTreeNode child in block.Children)
                {
                    WriteNode(indent + 1, child);
                }
            }
        }

        public static void EvaluateResults(ParserResults results, Block expectedRoot)
        {
            EvaluateResults(results, expectedRoot, null);
        }

        public static void EvaluateResults(ParserResults results, Block expectedRoot, IList<RazorError> expectedErrors)
        {
            EvaluateParseTree(results.Document, expectedRoot);
            EvaluateRazorErrors(results.ParserErrors, expectedErrors);
        }

        public static void EvaluateParseTree(Block actualRoot, Block expectedRoot)
        {
            // Evaluate the result
            ErrorCollector collector = new ErrorCollector();

            // Link all the nodes
            expectedRoot.LinkNodes();

            if (expectedRoot == null)
            {
                Assert.Null(actualRoot);
            }
            else
            {
                Assert.NotNull(actualRoot);
                EvaluateSyntaxTreeNode(collector, actualRoot, expectedRoot);
                if (collector.Success)
                {
                    WriteTraceLine("Parse Tree Validation Succeeded:\r\n{0}", collector.Message);
                }
                else
                {
                    Assert.True(false, String.Format("\r\n{0}", collector.Message));
                }
            }
        }

        private static void EvaluateSyntaxTreeNode(ErrorCollector collector, SyntaxTreeNode actual, SyntaxTreeNode expected)
        {
            if (actual == null)
            {
                AddNullActualError(collector, actual, expected);
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
            if (actual.Type != expected.Type || !expected.CodeGenerator.Equals(actual.CodeGenerator))
            {
                AddMismatchError(collector, actual, expected);
            }
            else
            {
                AddPassedMessage(collector, expected);
                using (collector.Indent())
                {
                    IEnumerator<SyntaxTreeNode> expectedNodes = expected.Children.GetEnumerator();
                    IEnumerator<SyntaxTreeNode> actualNodes = actual.Children.GetEnumerator();
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

        public static void EvaluateRazorErrors(IList<RazorError> actualErrors, IList<RazorError> expectedErrors)
        {
            // Evaluate the errors
            if (expectedErrors == null || expectedErrors.Count == 0)
            {
                Assert.True(actualErrors.Count == 0,
                            String.Format("Expected that no errors would be raised, but the following errors were:\r\n{0}", FormatErrors(actualErrors)));
            }
            else
            {
                Assert.True(expectedErrors.Count == actualErrors.Count,
                            String.Format("Expected that {0} errors would be raised, but {1} errors were.\r\nExpected Errors: \r\n{2}\r\nActual Errors: \r\n{3}",
                                          expectedErrors.Count,
                                          actualErrors.Count,
                                          FormatErrors(expectedErrors),
                                          FormatErrors(actualErrors)));
                Assert.Equal(expectedErrors.ToArray(), actualErrors.ToArray());
            }
            WriteTraceLine("Expected Errors were raised:\r\n{0}", FormatErrors(expectedErrors));
        }

        public static string FormatErrors(IList<RazorError> errors)
        {
            if (errors == null)
            {
                return "\t<< No Errors >>";
            }

            StringBuilder builder = new StringBuilder();
            foreach (RazorError err in errors)
            {
                builder.AppendFormat("\t{0}", err);
                builder.AppendLine();
            }
            return builder.ToString();
        }

        [Conditional("PARSER_TRACE")]
        private static void WriteTraceLine(string format, params object[] args)
        {
            Trace.WriteLine(String.Format(format, args));
        }

        protected virtual Block CreateSimpleBlockAndSpan(string spanContent, BlockType blockType, SpanKind spanType, AcceptedCharacters acceptedCharacters = AcceptedCharacters.Any)
        {
            SpanConstructor span = Factory.Span(spanType, spanContent, spanType == SpanKind.Markup).Accepts(acceptedCharacters);
            BlockBuilder b = new BlockBuilder()
            {
                Type = blockType
            };
            return ConfigureAndAddSpanToBlock(b, span);
        }

        protected virtual Block ConfigureAndAddSpanToBlock(BlockBuilder block, SpanConstructor span)
        {
            switch (block.Type)
            {
                case BlockType.Markup:
                    span.With(new MarkupCodeGenerator());
                    break;
                case BlockType.Statement:
                    span.With(new StatementCodeGenerator());
                    break;
                case BlockType.Expression:
                    block.CodeGenerator = new ExpressionCodeGenerator();
                    span.With(new ExpressionCodeGenerator());
                    break;
            }
            block.Children.Add(span);
            return block.Build();
        }

        private class IgnoreOutputBlock : Block
        {
            public IgnoreOutputBlock() : base(BlockType.Template, Enumerable.Empty<SyntaxTreeNode>(), null) { }
        }
    }
}
