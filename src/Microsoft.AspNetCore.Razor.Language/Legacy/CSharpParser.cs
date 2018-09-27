// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNetCore.Razor.Language.Syntax.InternalSyntax;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal partial class CSharpCodeParser
    {
        private Dictionary<CSharpKeyword, Action<SyntaxListBuilder<RazorSyntaxNode>, CSharpTransitionSyntax>> _keywordParserMap = new Dictionary<CSharpKeyword, Action<SyntaxListBuilder<RazorSyntaxNode>, CSharpTransitionSyntax>>();
        private Dictionary<string, Action<SyntaxListBuilder<RazorSyntaxNode>, CSharpTransitionSyntax>> _directiveParserMap = new Dictionary<string, Action<SyntaxListBuilder<RazorSyntaxNode>, CSharpTransitionSyntax>>(StringComparer.Ordinal);

        public CSharpCodeBlockSyntax ParseBlock()
        {
            if (Context == null)
            {
                throw new InvalidOperationException(Resources.Parser_Context_Not_Set);
            }

            if (EndOfFile)
            {
                // Nothing to parse.
                return null;
            }

            using (var pooledResult = Pool.Allocate<RazorSyntaxNode>())
            using (PushSpanContextConfig(DefaultSpanContextConfig))
            {
                var builder = pooledResult.Builder;
                try
                {
                    NextToken();

                    // Unless changed, the block is a statement block
                    AcceptTokenWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
                    builder.Add(OutputTokensAsStatementLiteral());

                    // We are usually called when the other parser sees a transition '@'. Look for it.
                    SyntaxToken transitionToken = null;
                    if (At(SyntaxKind.StringLiteral) &&
                        CurrentToken.Content.Length > 0 &&
                        CurrentToken.Content[0] == SyntaxConstants.TransitionCharacter)
                    {
                        var split = Language.SplitToken(CurrentToken, 1, SyntaxKind.Transition);
                        transitionToken = split.Item1;

                        // Back up to the end of the transition
                        Context.Source.Position -= split.Item2.Content.Length;
                        NextToken();
                    }
                    else if (At(SyntaxKind.Transition))
                    {
                        transitionToken = EatCurrentToken();
                    }

                    if (transitionToken == null)
                    {
                        transitionToken = SyntaxFactory.MissingToken(SyntaxKind.Transition);
                    }

                    SpanContext.ChunkGenerator = SpanChunkGenerator.Null;
                    SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
                    var transition = GetNodeWithSpanContext(SyntaxFactory.CSharpTransition(transitionToken));

                    if (At(SyntaxKind.LeftBrace))
                    {
                        var statementBody = ParseStatementBody();
                        var statement = SyntaxFactory.CSharpStatement(transition, statementBody);
                        builder.Add(statement);
                    }
                    else if (At(SyntaxKind.LeftParenthesis))
                    {
                        var expressionBody = ParseExplicitExpressionBody();
                        var expression = SyntaxFactory.CSharpExplicitExpression(transition, expressionBody);
                        builder.Add(expression);
                    }
                    else if (At(SyntaxKind.Identifier))
                    {
                        if (!TryParseDirective(builder, transition, CurrentToken.Content))
                        {
                            if (string.Equals(
                                CurrentToken.Content,
                                SyntaxConstants.CSharp.HelperKeyword,
                                StringComparison.Ordinal))
                            {
                                var diagnostic = RazorDiagnosticFactory.CreateParsing_HelperDirectiveNotAvailable(
                                    new SourceSpan(CurrentStart, CurrentToken.Content.Length));
                                CurrentToken.SetDiagnostics(new[] { diagnostic });
                                Context.ErrorSink.OnError(diagnostic);
                            }

                            var implicitExpressionBody = ParseImplicitExpressionBody();
                            var implicitExpression = SyntaxFactory.CSharpImplicitExpression(transition, implicitExpressionBody);
                            builder.Add(implicitExpression);
                        }
                    }
                    else if (At(SyntaxKind.Keyword))
                    {
                        if (!TryParseDirective(builder, transition, CurrentToken.Content) &&
                            !TryParseKeyword(builder, transition))
                        {
                            // Not a directive or a special keyword. Just parse as an implicit expression.
                            var implicitExpressionBody = ParseImplicitExpressionBody();
                            var implicitExpression = SyntaxFactory.CSharpImplicitExpression(transition, implicitExpressionBody);
                            builder.Add(implicitExpression);
                        }

                        builder.Add(OutputTokensAsStatementLiteral());
                    }
                    else
                    {
                        // Invalid character
                        SpanContext.ChunkGenerator = new ExpressionChunkGenerator();
                        SpanContext.EditHandler = new ImplicitExpressionEditHandler(
                            Language.TokenizeString,
                            CurrentKeywords,
                            acceptTrailingDot: IsNested)
                        {
                            AcceptedCharacters = AcceptedCharactersInternal.NonWhitespace
                        };

                        AcceptMarkerTokenIfNecessary();
                        var expressionLiteral = SyntaxFactory.CSharpCodeBlock(OutputTokensAsExpressionLiteral());
                        var expressionBody = SyntaxFactory.CSharpImplicitExpressionBody(expressionLiteral);
                        var expressionBlock = SyntaxFactory.CSharpImplicitExpression(transition, expressionBody);
                        builder.Add(expressionBlock);

                        if (At(SyntaxKind.Whitespace) || At(SyntaxKind.NewLine))
                        {
                            Context.ErrorSink.OnError(
                                RazorDiagnosticFactory.CreateParsing_UnexpectedWhiteSpaceAtStartOfCodeBlock(
                                    new SourceSpan(CurrentStart, CurrentToken.Content.Length)));
                        }
                        else if (EndOfFile)
                        {
                            Context.ErrorSink.OnError(
                                RazorDiagnosticFactory.CreateParsing_UnexpectedEndOfFileAtStartOfCodeBlock(
                                    new SourceSpan(CurrentStart, contentLength: 1 /* end of file */)));
                        }
                        else
                        {
                            Context.ErrorSink.OnError(
                                RazorDiagnosticFactory.CreateParsing_UnexpectedCharacterAtStartOfCodeBlock(
                                    new SourceSpan(CurrentStart, CurrentToken.Content.Length),
                                    CurrentToken.Content));
                        }
                    }

                    Debug.Assert(TokenBuilder.Count == 0, "We should not have any tokens left.");

                    var codeBlock = SyntaxFactory.CSharpCodeBlock(builder.ToList());
                    return codeBlock;
                }
                finally
                {
                    // Always put current character back in the buffer for the next parser.
                    PutCurrentBack();
                }
            }
        }

        private CSharpExplicitExpressionBodySyntax ParseExplicitExpressionBody()
        {
            var block = new Block(Resources.BlockName_ExplicitExpression, CurrentStart);
            Assert(SyntaxKind.LeftParenthesis);
            var leftParenToken = EatCurrentToken();
            var leftParen = OutputAsMetaCode(leftParenToken);

            using (var pooledResult = Pool.Allocate<RazorSyntaxNode>())
            {
                var expressionBuilder = pooledResult.Builder;
                using (PushSpanContextConfig(ExplicitExpressionSpanContextConfig))
                {
                    var success = BalanceToken(
                        expressionBuilder,
                        BalancingModes.BacktrackOnFailure |
                            BalancingModes.NoErrorOnFailure |
                            BalancingModes.AllowCommentsAndTemplates,
                        SyntaxKind.LeftParenthesis,
                        SyntaxKind.RightParenthesis,
                        block.Start);

                    if (!success)
                    {
                        AcceptTokenUntil(SyntaxKind.LessThan);
                        Context.ErrorSink.OnError(
                            RazorDiagnosticFactory.CreateParsing_ExpectedEndOfBlockBeforeEOF(
                                new SourceSpan(block.Start, contentLength: 1 /* ( */), block.Name, ")", "("));
                    }

                    // If necessary, put an empty-content marker token here
                    AcceptMarkerTokenIfNecessary();
                    expressionBuilder.Add(OutputTokensAsExpressionLiteral());
                }

                var expressionBlock = SyntaxFactory.CSharpCodeBlock(expressionBuilder.ToList());

                RazorMetaCodeSyntax rightParen = null;
                if (At(SyntaxKind.RightParenthesis))
                {
                    rightParen = OutputAsMetaCode(EatCurrentToken());
                }
                else
                {
                    var missingToken = SyntaxFactory.MissingToken(SyntaxKind.RightParenthesis);
                    rightParen = OutputAsMetaCode(missingToken, SpanContext.EditHandler.AcceptedCharacters);
                }
                if (!EndOfFile)
                {
                    PutCurrentBack();
                }

                return SyntaxFactory.CSharpExplicitExpressionBody(leftParen, expressionBlock, rightParen);
            }
        }

        private CSharpImplicitExpressionBodySyntax ParseImplicitExpressionBody(bool async = false)
        {
            var accepted = AcceptedCharactersInternal.NonWhitespace;
            if (async)
            {
                // Async implicit expressions include the "await" keyword and therefore need to allow spaces to
                // separate the "await" and the following code.
                accepted = AcceptedCharactersInternal.AnyExceptNewline;
            }

            using (var pooledResult = Pool.Allocate<RazorSyntaxNode>())
            {
                var expressionBuilder = pooledResult.Builder;
                ParseImplicitExpression(expressionBuilder, accepted);
                var codeBlock = SyntaxFactory.CSharpCodeBlock(expressionBuilder.ToList());
                return SyntaxFactory.CSharpImplicitExpressionBody(codeBlock);
            }
        }

        private void ParseImplicitExpression(in SyntaxListBuilder<RazorSyntaxNode> builder, AcceptedCharactersInternal acceptedCharacters)
        {
            using (PushSpanContextConfig(spanContext =>
            {
                spanContext.EditHandler = new ImplicitExpressionEditHandler(
                    Language.TokenizeString,
                    Keywords,
                    acceptTrailingDot: IsNested);
                spanContext.EditHandler.AcceptedCharacters = acceptedCharacters;
                spanContext.ChunkGenerator = new ExpressionChunkGenerator();
            }))
            {
                do
                {
                    if (AtIdentifier(allowKeywords: true))
                    {
                        AcceptTokenAndMoveNext();
                    }
                }
                while (ParseMethodCallOrArrayIndex(builder, acceptedCharacters));

                PutCurrentBack();
                builder.Add(OutputTokensAsExpressionLiteral());
            }
        }

        private bool ParseMethodCallOrArrayIndex(in SyntaxListBuilder<RazorSyntaxNode> builder, AcceptedCharactersInternal acceptedCharacters)
        {
            if (!EndOfFile)
            {
                if (CurrentToken.Kind == SyntaxKind.LeftParenthesis ||
                    CurrentToken.Kind == SyntaxKind.LeftBracket)
                {
                    // If we end within "(", whitespace is fine
                    SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Any;

                    SyntaxKind right;
                    bool success;

                    using (PushSpanContextConfig((spanContext, prev) =>
                    {
                        prev(spanContext);
                        spanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Any;
                    }))
                    {
                        right = Language.FlipBracket(CurrentToken.Kind);
                        success = BalanceToken(builder, BalancingModes.BacktrackOnFailure | BalancingModes.AllowCommentsAndTemplates);
                    }

                    if (!success)
                    {
                        AcceptTokenUntil(SyntaxKind.LessThan);
                    }
                    if (At(right))
                    {
                        AcceptTokenAndMoveNext();

                        // At the ending brace, restore the initial accepted characters.
                        SpanContext.EditHandler.AcceptedCharacters = acceptedCharacters;
                    }
                    return ParseMethodCallOrArrayIndex(builder, acceptedCharacters);
                }
                if (At(SyntaxKind.QuestionMark))
                {
                    var next = Lookahead(count: 1);

                    if (next != null)
                    {
                        if (next.Kind == SyntaxKind.Dot)
                        {
                            // Accept null conditional dot operator (?.).
                            AcceptTokenAndMoveNext();
                            AcceptTokenAndMoveNext();

                            // If the next piece after the ?. is a keyword or identifier then we want to continue.
                            return At(SyntaxKind.Identifier) || At(SyntaxKind.Keyword);
                        }
                        else if (next.Kind == SyntaxKind.LeftBracket)
                        {
                            // We're at the ? for a null conditional bracket operator (?[).
                            AcceptTokenAndMoveNext();

                            // Accept the [ and any content inside (it will attempt to balance).
                            return ParseMethodCallOrArrayIndex(builder, acceptedCharacters);
                        }
                    }
                }
                else if (At(SyntaxKind.Dot))
                {
                    var dot = CurrentToken;
                    if (NextToken())
                    {
                        if (At(SyntaxKind.Identifier) || At(SyntaxKind.Keyword))
                        {
                            // Accept the dot and return to the start
                            AcceptToken(dot);
                            return true; // continue
                        }
                        else
                        {
                            // Put the token back
                            PutCurrentBack();
                        }
                    }
                    if (!IsNested)
                    {
                        // Put the "." back
                        PutBack(dot);
                    }
                    else
                    {
                        AcceptToken(dot);
                    }
                }
                else if (!At(SyntaxKind.Whitespace) && !At(SyntaxKind.NewLine))
                {
                    PutCurrentBack();
                }
            }

            // Implicit Expression is complete
            return false;
        }

        private CSharpStatementBodySyntax ParseStatementBody(Block block = null)
        {
            Assert(SyntaxKind.LeftBrace);
            block = block ?? new Block(Resources.BlockName_Code, CurrentStart);
            var leftBrace = OutputAsMetaCode(GetExpectedToken(SyntaxKind.LeftBrace));
            CSharpCodeBlockSyntax codeBlock = null;
            using (var pooledResult = Pool.Allocate<RazorSyntaxNode>())
            {
                var builder = pooledResult.Builder;
                // Set up auto-complete and parse the code block
                var editHandler = new AutoCompleteEditHandler(Language.TokenizeString);
                SpanContext.EditHandler = editHandler;
                ParseCodeBlock(builder, block, acceptTerminatingBrace: false);

                EnsureCurrent();
                SpanContext.ChunkGenerator = new StatementChunkGenerator();
                AcceptMarkerTokenIfNecessary();
                if (!At(SyntaxKind.RightBrace))
                {
                    editHandler.AutoCompleteString = "}";
                }
                builder.Add(OutputTokensAsStatementLiteral());

                codeBlock = SyntaxFactory.CSharpCodeBlock(builder.ToList());
            }

            RazorMetaCodeSyntax rightBrace = null;
            if (At(SyntaxKind.RightBrace))
            {
                rightBrace = OutputAsMetaCode(EatCurrentToken());
            }
            else
            {
                rightBrace = OutputAsMetaCode(
                    SyntaxFactory.MissingToken(SyntaxKind.RightBrace),
                    SpanContext.EditHandler.AcceptedCharacters);
            }

            if (!IsNested)
            {
                EnsureCurrent();
                if (At(SyntaxKind.NewLine) ||
                    (At(SyntaxKind.Whitespace) && NextIs(SyntaxKind.NewLine)))
                {
                    Context.NullGenerateWhitespaceAndNewLine = true;
                }
            }

            return SyntaxFactory.CSharpStatementBody(leftBrace, codeBlock, rightBrace);
        }

        private void ParseCodeBlock(in SyntaxListBuilder<RazorSyntaxNode> builder, Block block, bool acceptTerminatingBrace = true)
        {
            EnsureCurrent();
            while (!EndOfFile && !At(SyntaxKind.RightBrace))
            {
                // Parse a statement, then return here
                ParseStatement(builder, block: block);
                EnsureCurrent();
            }

            if (EndOfFile)
            {
                Context.ErrorSink.OnError(
                    RazorDiagnosticFactory.CreateParsing_ExpectedEndOfBlockBeforeEOF(
                        new SourceSpan(block.Start, contentLength: 1 /* { OR } */), block.Name, "}", "{"));
            }
            else if (acceptTerminatingBrace)
            {
                Assert(SyntaxKind.RightBrace);
                SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
                AcceptTokenAndMoveNext();
            }
        }

        private void ParseStatement(in SyntaxListBuilder<RazorSyntaxNode> builder, Block block)
        {
            SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Any;
            // Accept whitespace but always keep the last whitespace node so we can put it back if necessary
            var lastWhitespace = AcceptWhitespaceTokensInLines();
            if (EndOfFile)
            {
                if (lastWhitespace != null)
                {
                    AcceptToken(lastWhitespace);
                }

                builder.Add(OutputTokensAsStatementLiteral());
                return;
            }

            var kind = CurrentToken.Kind;
            var location = CurrentStart;

            // Both cases @: and @:: are triggered as markup, second colon in second case will be triggered as a plain text
            var isSingleLineMarkup = kind == SyntaxKind.Transition &&
                (NextIs(SyntaxKind.Colon, SyntaxKind.DoubleColon));

            var isMarkup = isSingleLineMarkup ||
                kind == SyntaxKind.LessThan ||
                (kind == SyntaxKind.Transition && NextIs(SyntaxKind.LessThan));

            if (Context.DesignTimeMode || !isMarkup)
            {
                // CODE owns whitespace, MARKUP owns it ONLY in DesignTimeMode.
                if (lastWhitespace != null)
                {
                    AcceptToken(lastWhitespace);
                }
            }
            else
            {
                var nextToken = Lookahead(1);

                // MARKUP owns whitespace EXCEPT in DesignTimeMode.
                PutCurrentBack();

                // Put back the whitespace unless it precedes a '<text>' tag.
                if (nextToken != null &&
                    !string.Equals(nextToken.Content, SyntaxConstants.TextTagName, StringComparison.Ordinal))
                {
                    PutBack(lastWhitespace);
                }
                else
                {
                    // If it precedes a '<text>' tag, it should be accepted as code.
                    AcceptToken(lastWhitespace);
                }
            }

            if (isMarkup)
            {
                if (kind == SyntaxKind.Transition && !isSingleLineMarkup)
                {
                    Context.ErrorSink.OnError(
                        RazorDiagnosticFactory.CreateParsing_AtInCodeMustBeFollowedByColonParenOrIdentifierStart(
                            new SourceSpan(location, contentLength: 1 /* @ */)));
                }

                // Markup block
                builder.Add(OutputTokensAsStatementLiteral());
                if (Context.DesignTimeMode && CurrentToken != null &&
                    (CurrentToken.Kind == SyntaxKind.LessThan || CurrentToken.Kind == SyntaxKind.Transition))
                {
                    PutCurrentBack();
                }
                OtherParserBlock(builder);
            }
            else
            {
                // What kind of statement is this?
                switch (kind)
                {
                    case SyntaxKind.RazorCommentTransition:
                        AcceptMarkerTokenIfNecessary();
                        builder.Add(OutputTokensAsStatementLiteral());
                        var comment = ParseRazorComment();
                        builder.Add(comment);
                        ParseStatement(builder, block);
                        break;
                    case SyntaxKind.LeftBrace:
                        // Verbatim Block
                        AcceptTokenAndMoveNext();
                        ParseCodeBlock(builder, block);
                        break;
                    case SyntaxKind.Keyword:
                        if (!TryParseKeyword(builder, transition: null))
                        {
                            ParseStandardStatement(builder);
                        }
                        break;
                    case SyntaxKind.Transition:
                        // Embedded Expression block
                        ParseEmbeddedExpression(builder);
                        break;
                    case SyntaxKind.RightBrace:
                        // Possible end of Code Block, just run the continuation
                        break;
                    case SyntaxKind.CSharpComment:
                        AcceptToken(CurrentToken);
                        NextToken();
                        break;
                    default:
                        // Other statement
                        ParseStandardStatement(builder);
                        break;
                }
            }
        }

        private void ParseEmbeddedExpression(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            // First, verify the type of the block
            Assert(SyntaxKind.Transition);
            var transition = CurrentToken;
            NextToken();

            if (At(SyntaxKind.Transition))
            {
                // Escaped "@"
                builder.Add(OutputTokensAsStatementLiteral());

                // Output "@" as hidden span
                AcceptToken(transition);
                SpanContext.ChunkGenerator = SpanChunkGenerator.Null;
                builder.Add(OutputTokensAsEphemeralLiteral());

                Assert(SyntaxKind.Transition);
                AcceptTokenAndMoveNext();
                ParseStandardStatement(builder);
            }
            else
            {
                // Throw errors as necessary, but continue parsing
                if (At(SyntaxKind.LeftBrace))
                {
                    Context.ErrorSink.OnError(
                        RazorDiagnosticFactory.CreateParsing_UnexpectedNestedCodeBlock(
                            new SourceSpan(CurrentStart, contentLength: 1 /* { */)));
                }

                // @( or @foo - Nested expression, parse a child block
                PutCurrentBack();
                PutBack(transition);

                // Before exiting, add a marker span if necessary
                AcceptMarkerTokenIfNecessary();
                builder.Add(OutputTokensAsStatementLiteral());

                var nestedBlock = ParseNestedBlock();
                builder.Add(nestedBlock);
            }
        }

        private RazorSyntaxNode ParseNestedBlock()
        {
            var wasNested = IsNested;
            IsNested = true;

            RazorSyntaxNode nestedBlock;
            using (PushSpanContextConfig())
            {
                nestedBlock = ParseBlock();
            }

            InitializeContext(SpanContext);
            IsNested = wasNested;
            NextToken();

            return nestedBlock;
        }

        private void ParseStandardStatement(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            while (!EndOfFile)
            {
                var bookmark = CurrentStart.AbsoluteIndex;
                var read = ReadWhile(token =>
                    token.Kind != SyntaxKind.Semicolon &&
                    token.Kind != SyntaxKind.RazorCommentTransition &&
                    token.Kind != SyntaxKind.Transition &&
                    token.Kind != SyntaxKind.LeftBrace &&
                    token.Kind != SyntaxKind.LeftParenthesis &&
                    token.Kind != SyntaxKind.LeftBracket &&
                    token.Kind != SyntaxKind.RightBrace);

                if (At(SyntaxKind.LeftBrace) ||
                    At(SyntaxKind.LeftParenthesis) ||
                    At(SyntaxKind.LeftBracket))
                {
                    AcceptToken(read);
                    if (BalanceToken(builder, BalancingModes.AllowCommentsAndTemplates | BalancingModes.BacktrackOnFailure))
                    {
                        OptionalToken(SyntaxKind.RightBrace);
                    }
                    else
                    {
                        // Recovery
                        AcceptTokenUntil(SyntaxKind.LessThan, SyntaxKind.RightBrace);
                        return;
                    }
                }
                else if (At(SyntaxKind.Transition) && (NextIs(SyntaxKind.LessThan, SyntaxKind.Colon)))
                {
                    AcceptToken(read);
                    builder.Add(OutputTokensAsStatementLiteral());
                    ParseTemplate(builder);
                }
                else if (At(SyntaxKind.RazorCommentTransition))
                {
                    AcceptToken(read);
                    AcceptMarkerTokenIfNecessary();
                    builder.Add(OutputTokensAsStatementLiteral());
                    builder.Add(ParseRazorComment());
                }
                else if (At(SyntaxKind.Semicolon))
                {
                    AcceptToken(read);
                    AcceptTokenAndMoveNext();
                    return;
                }
                else if (At(SyntaxKind.RightBrace))
                {
                    AcceptToken(read);
                    return;
                }
                else
                {
                    Context.Source.Position = bookmark;
                    NextToken();
                    AcceptTokenUntil(SyntaxKind.LessThan, SyntaxKind.LeftBrace, SyntaxKind.RightBrace);
                    return;
                }
            }
        }

        private void ParseTemplate(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            if (Context.InTemplateContext)
            {
                Context.ErrorSink.OnError(
                    RazorDiagnosticFactory.CreateParsing_InlineMarkupBlocksCannotBeNested(
                        new SourceSpan(CurrentStart, contentLength: 1 /* @ */)));
            }
            builder.Add(OutputTokensAsStatementLiteral());

            using (var pooledResult = Pool.Allocate<RazorSyntaxNode>())
            {
                var templateBuilder = pooledResult.Builder;
                Context.InTemplateContext = true;
                PutCurrentBack();
                OtherParserBlock(templateBuilder);

                var template = SyntaxFactory.CSharpTemplateBlock(templateBuilder.ToList());
                builder.Add(template);

                Context.InTemplateContext = false;
            }
        }

        protected bool TryParseDirective(in SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition, string directive)
        {
            if (_directiveParserMap.TryGetValue(directive, out var handler))
            {
                SpanContext.ChunkGenerator = SpanChunkGenerator.Null;
                handler(builder, transition);
                return true;
            }

            return false;
        }

        private void SetupDirectiveParsers(IEnumerable<DirectiveDescriptor> directiveDescriptors)
        {
            var allDirectives = directiveDescriptors.Concat(DefaultDirectiveDescriptors).ToList();

            for (var i = 0; i < allDirectives.Count; i++)
            {
                var directiveDescriptor = allDirectives[i];
                CurrentKeywords.Add(directiveDescriptor.Directive);
                MapDirectives((builder, transition) => ParseExtensibleDirective(builder, transition, directiveDescriptor), directiveDescriptor.Directive);
            }

            MapDirectives(ParseTagHelperPrefixDirective, SyntaxConstants.CSharp.TagHelperPrefixKeyword);
            MapDirectives(ParseAddTagHelperDirective, SyntaxConstants.CSharp.AddTagHelperKeyword);
            MapDirectives(ParseRemoveTagHelperDirective, SyntaxConstants.CSharp.RemoveTagHelperKeyword);
        }

        protected void MapDirectives(Action<SyntaxListBuilder<RazorSyntaxNode>, CSharpTransitionSyntax> handler, params string[] directives)
        {
            foreach (var directive in directives)
            {
                _directiveParserMap.Add(directive, (builder, transition) =>
                {
                    handler(builder, transition);
                    Context.SeenDirectives.Add(directive);
                });

                Keywords.Add(directive);

                // These C# keywords are reserved for use in directives. It's an error to use them outside of
                // a directive. This code removes the error generation if the directive *is* registered.
                if (string.Equals(directive, "class", StringComparison.OrdinalIgnoreCase))
                {
                    _keywordParserMap.Remove(CSharpKeyword.Class);
                }
                else if (string.Equals(directive, "namespace", StringComparison.OrdinalIgnoreCase))
                {
                    _keywordParserMap.Remove(CSharpKeyword.Namespace);
                }
            }
        }

        private void ParseTagHelperPrefixDirective(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            RazorDiagnostic duplicateDiagnostic = null;
            if (Context.SeenDirectives.Contains(SyntaxConstants.CSharp.TagHelperPrefixKeyword))
            {
                var directiveStart = CurrentStart;
                if (transition != null)
                {
                    // Start the error from the Transition '@'.
                    directiveStart = new SourceLocation(
                        directiveStart.FilePath,
                        directiveStart.AbsoluteIndex - 1,
                        directiveStart.LineIndex,
                        directiveStart.CharacterIndex - 1);
                }
                var errorLength = /* @ */ 1 + SyntaxConstants.CSharp.TagHelperPrefixKeyword.Length;
                duplicateDiagnostic = RazorDiagnosticFactory.CreateParsing_DuplicateDirective(
                    new SourceSpan(directiveStart, errorLength),
                    SyntaxConstants.CSharp.TagHelperPrefixKeyword);
            }

            var directiveBody = ParseTagHelperDirective(
                SyntaxConstants.CSharp.TagHelperPrefixKeyword,
                (prefix, errors) =>
                {
                    if (duplicateDiagnostic != null)
                    {
                        errors.Add(duplicateDiagnostic);
                    }

                    var parsedDirective = ParseDirective(prefix, CurrentStart, TagHelperDirectiveType.TagHelperPrefix, errors);

                    return new TagHelperPrefixDirectiveChunkGenerator(
                        prefix,
                        parsedDirective.DirectiveText,
                        errors);
                });

            var directive = SyntaxFactory.RazorDirective(transition, directiveBody);
            builder.Add(directive);
        }

        private void ParseAddTagHelperDirective(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            var directiveBody = ParseTagHelperDirective(
                SyntaxConstants.CSharp.AddTagHelperKeyword,
                (lookupText, errors) =>
                {
                    var parsedDirective = ParseDirective(lookupText, CurrentStart, TagHelperDirectiveType.AddTagHelper, errors);

                    return new AddTagHelperChunkGenerator(
                        lookupText,
                        parsedDirective.DirectiveText,
                        parsedDirective.TypePattern,
                        parsedDirective.AssemblyName,
                        errors);
                });

            var directive = SyntaxFactory.RazorDirective(transition, directiveBody);
            builder.Add(directive);
        }

        private void ParseRemoveTagHelperDirective(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            var directiveBody = ParseTagHelperDirective(
                SyntaxConstants.CSharp.RemoveTagHelperKeyword,
                (lookupText, errors) =>
                {
                    var parsedDirective = ParseDirective(lookupText, CurrentStart, TagHelperDirectiveType.RemoveTagHelper, errors);

                    return new RemoveTagHelperChunkGenerator(
                        lookupText,
                        parsedDirective.DirectiveText,
                        parsedDirective.TypePattern,
                        parsedDirective.AssemblyName,
                        errors);
                });

            var directive = SyntaxFactory.RazorDirective(transition, directiveBody);
            builder.Add(directive);
        }

        private RazorDirectiveBodySyntax ParseTagHelperDirective(
            string keyword,
            Func<string, List<RazorDiagnostic>, ISpanChunkGenerator> chunkGeneratorFactory)
        {
            AssertDirective(keyword);

            var savedErrorSink = Context.ErrorSink;
            var directiveErrorSink = new ErrorSink();
            RazorMetaCodeSyntax keywordBlock = null;
            using (var pooledResult = Pool.Allocate<RazorSyntaxNode>())
            {
                var directiveBuilder = pooledResult.Builder;
                Context.ErrorSink = directiveErrorSink;

                string directiveValue = null;
                try
                {
                    EnsureDirectiveIsAtStartOfLine();

                    var keywordStartLocation = CurrentStart;

                    // Accept the directive name
                    var keywordToken = EatCurrentToken();
                    var keywordLength = keywordToken.FullWidth + 1 /* @ */;

                    var foundWhitespace = At(SyntaxKind.Whitespace);

                    // If we found whitespace then any content placed within the whitespace MAY cause a destructive change
                    // to the document.  We can't accept it.
                    var acceptedCharacters = foundWhitespace ? AcceptedCharactersInternal.None : AcceptedCharactersInternal.AnyExceptNewline;
                    AcceptToken(keywordToken);
                    keywordBlock = OutputAsMetaCode(OutputTokens(), acceptedCharacters);

                    AcceptTokenWhile(SyntaxKind.Whitespace);
                    SpanContext.ChunkGenerator = SpanChunkGenerator.Null;
                    SpanContext.EditHandler.AcceptedCharacters = acceptedCharacters;
                    directiveBuilder.Add(OutputTokensAsMarkupLiteral());

                    if (EndOfFile || At(SyntaxKind.NewLine))
                    {
                        Context.ErrorSink.OnError(
                            RazorDiagnosticFactory.CreateParsing_DirectiveMustHaveValue(
                                new SourceSpan(keywordStartLocation, keywordLength), keyword));

                        directiveValue = string.Empty;
                    }
                    else
                    {
                        // Need to grab the current location before we accept until the end of the line.
                        var startLocation = CurrentStart;

                        // Parse to the end of the line. Essentially accepts anything until end of line, comments, invalid code
                        // etc.
                        AcceptTokenUntil(SyntaxKind.NewLine);

                        // Pull out the value and remove whitespaces and optional quotes
                        var rawValue = string.Concat(TokenBuilder.ToList().Nodes.Select(s => s.Content)).Trim();

                        var startsWithQuote = rawValue.StartsWith("\"", StringComparison.Ordinal);
                        var endsWithQuote = rawValue.EndsWith("\"", StringComparison.Ordinal);
                        if (startsWithQuote != endsWithQuote)
                        {
                            Context.ErrorSink.OnError(
                                RazorDiagnosticFactory.CreateParsing_IncompleteQuotesAroundDirective(
                                    new SourceSpan(startLocation, rawValue.Length), keyword));
                        }

                        directiveValue = rawValue;
                    }
                }
                finally
                {
                    SpanContext.ChunkGenerator = chunkGeneratorFactory(directiveValue, directiveErrorSink.Errors.ToList());
                    Context.ErrorSink = savedErrorSink;
                }

                // Finish the block and output the tokens
                CompleteBlock();
                SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.AnyExceptNewline;

                directiveBuilder.Add(OutputTokensAsStatementLiteral());
                var directiveCodeBlock = SyntaxFactory.CSharpCodeBlock(directiveBuilder.ToList());

                return SyntaxFactory.RazorDirectiveBody(keywordBlock, directiveCodeBlock);
            }
        }

        private ParsedDirective ParseDirective(
            string directiveText,
            SourceLocation directiveLocation,
            TagHelperDirectiveType directiveType,
            List<RazorDiagnostic> errors)
        {
            var offset = 0;
            directiveText = directiveText.Trim();
            if (directiveText.Length >= 2 &&
                directiveText.StartsWith("\"", StringComparison.Ordinal) &&
                directiveText.EndsWith("\"", StringComparison.Ordinal))
            {
                directiveText = directiveText.Substring(1, directiveText.Length - 2);
                if (string.IsNullOrEmpty(directiveText))
                {
                    offset = 1;
                }
            }

            // If this is the "string literal" form of a directive, we'll need to postprocess the location
            // and content.
            //
            // Ex: @addTagHelper "*, Microsoft.AspNetCore.CoolLibrary"
            //                    ^                                 ^
            //                  Start                              End
            if (TokenBuilder.Count == 1 &&
                TokenBuilder[0] is SyntaxToken token &&
                token.Kind == SyntaxKind.StringLiteral)
            {
                offset += token.Content.IndexOf(directiveText, StringComparison.Ordinal);

                // This is safe because inside one of these directives all of the text needs to be on the
                // same line.
                var original = directiveLocation;
                directiveLocation = new SourceLocation(
                    original.FilePath,
                    original.AbsoluteIndex + offset,
                    original.LineIndex,
                    original.CharacterIndex + offset);
            }

            var parsedDirective = new ParsedDirective()
            {
                DirectiveText = directiveText
            };

            if (directiveType == TagHelperDirectiveType.TagHelperPrefix)
            {
                ValidateTagHelperPrefix(parsedDirective.DirectiveText, directiveLocation, errors);

                return parsedDirective;
            }

            return ParseAddOrRemoveDirective(parsedDirective, directiveLocation, errors);
        }

        private void ParseExtensibleDirective(in SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition, DirectiveDescriptor descriptor)
        {
            AssertDirective(descriptor.Directive);

            var directiveErrorSink = new ErrorSink();
            var savedErrorSink = Context.ErrorSink;
            Context.ErrorSink = directiveErrorSink;

            using (var pooledResult = Pool.Allocate<RazorSyntaxNode>())
            {
                var directiveBuilder = pooledResult.Builder;
                var directiveChunkGenerator = new DirectiveChunkGenerator(descriptor);
                RazorMetaCodeSyntax keywordBlock = null;

                try
                {
                    EnsureDirectiveIsAtStartOfLine();
                    var directiveStart = CurrentStart;
                    if (transition != null)
                    {
                        // Start the error from the Transition '@'.
                        directiveStart = new SourceLocation(
                            directiveStart.FilePath,
                            directiveStart.AbsoluteIndex - 1,
                            directiveStart.LineIndex,
                            directiveStart.CharacterIndex - 1);
                    }

                    AcceptTokenAndMoveNext();
                    keywordBlock = OutputAsMetaCode(OutputTokens());

                    // Even if an error was logged do not bail out early. If a directive was used incorrectly it doesn't mean it can't be parsed.
                    ValidateDirectiveUsage(descriptor, directiveStart);

                    for (var i = 0; i < descriptor.Tokens.Count; i++)
                    {
                        if (!At(SyntaxKind.Whitespace) &&
                            !At(SyntaxKind.NewLine) &&
                            !EndOfFile)
                        {
                            // This case should never happen in a real scenario. We're just being defensive.
                            Context.ErrorSink.OnError(
                                RazorDiagnosticFactory.CreateParsing_DirectiveTokensMustBeSeparatedByWhitespace(
                                    new SourceSpan(CurrentStart, CurrentToken.Content.Length), descriptor.Directive));

                            builder.Add(BuildDirective());
                            return;
                        }

                        var tokenDescriptor = descriptor.Tokens[i];
                        AcceptTokenWhile(IsSpacingToken(includeNewLines: false, includeComments: true));

                        if (tokenDescriptor.Kind == DirectiveTokenKind.Member ||
                            tokenDescriptor.Kind == DirectiveTokenKind.Namespace ||
                            tokenDescriptor.Kind == DirectiveTokenKind.Type)
                        {
                            SpanContext.ChunkGenerator = SpanChunkGenerator.Null;
                            SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Whitespace;
                            directiveBuilder.Add(OutputTokensAsStatementLiteral());

                            if (EndOfFile || At(SyntaxKind.NewLine))
                            {
                                // Add a marker token to provide CSharp intellisense when we start typing the directive token.
                                AcceptMarkerTokenIfNecessary();
                                SpanContext.ChunkGenerator = new DirectiveTokenChunkGenerator(tokenDescriptor);
                                SpanContext.EditHandler = new DirectiveTokenEditHandler(Language.TokenizeString);
                                SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.NonWhitespace;
                                directiveBuilder.Add(OutputTokensAsStatementLiteral());
                            }
                        }
                        else
                        {
                            SpanContext.ChunkGenerator = SpanChunkGenerator.Null;
                            SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Whitespace;
                            directiveBuilder.Add(OutputTokensAsMarkupEphemeralLiteral());
                        }

                        if (tokenDescriptor.Optional && (EndOfFile || At(SyntaxKind.NewLine)))
                        {
                            break;
                        }
                        else if (EndOfFile)
                        {
                            Context.ErrorSink.OnError(
                                RazorDiagnosticFactory.CreateParsing_UnexpectedEOFAfterDirective(
                                    new SourceSpan(CurrentStart, contentLength: 1),
                                    descriptor.Directive,
                                    tokenDescriptor.Kind.ToString().ToLowerInvariant()));
                            builder.Add(BuildDirective());
                            return;
                        }

                        switch (tokenDescriptor.Kind)
                        {
                            case DirectiveTokenKind.Type:
                                if (!TryParseNamespaceOrTypeName(directiveBuilder))
                                {
                                    Context.ErrorSink.OnError(
                                        RazorDiagnosticFactory.CreateParsing_DirectiveExpectsTypeName(
                                            new SourceSpan(CurrentStart, CurrentToken.Content.Length), descriptor.Directive));

                                    builder.Add(BuildDirective());
                                    return;
                                }
                                break;

                            case DirectiveTokenKind.Namespace:
                                if (!TryParseQualifiedIdentifier(out var identifierLength))
                                {
                                    Context.ErrorSink.OnError(
                                        RazorDiagnosticFactory.CreateParsing_DirectiveExpectsNamespace(
                                            new SourceSpan(CurrentStart, identifierLength), descriptor.Directive));

                                    builder.Add(BuildDirective());
                                    return;
                                }
                                break;

                            case DirectiveTokenKind.Member:
                                if (At(SyntaxKind.Identifier))
                                {
                                    AcceptTokenAndMoveNext();
                                }
                                else
                                {
                                    Context.ErrorSink.OnError(
                                        RazorDiagnosticFactory.CreateParsing_DirectiveExpectsIdentifier(
                                            new SourceSpan(CurrentStart, CurrentToken.Content.Length), descriptor.Directive));
                                    builder.Add(BuildDirective());
                                    return;
                                }
                                break;

                            case DirectiveTokenKind.String:
                                if (At(SyntaxKind.StringLiteral) && !CurrentToken.ContainsDiagnostics)
                                {
                                    AcceptTokenAndMoveNext();
                                }
                                else
                                {
                                    Context.ErrorSink.OnError(
                                        RazorDiagnosticFactory.CreateParsing_DirectiveExpectsQuotedStringLiteral(
                                            new SourceSpan(CurrentStart, CurrentToken.Content.Length), descriptor.Directive));
                                    builder.Add(BuildDirective());
                                    return;
                                }
                                break;
                        }

                        SpanContext.ChunkGenerator = new DirectiveTokenChunkGenerator(tokenDescriptor);
                        SpanContext.EditHandler = new DirectiveTokenEditHandler(Language.TokenizeString);
                        SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.NonWhitespace;
                        directiveBuilder.Add(OutputTokensAsStatementLiteral());
                    }

                    AcceptTokenWhile(IsSpacingToken(includeNewLines: false, includeComments: true));
                    SpanContext.ChunkGenerator = SpanChunkGenerator.Null;

                    switch (descriptor.Kind)
                    {
                        case DirectiveKind.SingleLine:
                            SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Whitespace;
                            directiveBuilder.Add(OutputTokensAsUnclassifiedLiteral());

                            OptionalToken(SyntaxKind.Semicolon);
                            directiveBuilder.Add(OutputAsMetaCode(OutputTokens(), AcceptedCharactersInternal.Whitespace));

                            AcceptTokenWhile(IsSpacingToken(includeNewLines: false, includeComments: true));

                            if (At(SyntaxKind.NewLine))
                            {
                                AcceptTokenAndMoveNext();
                            }
                            else if (!EndOfFile)
                            {
                                Context.ErrorSink.OnError(
                                    RazorDiagnosticFactory.CreateParsing_UnexpectedDirectiveLiteral(
                                        new SourceSpan(CurrentStart, CurrentToken.Content.Length),
                                        descriptor.Directive,
                                        Resources.ErrorComponent_Newline));
                            }


                            // This should contain the optional whitespace after the optional semicolon and the new line.
                            // Output as Markup as we want intellisense here.
                            SpanContext.ChunkGenerator = SpanChunkGenerator.Null;
                            SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Whitespace;
                            directiveBuilder.Add(OutputTokensAsMarkupEphemeralLiteral());
                            break;
                        case DirectiveKind.RazorBlock:
                            AcceptTokenWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
                            SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.AllWhitespace;
                            directiveBuilder.Add(OutputTokensAsMarkupLiteral());

                            ParseDirectiveBlock(directiveBuilder, descriptor, parseChildren: (childBuilder, startingBraceLocation) =>
                            {
                                // When transitioning to the HTML parser we no longer want to act as if we're in a nested C# state.
                                // For instance, if <div>@hello.</div> is in a nested C# block we don't want the trailing '.' to be handled
                                // as C#; it should be handled as a period because it's wrapped in markup.
                                var wasNested = IsNested;
                                IsNested = false;

                                using (PushSpanContextConfig())
                                {
                                    var razorBlock = HtmlParser.ParseRazorBlock(Tuple.Create("{", "}"), caseSensitive: true);
                                    directiveBuilder.Add(razorBlock);
                                }

                                InitializeContext(SpanContext);
                                IsNested = wasNested;
                                NextToken();
                            });
                            break;
                        case DirectiveKind.CodeBlock:
                            AcceptTokenWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
                            SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.AllWhitespace;
                            directiveBuilder.Add(OutputTokensAsMarkupLiteral());

                            ParseDirectiveBlock(directiveBuilder, descriptor, parseChildren: (childBuilder, startingBraceLocation) =>
                            {
                                NextToken();
                                BalanceToken(childBuilder, BalancingModes.NoErrorOnFailure, SyntaxKind.LeftBrace, SyntaxKind.RightBrace, startingBraceLocation);
                                SpanContext.ChunkGenerator = new StatementChunkGenerator();
                                var existingEditHandler = SpanContext.EditHandler;
                                SpanContext.EditHandler = new CodeBlockEditHandler(Language.TokenizeString);

                                AcceptMarkerTokenIfNecessary();

                                childBuilder.Add(OutputTokensAsStatementLiteral());

                                SpanContext.EditHandler = existingEditHandler;
                            });
                            break;
                    }
                }
                finally
                {
                    if (directiveErrorSink.Errors.Count > 0)
                    {
                        directiveChunkGenerator.Diagnostics.AddRange(directiveErrorSink.Errors);
                    }

                    Context.ErrorSink = savedErrorSink;
                }

                builder.Add(BuildDirective());

                RazorDirectiveSyntax BuildDirective()
                {
                    directiveBuilder.Add(OutputTokensAsStatementLiteral());
                    var directiveCodeBlock = SyntaxFactory.CSharpCodeBlock(directiveBuilder.ToList());

                    var directiveBody = SyntaxFactory.RazorDirectiveBody(keywordBlock, directiveCodeBlock);
                    var directive = SyntaxFactory.RazorDirective(transition, directiveBody);
                    directive = (RazorDirectiveSyntax)directive.SetDiagnostics(directiveErrorSink.Errors.ToArray());
                    return directive;
                }
            }
        }

        private void ValidateDirectiveUsage(DirectiveDescriptor descriptor, SourceLocation directiveStart)
        {
            if (descriptor.Usage == DirectiveUsage.FileScopedSinglyOccurring)
            {
                if (Context.SeenDirectives.Contains(descriptor.Directive))
                {
                    // There will always be at least 1 child because of the `@` transition.
                    var errorLength = /* @ */ 1 + descriptor.Directive.Length;
                    Context.ErrorSink.OnError(
                        RazorDiagnosticFactory.CreateParsing_DuplicateDirective(
                            new SourceSpan(directiveStart, errorLength), descriptor.Directive));

                    return;
                }
            }
        }

        // Used for parsing a qualified name like that which follows the `namespace` keyword.
        //
        // qualified-identifier:
        //      identifier
        //      qualified-identifier . identifier
        protected bool TryParseQualifiedIdentifier(out int identifierLength)
        {
            var currentIdentifierLength = 0;
            var expectingDot = false;
            var tokens = ReadWhile(token =>
            {
                var type = token.Kind;
                if ((expectingDot && type == SyntaxKind.Dot) ||
                    (!expectingDot && type == SyntaxKind.Identifier))
                {
                    expectingDot = !expectingDot;
                    return true;
                }

                if (type != SyntaxKind.Whitespace &&
                    type != SyntaxKind.NewLine)
                {
                    expectingDot = false;
                    currentIdentifierLength += token.Content.Length;
                }

                return false;
            });

            identifierLength = currentIdentifierLength;
            var validQualifiedIdentifier = expectingDot;
            if (validQualifiedIdentifier)
            {
                foreach (var token in tokens)
                {
                    identifierLength += token.Content.Length;
                    AcceptToken(token);
                }

                return true;
            }
            else
            {
                PutCurrentBack();

                foreach (var token in tokens)
                {
                    identifierLength += token.Content.Length;
                    PutBack(token);
                }

                EnsureCurrent();
                return false;
            }
        }

        private void ParseDirectiveBlock(in SyntaxListBuilder<RazorSyntaxNode> builder, DirectiveDescriptor descriptor, Action<SyntaxListBuilder<RazorSyntaxNode>, SourceLocation> parseChildren)
        {
            if (EndOfFile)
            {
                Context.ErrorSink.OnError(
                    RazorDiagnosticFactory.CreateParsing_UnexpectedEOFAfterDirective(
                        new SourceSpan(CurrentStart, contentLength: 1 /* { */), descriptor.Directive, "{"));
            }
            else if (!At(SyntaxKind.LeftBrace))
            {
                Context.ErrorSink.OnError(
                    RazorDiagnosticFactory.CreateParsing_UnexpectedDirectiveLiteral(
                        new SourceSpan(CurrentStart, CurrentToken.Content.Length), descriptor.Directive, "{"));
            }
            else
            {
                var editHandler = new AutoCompleteEditHandler(Language.TokenizeString, autoCompleteAtEndOfSpan: true);
                SpanContext.EditHandler = editHandler;
                var startingBraceLocation = CurrentStart;
                AcceptToken(CurrentToken);
                builder.Add(OutputAsMetaCode(OutputTokens()));

                using (var pooledResult = Pool.Allocate<RazorSyntaxNode>())
                {
                    var childBuilder = pooledResult.Builder;
                    parseChildren(childBuilder, startingBraceLocation);
                    if (childBuilder.Count > 0)
                    {
                        builder.Add(SyntaxFactory.CSharpCodeBlock(childBuilder.ToList()));
                    }
                }

                SpanContext.ChunkGenerator = SpanChunkGenerator.Null;
                if (!OptionalToken(SyntaxKind.RightBrace))
                {
                    editHandler.AutoCompleteString = "}";
                    Context.ErrorSink.OnError(
                        RazorDiagnosticFactory.CreateParsing_ExpectedEndOfBlockBeforeEOF(
                            new SourceSpan(startingBraceLocation, contentLength: 1 /* } */), descriptor.Directive, "}", "{"));

                    AcceptToken(SyntaxFactory.MissingToken(SyntaxKind.RightBrace));
                }
                else
                {
                    SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
                }
                CompleteBlock(insertMarkerIfNecessary: false, captureWhitespaceToEndOfLine: true);
                builder.Add(OutputAsMetaCode(OutputTokens(), SpanContext.EditHandler.AcceptedCharacters));
            }
        }

        private bool TryParseKeyword(in SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            var result = CSharpTokenizer.GetTokenKeyword(CurrentToken);
            Debug.Assert(CurrentToken.Kind == SyntaxKind.Keyword && result.HasValue);
            if (_keywordParserMap.TryGetValue(result.Value, out var handler))
            {
                handler(builder, transition);
                return true;
            }

            return false;
        }

        private void SetupExpressionParsers()
        {
            MapExpressionKeyword(ParseAwaitExpression, CSharpKeyword.Await);
        }

        private void SetupKeywordParsers()
        {
            MapKeywords(
                ParseConditionalBlock,
                CSharpKeyword.For,
                CSharpKeyword.Foreach,
                CSharpKeyword.While,
                CSharpKeyword.Switch,
                CSharpKeyword.Lock);
            MapKeywords(ParseCaseStatement, false, CSharpKeyword.Case, CSharpKeyword.Default);
            MapKeywords(ParseIfStatement, CSharpKeyword.If);
            MapKeywords(ParseTryStatement, CSharpKeyword.Try);
            MapKeywords(ParseDoStatement, CSharpKeyword.Do);
            MapKeywords(ParseUsingKeyword, CSharpKeyword.Using);
            MapKeywords(ParseReservedDirective, CSharpKeyword.Class, CSharpKeyword.Namespace);
        }

        private void MapExpressionKeyword(Action<SyntaxListBuilder<RazorSyntaxNode>, CSharpTransitionSyntax> handler, CSharpKeyword keyword)
        {
            _keywordParserMap.Add(keyword, handler);

            // Expression keywords don't belong in the regular keyword list
        }

        private void MapKeywords(Action<SyntaxListBuilder<RazorSyntaxNode>, CSharpTransitionSyntax> handler, params CSharpKeyword[] keywords)
        {
            MapKeywords(handler, topLevel: true, keywords: keywords);
        }

        private void MapKeywords(Action<SyntaxListBuilder<RazorSyntaxNode>, CSharpTransitionSyntax> handler, bool topLevel, params CSharpKeyword[] keywords)
        {
            foreach (var keyword in keywords)
            {
                _keywordParserMap.Add(keyword, handler);
                if (topLevel)
                {
                    Keywords.Add(CSharpLanguageCharacteristics.GetKeyword(keyword));
                }
            }
        }

        private void ParseAwaitExpression(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            // Ensure that we're on the await statement (only runs in debug)
            Assert(CSharpKeyword.Await);

            // Accept the "await" and move on
            AcceptTokenAndMoveNext();

            // Accept 1 or more spaces between the await and the following code.
            AcceptTokenWhile(IsSpacingToken(includeNewLines: false, includeComments: true));

            // Top level basically indicates if we're within an expression or statement.
            // Ex: topLevel true = @await Foo()  |  topLevel false = @{ await Foo(); }
            // Note that in this case @{ <b>@await Foo()</b> } top level is true for await.
            // Therefore, if we're top level then we want to act like an implicit expression,
            // otherwise just act as whatever we're contained in.
            var topLevel = transition != null;
            if (topLevel)
            {
                // Setup the Span to be an async implicit expression (an implicit expresison that allows spaces).
                // Spaces are allowed because of "@await Foo()".
                var implicitExpressionBody = ParseImplicitExpressionBody(async: true);
                builder.Add(SyntaxFactory.CSharpImplicitExpression(transition, implicitExpressionBody));
            }
        }

        private void ParseConditionalBlock(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            var topLevel = transition != null;
            ParseConditionalBlock(builder, transition, topLevel);
        }

        private void ParseConditionalBlock(in SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition, bool topLevel)
        {
            Assert(SyntaxKind.Keyword);
            if (transition != null)
            {
                builder.Add(transition);
            }

            var block = new Block(CurrentToken, CurrentStart);
            ParseConditionalBlock(builder, block);
            if (topLevel)
            {
                CompleteBlock();
            }
        }

        private void ParseConditionalBlock(in SyntaxListBuilder<RazorSyntaxNode> builder, Block block)
        {
            AcceptTokenAndMoveNext();
            AcceptTokenWhile(IsSpacingToken(includeNewLines: true, includeComments: true));

            // Parse the condition, if present (if not present, we'll let the C# compiler complain)
            if (TryParseCondition(builder))
            {
                AcceptTokenWhile(IsSpacingToken(includeNewLines: true, includeComments: true));

                ParseExpectedCodeBlock(builder, block);
            }
        }

        private bool TryParseCondition(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            if (At(SyntaxKind.LeftParenthesis))
            {
                var complete = BalanceToken(builder, BalancingModes.BacktrackOnFailure | BalancingModes.AllowCommentsAndTemplates);
                if (!complete)
                {
                    AcceptTokenUntil(SyntaxKind.NewLine);
                }
                else
                {
                    OptionalToken(SyntaxKind.RightParenthesis);
                }
                return complete;
            }
            return true;
        }

        private void ParseExpectedCodeBlock(in SyntaxListBuilder<RazorSyntaxNode> builder, Block block)
        {
            if (!EndOfFile)
            {
                // Check for "{" to make sure we're at a block
                if (!At(SyntaxKind.LeftBrace))
                {
                    Context.ErrorSink.OnError(
                        RazorDiagnosticFactory.CreateParsing_SingleLineControlFlowStatementsNotAllowed(
                            new SourceSpan(CurrentStart, CurrentToken.Content.Length),
                            Language.GetSample(SyntaxKind.LeftBrace),
                            CurrentToken.Content));
                }

                // Parse the statement and then we're done
                ParseStatement(builder, block);
            }
        }

        private void ParseUnconditionalBlock(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            Assert(SyntaxKind.Keyword);
            var block = new Block(CurrentToken, CurrentStart);
            AcceptTokenAndMoveNext();
            AcceptTokenWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
            ParseExpectedCodeBlock(builder, block);
        }

        private void ParseCaseStatement(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            Assert(SyntaxKind.Keyword);
            if (transition != null)
            {
                // Normally, case statement won't start with a transition in a valid scenario.
                // If it does, just accept it and let the compiler complain.
                builder.Add(transition);
            }
            var result = CSharpTokenizer.GetTokenKeyword(CurrentToken);
            Debug.Assert(result.HasValue &&
                         (result.Value == CSharpKeyword.Case ||
                          result.Value == CSharpKeyword.Default));
            AcceptTokenUntil(SyntaxKind.Colon);
            OptionalToken(SyntaxKind.Colon);
        }

        private void ParseIfStatement(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            Assert(CSharpKeyword.If);
            ParseConditionalBlock(builder, transition, topLevel: false);
            ParseAfterIfClause(builder);
            var topLevel = transition != null;
            if (topLevel)
            {
                CompleteBlock();
            }
        }

        private void ParseAfterIfClause(SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            // Grab whitespace and razor comments
            var whitespace = SkipToNextImportantToken(builder);

            // Check for an else part
            if (At(CSharpKeyword.Else))
            {
                AcceptToken(whitespace);
                Assert(CSharpKeyword.Else);
                ParseElseClause(builder);
            }
            else
            {
                // No else, return whitespace
                PutCurrentBack();
                PutBack(whitespace);
                SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Any;
            }
        }

        private void ParseElseClause(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            if (!At(CSharpKeyword.Else))
            {
                return;
            }
            var block = new Block(CurrentToken, CurrentStart);

            AcceptTokenAndMoveNext();
            AcceptTokenWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
            if (At(CSharpKeyword.If))
            {
                // ElseIf
                block.Name = SyntaxConstants.CSharp.ElseIfKeyword;
                ParseConditionalBlock(builder, block);
                ParseAfterIfClause(builder);
            }
            else if (!EndOfFile)
            {
                // Else
                ParseExpectedCodeBlock(builder, block);
            }
        }

        private void ParseTryStatement(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            Assert(CSharpKeyword.Try);
            var topLevel = transition != null;
            if (topLevel)
            {
                builder.Add(transition);
            }

            ParseUnconditionalBlock(builder);
            ParseAfterTryClause(builder);
            if (topLevel)
            {
                CompleteBlock();
            }
        }

        private void ParseAfterTryClause(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            // Grab whitespace
            var whitespace = SkipToNextImportantToken(builder);

            // Check for a catch or finally part
            if (At(CSharpKeyword.Catch))
            {
                AcceptToken(whitespace);
                Assert(CSharpKeyword.Catch);
                ParseFilterableCatchBlock(builder);
                ParseAfterTryClause(builder);
            }
            else if (At(CSharpKeyword.Finally))
            {
                AcceptToken(whitespace);
                Assert(CSharpKeyword.Finally);
                ParseUnconditionalBlock(builder);
            }
            else
            {
                // Return whitespace and end the block
                PutCurrentBack();
                PutBack(whitespace);
                SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Any;
            }
        }

        private void ParseFilterableCatchBlock(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            Assert(CSharpKeyword.Catch);

            var block = new Block(CurrentToken, CurrentStart);

            // Accept "catch"
            AcceptTokenAndMoveNext();
            AcceptTokenWhile(IsValidStatementSpacingToken);

            // Parse the catch condition if present. If not present, let the C# compiler complain.
            if (TryParseCondition(builder))
            {
                AcceptTokenWhile(IsValidStatementSpacingToken);

                if (At(CSharpKeyword.When))
                {
                    // Accept "when".
                    AcceptTokenAndMoveNext();
                    AcceptTokenWhile(IsValidStatementSpacingToken);

                    // Parse the filter condition if present. If not present, let the C# compiler complain.
                    if (!TryParseCondition(builder))
                    {
                        // Incomplete condition.
                        return;
                    }

                    AcceptTokenWhile(IsValidStatementSpacingToken);
                }

                ParseExpectedCodeBlock(builder, block);
            }
        }

        private void ParseDoStatement(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            Assert(CSharpKeyword.Do);
            if (transition != null)
            {
                builder.Add(transition);
            }

            ParseUnconditionalBlock(builder);
            ParseWhileClause(builder);
            var topLevel = transition != null;
            if (topLevel)
            {
                CompleteBlock();
            }
        }

        private void ParseWhileClause(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Any;
            var whitespace = SkipToNextImportantToken(builder);

            if (At(CSharpKeyword.While))
            {
                AcceptToken(whitespace);
                Assert(CSharpKeyword.While);
                AcceptTokenAndMoveNext();
                AcceptTokenWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
                if (TryParseCondition(builder) && OptionalToken(SyntaxKind.Semicolon))
                {
                    SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
                }
            }
            else
            {
                PutCurrentBack();
                PutBack(whitespace);
            }
        }

        private void ParseUsingKeyword(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            Assert(CSharpKeyword.Using);
            var topLevel = transition != null;
            var block = new Block(CurrentToken, CurrentStart);
            var usingToken = EatCurrentToken();
            var whitespaceOrComments = ReadWhile(IsSpacingToken(includeNewLines: false, includeComments: true));
            var atLeftParen = At(SyntaxKind.LeftParenthesis);
            var atIdentifier = At(SyntaxKind.Identifier);
            var atStatic = At(CSharpKeyword.Static);

            // Put the read tokens back and let them be handled later.
            PutCurrentBack();
            PutBack(whitespaceOrComments);
            PutBack(usingToken);
            EnsureCurrent();

            if (atLeftParen)
            {
                // using ( ==> Using Statement
                ParseUsingStatement(builder, transition, block);
            }
            else if (atIdentifier || atStatic)
            {
                // using Identifier ==> Using Declaration
                if (!topLevel)
                {
                    Context.ErrorSink.OnError(
                        RazorDiagnosticFactory.CreateParsing_NamespaceImportAndTypeAliasCannotExistWithinCodeBlock(
                            new SourceSpan(block.Start, block.Name.Length)));
                    if (transition != null)
                    {
                        builder.Add(transition);
                    }
                    AcceptTokenAndMoveNext();
                    AcceptTokenWhile(IsSpacingToken(includeNewLines: false, includeComments: true));
                    ParseStandardStatement(builder);
                }
                else
                {
                    ParseUsingDeclaration(builder, transition);
                    return;
                }
            }
            else
            {
                AcceptTokenAndMoveNext();
                AcceptTokenWhile(IsSpacingToken(includeNewLines: false, includeComments: true));
            }

            if (topLevel)
            {
                CompleteBlock();
            }
        }

        private void ParseUsingStatement(in SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition, Block block)
        {
            Assert(CSharpKeyword.Using);
            AcceptTokenAndMoveNext();
            AcceptTokenWhile(IsSpacingToken(includeNewLines: false, includeComments: true));

            Assert(SyntaxKind.LeftParenthesis);
            if (transition != null)
            {
                builder.Add(transition);
            }

            // Parse condition
            if (TryParseCondition(builder))
            {
                AcceptTokenWhile(IsSpacingToken(includeNewLines: true, includeComments: true));

                // Parse code block
                ParseExpectedCodeBlock(builder, block);
            }
        }

        private void ParseUsingDeclaration(in SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            // Using declarations should always be top level. The error case is handled in a different code path.
            Debug.Assert(transition != null);
            using (var pooledResult = Pool.Allocate<RazorSyntaxNode>())
            {
                var directiveBuilder = pooledResult.Builder;
                Assert(CSharpKeyword.Using);
                AcceptTokenAndMoveNext();
                AcceptTokenWhile(IsSpacingToken(includeNewLines: false, includeComments: true));
                var start = CurrentStart;
                if (At(SyntaxKind.Identifier))
                {
                    // non-static using
                    TryParseNamespaceOrTypeName(directiveBuilder);
                    var whitespace = ReadWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
                    if (At(SyntaxKind.Assign))
                    {
                        // Alias
                        AcceptToken(whitespace);
                        Assert(SyntaxKind.Assign);
                        AcceptTokenAndMoveNext();

                        AcceptTokenWhile(IsSpacingToken(includeNewLines: true, includeComments: true));

                        // One more namespace or type name
                        TryParseNamespaceOrTypeName(directiveBuilder);
                    }
                    else
                    {
                        PutCurrentBack();
                        PutBack(whitespace);
                    }
                }
                else if (At(CSharpKeyword.Static))
                {
                    // static using
                    AcceptTokenAndMoveNext();
                    AcceptTokenWhile(IsSpacingToken(includeNewLines: false, includeComments: true));
                    TryParseNamespaceOrTypeName(directiveBuilder);
                }

                SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.AnyExceptNewline;
                SpanContext.ChunkGenerator = new AddImportChunkGenerator(new LocationTagged<string>(
                    string.Concat(TokenBuilder.ToList().Nodes.Select(s => s.Content)),
                    start));

                // Optional ";"
                if (EnsureCurrent())
                {
                    OptionalToken(SyntaxKind.Semicolon);
                }

                CompleteBlock();
                Debug.Assert(directiveBuilder.Count == 0, "We should not have built any blocks so far.");
                var keywordTokens = OutputTokensAsStatementLiteral();
                var directiveBody = SyntaxFactory.RazorDirectiveBody(keywordTokens, null);
                builder.Add(SyntaxFactory.RazorDirective(transition, directiveBody));
            }
        }

        private bool TryParseNamespaceOrTypeName(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            if (OptionalToken(SyntaxKind.LeftParenthesis))
            {
                while (!OptionalToken(SyntaxKind.RightParenthesis) && !EndOfFile)
                {
                    OptionalToken(SyntaxKind.Whitespace);

                    if (!TryParseNamespaceOrTypeName(builder))
                    {
                        return false;
                    }

                    OptionalToken(SyntaxKind.Whitespace);
                    OptionalToken(SyntaxKind.Identifier);
                    OptionalToken(SyntaxKind.Whitespace);
                    OptionalToken(SyntaxKind.Comma);
                }

                if (At(SyntaxKind.Whitespace) && NextIs(SyntaxKind.QuestionMark))
                {
                    // Only accept the whitespace if we are going to consume the next token.
                    AcceptTokenAndMoveNext();
                }

                OptionalToken(SyntaxKind.QuestionMark); // Nullable

                return true;
            }
            else if (OptionalToken(SyntaxKind.Identifier) || OptionalToken(SyntaxKind.Keyword))
            {
                if (OptionalToken(SyntaxKind.DoubleColon))
                {
                    if (!OptionalToken(SyntaxKind.Identifier))
                    {
                        OptionalToken(SyntaxKind.Keyword);
                    }
                }
                if (At(SyntaxKind.LessThan))
                {
                    ParseTypeArgumentList(builder);
                }
                if (OptionalToken(SyntaxKind.Dot))
                {
                    TryParseNamespaceOrTypeName(builder);
                }

                if (At(SyntaxKind.Whitespace) && NextIs(SyntaxKind.QuestionMark))
                {
                    // Only accept the whitespace if we are going to consume the next token.
                    AcceptTokenAndMoveNext();
                }

                OptionalToken(SyntaxKind.QuestionMark); // Nullable

                if (At(SyntaxKind.Whitespace) && NextIs(SyntaxKind.LeftBracket))
                {
                    // Only accept the whitespace if we are going to consume the next token.
                    AcceptTokenAndMoveNext();
                }

                while (At(SyntaxKind.LeftBracket))
                {
                    BalanceToken(builder, BalancingModes.None);
                    if (!OptionalToken(SyntaxKind.RightBracket))
                    {
                        AcceptToken(SyntaxFactory.MissingToken(SyntaxKind.RightBracket));
                    }
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void ParseTypeArgumentList(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            Assert(SyntaxKind.LessThan);
            BalanceToken(builder, BalancingModes.None);
            if (!OptionalToken(SyntaxKind.GreaterThan))
            {
                AcceptToken(SyntaxFactory.MissingToken(SyntaxKind.GreaterThan));
            }
        }

        private void ParseReservedDirective(SyntaxListBuilder<RazorSyntaxNode> builder, CSharpTransitionSyntax transition)
        {
            Context.ErrorSink.OnError(
                RazorDiagnosticFactory.CreateParsing_ReservedWord(
                    new SourceSpan(CurrentStart, CurrentToken.Content.Length), CurrentToken.Content));
                
            AcceptTokenAndMoveNext();
            SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
            SpanContext.ChunkGenerator = SpanChunkGenerator.Null;
            CompleteBlock();
            var keyword = OutputAsMetaCode(OutputTokens());
            var directiveBody = SyntaxFactory.RazorDirectiveBody(keyword, cSharpCode: null);
            var directive = SyntaxFactory.RazorDirective(transition, directiveBody);
            builder.Add(directive);
        }

        protected void CompleteBlock()
        {
            CompleteBlock(insertMarkerIfNecessary: true);
        }

        protected void CompleteBlock(bool insertMarkerIfNecessary)
        {
            CompleteBlock(insertMarkerIfNecessary, captureWhitespaceToEndOfLine: insertMarkerIfNecessary);
        }

        protected void CompleteBlock(bool insertMarkerIfNecessary, bool captureWhitespaceToEndOfLine)
        {
            if (insertMarkerIfNecessary && Context.LastAcceptedCharacters != AcceptedCharactersInternal.Any)
            {
                AcceptMarkerTokenIfNecessary();
            }

            EnsureCurrent();

            // Read whitespace, but not newlines
            // If we're not inserting a marker span, we don't need to capture whitespace
            if (!Context.WhiteSpaceIsSignificantToAncestorBlock &&
                captureWhitespaceToEndOfLine &&
                !Context.DesignTimeMode &&
                !IsNested)
            {
                CaptureWhitespaceAtEndOfCodeOnlyLine();
            }
            else
            {
                PutCurrentBack();
            }
        }

        private IEnumerable<SyntaxToken> SkipToNextImportantToken(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            while (!EndOfFile)
            {
                var whitespace = ReadWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
                if (At(SyntaxKind.RazorCommentTransition))
                {
                    AcceptToken(whitespace);
                    SpanContext.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Any;
                    AcceptMarkerTokenIfNecessary();
                    builder.Add(OutputTokensAsStatementLiteral());
                    var comment = ParseRazorComment();
                    builder.Add(comment);
                }
                else
                {
                    return whitespace;
                }
            }
            return Enumerable.Empty<SyntaxToken>();
        }

        private void CaptureWhitespaceAtEndOfCodeOnlyLine()
        {
            var whitespace = ReadWhile(token => token.Kind == SyntaxKind.Whitespace);
            if (At(SyntaxKind.NewLine))
            {
                AcceptToken(whitespace);
                AcceptTokenAndMoveNext();
                PutCurrentBack();
            }
            else
            {
                PutCurrentBack();
                PutBack(whitespace);
            }
        }

        protected override bool IsAtEmbeddedTransition(bool allowTemplatesAndComments, bool allowTransitions)
        {
            // No embedded transitions in C#, so ignore that param
            return allowTemplatesAndComments
                   && ((Language.IsTransition(CurrentToken)
                        && NextIs(SyntaxKind.LessThan, SyntaxKind.Colon, SyntaxKind.DoubleColon))
                       || Language.IsCommentStart(CurrentToken));
        }

        protected override void ParseEmbeddedTransition(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            if (Language.IsTransition(CurrentToken))
            {
                PutCurrentBack();
                ParseTemplate(builder);
            }
            else if (Language.IsCommentStart(CurrentToken))
            {
                // Output tokens before parsing the comment.
                AcceptMarkerTokenIfNecessary();
                builder.Add(OutputTokensAsStatementLiteral());
                var comment = ParseRazorComment();
                builder.Add(comment);
            }
        }

        private void DefaultSpanContextConfig(SpanContextBuilder spanContext)
        {
            spanContext.EditHandler = SpanEditHandler.CreateDefault(Language.TokenizeString);
            spanContext.ChunkGenerator = new StatementChunkGenerator();
        }

        private void ExplicitExpressionSpanContextConfig(SpanContextBuilder spanContext)
        {
            spanContext.EditHandler = SpanEditHandler.CreateDefault(Language.TokenizeString);
            spanContext.ChunkGenerator = new ExpressionChunkGenerator();
        }

        private CSharpStatementLiteralSyntax OutputTokensAsStatementLiteral()
        {
            var tokens = OutputTokens();
            if (tokens.Count == 0)
            {
                return null;
            }

            return GetNodeWithSpanContext(SyntaxFactory.CSharpStatementLiteral(tokens));
        }

        private CSharpExpressionLiteralSyntax OutputTokensAsExpressionLiteral()
        {
            var tokens = OutputTokens();
            if (tokens.Count == 0)
            {
                return null;
            }

            return GetNodeWithSpanContext(SyntaxFactory.CSharpExpressionLiteral(tokens));
        }

        private CSharpEphemeralTextLiteralSyntax OutputTokensAsEphemeralLiteral()
        {
            var tokens = OutputTokens();
            if (tokens.Count == 0)
            {
                return null;
            }

            return GetNodeWithSpanContext(SyntaxFactory.CSharpEphemeralTextLiteral(tokens));
        }

        private UnclassifiedTextLiteralSyntax OutputTokensAsUnclassifiedLiteral()
        {
            var tokens = OutputTokens();
            if (tokens.Count == 0)
            {
                return null;
            }


            return GetNodeWithSpanContext(SyntaxFactory.UnclassifiedTextLiteral(tokens));
        }

        private void OtherParserBlock(in SyntaxListBuilder<RazorSyntaxNode> builder)
        {
            // When transitioning to the HTML parser we no longer want to act as if we're in a nested C# state.
            // For instance, if <div>@hello.</div> is in a nested C# block we don't want the trailing '.' to be handled
            // as C#; it should be handled as a period because it's wrapped in markup.
            var wasNested = IsNested;
            IsNested = false;

            RazorSyntaxNode htmlBlock = null;
            using (PushSpanContextConfig())
            {
                htmlBlock = HtmlParser.ParseBlock();
            }

            builder.Add(htmlBlock);
            InitializeContext(SpanContext);

            IsNested = wasNested;
            NextToken();
        }
    }
}
