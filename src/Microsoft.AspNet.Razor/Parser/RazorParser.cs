// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Parser.TagHelpers;
using Microsoft.AspNet.Razor.Parser.TagHelpers.Internal;
using Microsoft.AspNet.Razor.TagHelpers;
using Microsoft.AspNet.Razor.Text;

namespace Microsoft.AspNet.Razor.Parser
{
    public class RazorParser
    {
        public RazorParser([NotNull] ParserBase codeParser,
                           [NotNull] ParserBase markupParser,
                           ITagHelperDescriptorResolver tagHelperDescriptorResolver)
            : this(codeParser,
                  markupParser,
                  tagHelperDescriptorResolver,
                  GetDefaultRewriters(markupParser))
        {
        }

        public RazorParser([NotNull] RazorParser parser)
            : this(parser.CodeParser, parser.MarkupParser, parser.TagHelperDescriptorResolver, parser.Optimizers)
        {
            DesignTimeMode = parser.DesignTimeMode;
        }

        private RazorParser(ParserBase codeParser,
                            ParserBase markupParser,
                            ITagHelperDescriptorResolver tagHelperDescriptorResolver,
                            IEnumerable<ISyntaxTreeRewriter> optimizers)
        {
            TagHelperDescriptorResolver = tagHelperDescriptorResolver;
            MarkupParser = markupParser;
            CodeParser = codeParser;
            Optimizers = new List<ISyntaxTreeRewriter>(optimizers);
        }

        public bool DesignTimeMode { get; set; }

        public ParserBase CodeParser { get; }

        public ParserBase MarkupParser { get; }

        public List<ISyntaxTreeRewriter> Optimizers { get; }

        public ITagHelperDescriptorResolver TagHelperDescriptorResolver { get; }

        public virtual void Parse(TextReader input, ParserVisitor visitor)
        {
            Parse(new SeekableTextReader(input), visitor);
        }

        public virtual ParserResults Parse(TextReader input)
        {
            return ParseCore(new SeekableTextReader(input));
        }

        public virtual ParserResults Parse(ITextDocument input)
        {
            return ParseCore(input);
        }

#pragma warning disable 0618
        [Obsolete("Lookahead-based readers have been deprecated, use overrides which accept a TextReader or ITextDocument instead")]
        public virtual void Parse(LookaheadTextReader input, ParserVisitor visitor)
        {
            ParserResults results = ParseCore(new SeekableTextReader(input));

            // Replay the results on the visitor
            visitor.Visit(results);
        }

        [Obsolete("Lookahead-based readers have been deprecated, use overrides which accept a TextReader or ITextDocument instead")]
        public virtual ParserResults Parse(LookaheadTextReader input)
        {
            return ParseCore(new SeekableTextReader(input));
        }
#pragma warning restore 0618

        public virtual Task CreateParseTask(TextReader input, Action<Span> spanCallback, Action<RazorError> errorCallback)
        {
            return CreateParseTask(input, new CallbackVisitor(spanCallback, errorCallback));
        }

        public virtual Task CreateParseTask(TextReader input, Action<Span> spanCallback, Action<RazorError> errorCallback, SynchronizationContext context)
        {
            return CreateParseTask(input, new CallbackVisitor(spanCallback, errorCallback) { SynchronizationContext = context });
        }

        public virtual Task CreateParseTask(TextReader input, Action<Span> spanCallback, Action<RazorError> errorCallback, CancellationToken cancelToken)
        {
            return CreateParseTask(input, new CallbackVisitor(spanCallback, errorCallback) { CancelToken = cancelToken });
        }

        public virtual Task CreateParseTask(TextReader input, Action<Span> spanCallback, Action<RazorError> errorCallback, SynchronizationContext context, CancellationToken cancelToken)
        {
            return CreateParseTask(input, new CallbackVisitor(spanCallback, errorCallback)
            {
                SynchronizationContext = context,
                CancelToken = cancelToken
            });
        }

        [SuppressMessage("Microsoft.Web.FxCop", "MW1200:DoNotConstructTaskInstances", Justification = "This rule is not applicable to this assembly.")]
        public virtual Task CreateParseTask(TextReader input,
                                            ParserVisitor consumer)
        {
            return new Task(() =>
            {
                try
                {
                    Parse(input, consumer);
                }
                catch (OperationCanceledException)
                {
                    return; // Just return if we're cancelled.
                }
            });
        }

        private ParserResults ParseCore(ITextDocument input)
        {
            // Setup the parser context
            ParserContext context = new ParserContext(input, CodeParser, MarkupParser, MarkupParser)
            {
                DesignTimeMode = DesignTimeMode
            };

            MarkupParser.Context = context;
            CodeParser.Context = context;

            // Execute the parse
            MarkupParser.ParseDocument();

            // Get the result
            ParserResults results = context.CompleteParse();

            // Rewrite whitespace if supported
            var rewritingContext = new RewritingContext(results.Document);
            foreach (ISyntaxTreeRewriter rewriter in Optimizers)
            {
                rewriter.Rewrite(rewritingContext);
            }

            if (TagHelperDescriptorResolver != null)
            {
                var descriptors = GetTagHelperDescriptors(rewritingContext);
                var tagHelperProvider = new TagHelperDescriptorProvider(descriptors);

                var tagHelperParseTreeRewriter = new TagHelperParseTreeRewriter(tagHelperProvider);
                // Rewrite the document to utilize tag helpers
                tagHelperParseTreeRewriter.Rewrite(rewritingContext);
            }

            var syntaxTree = rewritingContext.SyntaxTree;

            // Link the leaf nodes into a chain
            Span prev = null;
            foreach (Span node in syntaxTree.Flatten())
            {
                node.Previous = prev;
                if (prev != null)
                {
                    prev.Next = node;
                }
                prev = node;
            }

            // We want to surface both the parsing and rewriting errors as one unified list of errors because
            // both parsing and rewriting errors affect the end users Razor page.
            var errors = results.ParserErrors.Concat(rewritingContext.Errors).ToList();

            // Return the new result
            return new ParserResults(syntaxTree, errors);
        }

        protected virtual IEnumerable<TagHelperDescriptor> GetTagHelperDescriptors([NotNull] RewritingContext rewritingContext)
        {
            var tagHelperRegistrationVisitor = new TagHelperRegistrationVisitor(TagHelperDescriptorResolver);
            return tagHelperRegistrationVisitor.GetDescriptors(rewritingContext.SyntaxTree);
        }

        private static IEnumerable<ISyntaxTreeRewriter> GetDefaultRewriters(ParserBase markupParser)
        {
            return new ISyntaxTreeRewriter[]
            {
                // TODO: Modify the below WhiteSpaceRewriter & ConditionalAttributeCollapser to handle 
                // TagHelperBlock's: https://github.com/aspnet/Razor/issues/117

                // Move whitespace from start of expression block to markup
                new WhiteSpaceRewriter(markupParser.BuildSpan),
                // Collapse conditional attributes where the entire value is literal
                new ConditionalAttributeCollapser(markupParser.BuildSpan),
            };
        }
    }
}
