// Copyright (c) .NET Foundation. All rights reserved.
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
        /// <summary>
        /// Initializes a new instance of <see cref="RazorParser"/>.
        /// </summary>
        /// <param name="codeParser">The <see cref="ParserBase"/> used for parsing code content.</param>
        /// <param name="markupParser">The <see cref="ParserBase"/> used for parsing markup content.</param>
        /// <param name="tagHelperDescriptorResolver">The <see cref="ITagHelperDescriptorResolver"/> used to resolve
        /// <see cref="TagHelperDescriptor"/>s.</param>
        public RazorParser(ParserBase codeParser,
                           ParserBase markupParser,
                           ITagHelperDescriptorResolver tagHelperDescriptorResolver)
            : this(codeParser,
                  markupParser,
                  tagHelperDescriptorResolver,
                  GetDefaultRewriters(markupParser))
        {
            if (codeParser == null)
            {
                throw new ArgumentNullException(nameof(codeParser));
            }

            if (markupParser == null)
            {
                throw new ArgumentNullException(nameof(markupParser));
            }
        }

        /// <summary>
        /// Initializes a new instance of <see cref="RazorParser"/> from the specified <paramref name="parser" />.
        /// </summary>
        /// <param name="parser">The <see cref="RazorParser"/> to copy values from.</param>
        public RazorParser(RazorParser parser)
           : this(parser.CodeParser, parser.MarkupParser, parser.TagHelperDescriptorResolver, parser.Optimizers)
        {
            if (parser == null)
            {
                throw new ArgumentNullException(nameof(parser));
            }

            DesignTimeMode = parser.DesignTimeMode;
        }

        private RazorParser(ParserBase codeParser,
                            ParserBase markupParser,
                            ITagHelperDescriptorResolver tagHelperDescriptorResolver,
                            IEnumerable<ISyntaxTreeRewriter> optimizers)
        {
            CodeParser = codeParser;
            MarkupParser = markupParser;
            TagHelperDescriptorResolver = tagHelperDescriptorResolver;
            Optimizers = optimizers.ToList();
        }

        public bool DesignTimeMode { get; set; }

        /// <summary>
        /// Gets the <see cref="ITagHelperDescriptorResolver"/> used to resolve <see cref="TagHelperDescriptor"/>s.
        /// </summary>
        protected ITagHelperDescriptorResolver TagHelperDescriptorResolver { get; private set; }

        // Internal for unit testing
        internal ParserBase CodeParser { get; private set; }

        // Internal for unit testing
        internal ParserBase MarkupParser { get; private set; }

        // Internal for unit testing
        internal List<ISyntaxTreeRewriter> Optimizers { get; private set; }

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
            var results = ParseCore(new SeekableTextReader(input));

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
            var errorSink = new ErrorSink();
            var context = new ParserContext(input, CodeParser, MarkupParser, MarkupParser, errorSink)
            {
                DesignTimeMode = DesignTimeMode
            };

            MarkupParser.Context = context;
            CodeParser.Context = context;

            // Execute the parse
            MarkupParser.ParseDocument();

            // Get the result
            var results = context.CompleteParse();

            // Rewrite whitespace if supported
            var rewritingContext = new RewritingContext(results.Document, errorSink);
            foreach (ISyntaxTreeRewriter rewriter in Optimizers)
            {
                rewriter.Rewrite(rewritingContext);
            }

            var descriptors = Enumerable.Empty<TagHelperDescriptor>();

            if (TagHelperDescriptorResolver != null)
            {
                descriptors = GetTagHelperDescriptors(rewritingContext.SyntaxTree, rewritingContext.ErrorSink);
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

            // Return the new result
            return new ParserResults(syntaxTree, descriptors, errorSink);
        }

        /// <summary>
        /// Returns a sequence of <see cref="TagHelperDescriptor"/>s for tag helpers that are registered in the
        /// specified <paramref name="documentRoot"/>.
        /// </summary>
        /// <param name="documentRoot">The <see cref="Block"/> to scan for tag helper registrations in.</param>
        /// <param name="errorSink">Used to manage <see cref="RazorError"/>s encountered during the Razor parsing
        /// phase.</param>
        /// <returns><see cref="TagHelperDescriptor"/>s that are applicable to the <paramref name="documentRoot"/>
        /// </returns>
        protected virtual IEnumerable<TagHelperDescriptor> GetTagHelperDescriptors(Block documentRoot,
                                                                                   ErrorSink errorSink)
        {
            if (documentRoot == null)
            {
                throw new ArgumentNullException(nameof(documentRoot));
            }

            if (errorSink == null)
            {
                throw new ArgumentNullException(nameof(errorSink));
            }

            var addOrRemoveTagHelperSpanVisitor =
                new TagHelperDirectiveSpanVisitor(TagHelperDescriptorResolver, errorSink);
            return addOrRemoveTagHelperSpanVisitor.GetDescriptors(documentRoot);
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
