// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal class HtmlMarkupParser : TokenizerBackedParser<HtmlTokenizer, HtmlToken, HtmlTokenType>
    {
        private const string ScriptTagName = "script";

        private static readonly HtmlToken[] nonAllowedHtmlCommentEnding = new[] { HtmlToken.Hyphen, new HtmlToken("!", HtmlTokenType.Bang), new HtmlToken("<", HtmlTokenType.OpenAngle) };
        private static readonly HtmlToken[] singleHyphenArray = new[] { HtmlToken.Hyphen };

        private static readonly char[] ValidAfterTypeAttributeNameCharacters = { ' ', '\t', '\r', '\n', '\f', '=' };
        private SourceLocation _lastTagStart = SourceLocation.Zero;
        private HtmlToken _bufferedOpenAngle;

        //From http://dev.w3.org/html5/spec/Overview.html#elements-0
        private ISet<string> _voidElements = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "area",
            "base",
            "br",
            "col",
            "command",
            "embed",
            "hr",
            "img",
            "input",
            "keygen",
            "link",
            "meta",
            "param",
            "source",
            "track",
            "wbr"
        };

        public HtmlMarkupParser(ParserContext context)
            : base(context.ParseLeadingDirectives ? FirstDirectiveHtmlLanguageCharacteristics.Instance : HtmlLanguageCharacteristics.Instance, context)
        {
        }

        public ParserBase CodeParser { get; set; }

        public ISet<string> VoidElements
        {
            get { return _voidElements; }
        }

        private bool CaseSensitive { get; set; }

        private StringComparison Comparison
        {
            get { return CaseSensitive ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase; }
        }

        protected override bool TokenTypeEquals(HtmlTokenType x, HtmlTokenType y) => x == y;

        public override void BuildSpan(SpanBuilder span, SourceLocation start, string content)
        {
            span.Kind = SpanKindInternal.Markup;
            span.ChunkGenerator = new MarkupChunkGenerator();
            base.BuildSpan(span, start, content);
        }

        protected override void OutputSpanBeforeRazorComment()
        {
            Output(SpanKindInternal.Markup);
        }

        protected void SkipToAndParseCode(HtmlTokenType type)
        {
            SkipToAndParseCode(token => token.Type == type);
        }

        protected void SkipToAndParseCode(Func<HtmlToken, bool> condition)
        {
            HtmlToken last = null;
            var startOfLine = false;
            while (!EndOfFile && !condition(CurrentToken))
            {
                if (Context.NullGenerateWhitespaceAndNewLine)
                {
                    Context.NullGenerateWhitespaceAndNewLine = false;
                    Span.ChunkGenerator = SpanChunkGenerator.Null;
                    AcceptWhile(token => token.Type == HtmlTokenType.WhiteSpace);
                    if (At(HtmlTokenType.NewLine))
                    {
                        AcceptAndMoveNext();
                    }

                    Output(SpanKindInternal.Markup);
                }
                else if (At(HtmlTokenType.NewLine))
                {
                    if (last != null)
                    {
                        Accept(last);
                    }

                    // Mark the start of a new line
                    startOfLine = true;
                    last = null;
                    AcceptAndMoveNext();
                }
                else if (At(HtmlTokenType.Transition))
                {
                    var transition = CurrentToken;
                    NextToken();
                    if (At(HtmlTokenType.Transition))
                    {
                        if (last != null)
                        {
                            Accept(last);
                            last = null;
                        }
                        Output(SpanKindInternal.Markup);
                        Accept(transition);
                        Span.ChunkGenerator = SpanChunkGenerator.Null;
                        Output(SpanKindInternal.Markup);
                        AcceptAndMoveNext();
                        continue; // while
                    }
                    else
                    {
                        if (!EndOfFile)
                        {
                            PutCurrentBack();
                        }
                        PutBack(transition);
                    }

                    // Handle whitespace rewriting
                    if (last != null)
                    {
                        if (!Context.DesignTimeMode && last.Type == HtmlTokenType.WhiteSpace && startOfLine)
                        {
                            // Put the whitespace back too
                            startOfLine = false;
                            PutBack(last);
                            last = null;
                        }
                        else
                        {
                            // Accept last
                            Accept(last);
                            last = null;
                        }
                    }

                    OtherParserBlock();
                }
                else if (At(HtmlTokenType.RazorCommentTransition))
                {
                    if (last != null)
                    {
                        // Don't render the whitespace between the start of the line and the razor comment.
                        if (startOfLine && last.Type == HtmlTokenType.WhiteSpace)
                        {
                            AddMarkerTokenIfNecessary();
                            // Output the tokens that may have been accepted prior to the whitespace.
                            Output(SpanKindInternal.Markup);

                            Span.ChunkGenerator = SpanChunkGenerator.Null;
                        }

                        Accept(last);
                        last = null;
                    }

                    AddMarkerTokenIfNecessary();
                    Output(SpanKindInternal.Markup);

                    RazorComment();

                    // Handle the whitespace and newline at the end of a razor comment.
                    if (startOfLine &&
                        (At(HtmlTokenType.NewLine) ||
                        (At(HtmlTokenType.WhiteSpace) && NextIs(HtmlTokenType.NewLine))))
                    {
                        AcceptWhile(IsSpacingToken(includeNewLines: false));
                        AcceptAndMoveNext();
                        Span.ChunkGenerator = SpanChunkGenerator.Null;
                        Output(SpanKindInternal.Markup);
                    }
                }
                else
                {
                    // As long as we see whitespace, we're still at the "start" of the line
                    startOfLine &= At(HtmlTokenType.WhiteSpace);

                    // If there's a last token, accept it
                    if (last != null)
                    {
                        Accept(last);
                        last = null;
                    }

                    // Advance
                    last = CurrentToken;
                    NextToken();
                }
            }

            if (last != null)
            {
                Accept(last);
            }
        }

        protected static Func<HtmlToken, bool> IsSpacingToken(bool includeNewLines)
        {
            return token => token.Type == HtmlTokenType.WhiteSpace || (includeNewLines && token.Type == HtmlTokenType.NewLine);
        }

        private void OtherParserBlock()
        {
            AddMarkerTokenIfNecessary();
            Output(SpanKindInternal.Markup);

            using (PushSpanConfig())
            {
                CodeParser.ParseBlock();
            }

            Span.Start = CurrentLocation;
            Initialize(Span);
            NextToken();
        }

        private bool IsBangEscape(int lookahead)
        {
            var potentialBang = Lookahead(lookahead);

            if (potentialBang != null &&
                potentialBang.Type == HtmlTokenType.Bang)
            {
                var afterBang = Lookahead(lookahead + 1);

                return afterBang != null &&
                    afterBang.Type == HtmlTokenType.Text &&
                    !string.Equals(afterBang.Content, "DOCTYPE", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private void OptionalBangEscape()
        {
            if (IsBangEscape(lookahead: 0))
            {
                Output(SpanKindInternal.Markup);

                // Accept the parser escape character '!'.
                Assert(HtmlTokenType.Bang);
                AcceptAndMoveNext();

                // Setup the metacode span that we will be outputing.
                Span.ChunkGenerator = SpanChunkGenerator.Null;
                Output(SpanKindInternal.MetaCode, AcceptedCharactersInternal.None);
            }
        }

        public override void ParseBlock()
        {
            if (Context == null)
            {
                throw new InvalidOperationException(Resources.Parser_Context_Not_Set);
            }

            using (PushSpanConfig(DefaultMarkupSpan))
            {
                using (Context.Builder.StartBlock(BlockKindInternal.Markup))
                {
                    Span.Start = CurrentLocation;

                    if (!NextToken())
                    {
                        return;
                    }

                    AcceptWhile(IsSpacingToken(includeNewLines: true));

                    if (CurrentToken.Type == HtmlTokenType.OpenAngle)
                    {
                        // "<" => Implicit Tag Block
                        TagBlock(new Stack<Tuple<HtmlToken, SourceLocation>>());
                    }
                    else if (CurrentToken.Type == HtmlTokenType.Transition)
                    {
                        // "@" => Explicit Tag/Single Line Block OR Template
                        Output(SpanKindInternal.Markup);

                        // Definitely have a transition span
                        Assert(HtmlTokenType.Transition);
                        AcceptAndMoveNext();
                        Span.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
                        Span.ChunkGenerator = SpanChunkGenerator.Null;
                        Output(SpanKindInternal.Transition);
                        if (At(HtmlTokenType.Transition))
                        {
                            Span.ChunkGenerator = SpanChunkGenerator.Null;
                            AcceptAndMoveNext();
                            Output(SpanKindInternal.MetaCode);
                        }
                        AfterTransition();
                    }
                    else
                    {
                        Context.ErrorSink.OnError(
                            RazorDiagnosticFactory.CreateParsing_MarkupBlockMustStartWithTag(
                                new SourceSpan(CurrentStart, CurrentToken.Content.Length)));
                    }
                    Output(SpanKindInternal.Markup);
                }
            }
        }

        private void DefaultMarkupSpan(SpanBuilder span)
        {
            span.ChunkGenerator = new MarkupChunkGenerator();
            span.EditHandler = new SpanEditHandler(Language.TokenizeString, AcceptedCharactersInternal.Any);
        }

        private void AfterTransition()
        {
            // "@:" => Explicit Single Line Block
            if (CurrentToken.Type == HtmlTokenType.Text && CurrentToken.Content.Length > 0 && CurrentToken.Content[0] == ':')
            {
                // Split the token
                Tuple<HtmlToken, HtmlToken> split = Language.SplitToken(CurrentToken, 1, HtmlTokenType.Colon);

                // The first part (left) is added to this span and we return a MetaCode span
                Accept(split.Item1);
                Span.ChunkGenerator = SpanChunkGenerator.Null;
                Output(SpanKindInternal.MetaCode);
                if (split.Item2 != null)
                {
                    Accept(split.Item2);
                }
                NextToken();
                SingleLineMarkup();
            }
            else if (CurrentToken.Type == HtmlTokenType.OpenAngle)
            {
                TagBlock(new Stack<Tuple<HtmlToken, SourceLocation>>());
            }
        }

        private void SingleLineMarkup()
        {
            // Parse until a newline, it's that simple!
            // First, signal to code parser that whitespace is significant to us.
            var old = Context.WhiteSpaceIsSignificantToAncestorBlock;
            Context.WhiteSpaceIsSignificantToAncestorBlock = true;
            Span.EditHandler = new SpanEditHandler(Language.TokenizeString);
            SkipToAndParseCode(HtmlTokenType.NewLine);
            if (!EndOfFile && CurrentToken.Type == HtmlTokenType.NewLine)
            {
                AcceptAndMoveNext();
                Span.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
            }
            PutCurrentBack();
            Context.WhiteSpaceIsSignificantToAncestorBlock = old;
            Output(SpanKindInternal.Markup);
        }

        private void TagBlock(Stack<Tuple<HtmlToken, SourceLocation>> tags)
        {
            // Skip Whitespace and Text
            var complete = false;
            do
            {
                SkipToAndParseCode(HtmlTokenType.OpenAngle);

                // Output everything prior to the OpenAngle into a markup span
                Output(SpanKindInternal.Markup);

                // Do not want to start a new tag block if we're at the end of the file.
                IDisposable tagBlockWrapper = null;
                try
                {
                    var atSpecialTag = AtSpecialTag;

                    if (!EndOfFile && !atSpecialTag)
                    {
                        // Start a Block tag.  This is used to wrap things like <p> or <a class="btn"> etc.
                        tagBlockWrapper = Context.Builder.StartBlock(BlockKindInternal.Tag);
                    }

                    if (EndOfFile)
                    {
                        EndTagBlock(tags, complete: true);
                    }
                    else
                    {
                        _bufferedOpenAngle = null;
                        _lastTagStart = CurrentStart;
                        Assert(HtmlTokenType.OpenAngle);
                        _bufferedOpenAngle = CurrentToken;
                        var tagStart = CurrentStart;
                        if (!NextToken())
                        {
                            Accept(_bufferedOpenAngle);
                            EndTagBlock(tags, complete: false);
                        }
                        else
                        {
                            complete = AfterTagStart(tagStart, tags, atSpecialTag, tagBlockWrapper);
                        }
                    }

                    if (complete)
                    {
                        // Completed tags have no accepted characters inside of blocks.
                        Span.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
                    }

                    // Output the contents of the tag into its own markup span.
                    Output(SpanKindInternal.Markup);
                }
                finally
                {
                    // Will be null if we were at end of file or special tag when initially created.
                    if (tagBlockWrapper != null)
                    {
                        // End tag block
                        tagBlockWrapper.Dispose();
                    }
                }
            }
            while (tags.Count > 0);

            EndTagBlock(tags, complete);
        }

        private bool AfterTagStart(SourceLocation tagStart,
                                   Stack<Tuple<HtmlToken, SourceLocation>> tags,
                                   bool atSpecialTag,
                                   IDisposable tagBlockWrapper)
        {
            if (!EndOfFile)
            {
                switch (CurrentToken.Type)
                {
                    case HtmlTokenType.ForwardSlash:
                        // End Tag
                        return EndTag(tagStart, tags, tagBlockWrapper);
                    case HtmlTokenType.Bang:
                        // Comment, CDATA, DOCTYPE, or a parser-escaped HTML tag.
                        if (atSpecialTag)
                        {
                            Accept(_bufferedOpenAngle);
                            return BangTag();
                        }
                        else
                        {
                            goto default;
                        }
                    case HtmlTokenType.QuestionMark:
                        // XML PI
                        Accept(_bufferedOpenAngle);
                        return XmlPI();
                    default:
                        // Start Tag
                        return StartTag(tags, tagBlockWrapper);
                }
            }
            if (tags.Count == 0)
            {
                Context.ErrorSink.OnError(
                    RazorDiagnosticFactory.CreateParsing_OuterTagMissingName(
                        new SourceSpan(CurrentStart, contentLength: 1 /* end of file */)));
            }
            return false;
        }

        private bool XmlPI()
        {
            // Accept "?"
            Assert(HtmlTokenType.QuestionMark);
            AcceptAndMoveNext();
            return AcceptUntilAll(HtmlTokenType.QuestionMark, HtmlTokenType.CloseAngle);
        }

        private bool BangTag()
        {
            // Accept "!"
            Assert(HtmlTokenType.Bang);

            if (AcceptAndMoveNext())
            {
                if (IsHtmlCommentAhead())
                {
                    using (Context.Builder.StartBlock(BlockKindInternal.HtmlComment))
                    {
                        // Accept the double-hyphen token at the beginning of the comment block.
                        AcceptAndMoveNext();
                        Output(SpanKindInternal.Markup, AcceptedCharactersInternal.None);

                        Span.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.WhiteSpace;
                        while (!EndOfFile)
                        {
                            SkipToAndParseCode(HtmlTokenType.DoubleHyphen);
                            var lastDoubleHyphen = AcceptAllButLastDoubleHyphens();

                            if (At(HtmlTokenType.CloseAngle))
                            {
                                // Output the content in the comment block as a separate markup
                                Output(SpanKindInternal.Markup, AcceptedCharactersInternal.WhiteSpace);

                                // This is the end of a comment block
                                Accept(lastDoubleHyphen);
                                AcceptAndMoveNext();
                                Output(SpanKindInternal.Markup, AcceptedCharactersInternal.None);
                                return true;
                            }
                            else if (lastDoubleHyphen != null)
                            {
                                Accept(lastDoubleHyphen);
                            }
                        }
                    }
                }
                else if (CurrentToken.Type == HtmlTokenType.LeftBracket)
                {
                    if (AcceptAndMoveNext())
                    {
                        return CData();
                    }
                }
                else
                {
                    AcceptAndMoveNext();
                    return AcceptUntilAll(HtmlTokenType.CloseAngle);
                }
            }

            return false;
        }

        protected HtmlToken AcceptAllButLastDoubleHyphens()
        {
            var lastDoubleHyphen = CurrentToken;
            AcceptWhile(s =>
            {
                if (NextIs(HtmlTokenType.DoubleHyphen))
                {
                    lastDoubleHyphen = s;
                    return true;
                }

                return false;
            });

            NextToken();

            if (At(HtmlTokenType.Text) && IsHyphen(CurrentToken))
            {
                // Doing this here to maintain the order of tokens
                if (!NextIs(HtmlTokenType.CloseAngle))
                {
                    Accept(lastDoubleHyphen);
                    lastDoubleHyphen = null;
                }

                AcceptAndMoveNext();
            }

            return lastDoubleHyphen;
        }

        internal static bool IsHyphen(HtmlToken token)
        {
            return token.Equals(HtmlToken.Hyphen);
        }

        protected bool IsHtmlCommentAhead()
        {
            // From HTML5 Specification, available at http://www.w3.org/TR/html52/syntax.html#comments
            
            // Comments must have the following format:
            // 1. The string "<!--"
            // 2. Optionally, text, with the additional restriction that the text
            //      2.1 must not start with the string ">" nor start with the string "->"
            //      2.2 nor contain the strings
            //          2.2.1 "<!--"
            //          2.2.2 "-->" As we will be treating this as a comment ending, there is no need to handle this case at all.
            //          2.2.3 "--!>"
            //      2.3 nor end with the string "<!-".
            // 3. The string "-->"

            if (CurrentToken.Type != HtmlTokenType.DoubleHyphen)
            {
                return false;
            }

            // Check condition 2.1
            if (NextIs(HtmlTokenType.CloseAngle) || NextIs(next => IsHyphen(next) && NextIs(HtmlTokenType.CloseAngle)))
            {
                return false;
            }

            // Check condition 2.2
            var isValidComment = false;
            LookaheadUntil((token, prevTokens) =>
            {
                if (token.Type == HtmlTokenType.DoubleHyphen)
                {
                    if (NextIs(HtmlTokenType.CloseAngle))
                    {
                        // Check condition 2.3: We're at the end of a comment. Check to make sure the text ending is allowed.
                        isValidComment = !IsCommentContentEndingInvalid(prevTokens);
                        return true;
                    }
                    else if (NextIs(ns => IsHyphen(ns) && NextIs(HtmlTokenType.CloseAngle)))
                    {
                        // Check condition 2.3: we're at the end of a comment, which has an extra dash.
                        // Need to treat the dash as part of the content and check the ending.
                        // However, that case would have already been checked as part of check from 2.2.1 which
                        // would already fail this iteration and we wouldn't get here
                        isValidComment = true;
                        return true;
                    }
                    else if (NextIs(ns => ns.Type == HtmlTokenType.Bang && NextIs(HtmlTokenType.CloseAngle)))
                    {
                        // This is condition 2.2.3
                        isValidComment = false;
                        return true;
                    }
                }
                else if (token.Type == HtmlTokenType.OpenAngle)
                {
                    // Checking condition 2.2.1
                    if (NextIs(ns => ns.Type == HtmlTokenType.Bang && NextIs(HtmlTokenType.DoubleHyphen)))
                    {
                        isValidComment = false;
                        return true;
                    }
                }

                return false;
            });

            return isValidComment;
        }

        /// <summary>
        /// Verifies, that the sequence doesn't end with the "&lt;!-" HtmlTokens. Note, the first token is an opening bracket token
        /// </summary>
        internal static bool IsCommentContentEndingInvalid(IEnumerable<HtmlToken> sequence)
        {
            var reversedSequence = sequence.Reverse();
            var index = 0;
            foreach (var item in reversedSequence)
            {
                if (!item.Equals(nonAllowedHtmlCommentEnding[index++]))
                {
                    return false;
                }

                if (index == nonAllowedHtmlCommentEnding.Length)
                {
                    return true;
                }
            }

            return false;
        }

        private bool CData()
        {
            if (CurrentToken.Type == HtmlTokenType.Text && string.Equals(CurrentToken.Content, "cdata", StringComparison.OrdinalIgnoreCase))
            {
                if (AcceptAndMoveNext())
                {
                    if (CurrentToken.Type == HtmlTokenType.LeftBracket)
                    {
                        return AcceptUntilAll(HtmlTokenType.RightBracket, HtmlTokenType.RightBracket, HtmlTokenType.CloseAngle);
                    }
                }
            }

            return false;
        }

        private bool EndTag(SourceLocation tagStart,
                            Stack<Tuple<HtmlToken, SourceLocation>> tags,
                            IDisposable tagBlockWrapper)
        {
            // Accept "/" and move next
            Assert(HtmlTokenType.ForwardSlash);
            var forwardSlash = CurrentToken;
            if (!NextToken())
            {
                Accept(_bufferedOpenAngle);
                Accept(forwardSlash);
                return false;
            }
            else
            {
                var tagName = string.Empty;
                HtmlToken bangToken = null;

                if (At(HtmlTokenType.Bang))
                {
                    bangToken = CurrentToken;

                    var nextToken = Lookahead(count: 1);

                    if (nextToken != null && nextToken.Type == HtmlTokenType.Text)
                    {
                        tagName = "!" + nextToken.Content;
                    }
                }
                else if (At(HtmlTokenType.Text))
                {
                    tagName = CurrentToken.Content;
                }

                var matched = RemoveTag(tags, tagName, tagStart);

                if (tags.Count == 0 &&
                    // Note tagName may contain a '!' escape character. This ensures </!text> doesn't match here.
                    // </!text> tags are treated like any other escaped HTML end tag.
                    string.Equals(tagName, SyntaxConstants.TextTagName, StringComparison.OrdinalIgnoreCase) &&
                    matched)
                {
                    return EndTextTag(forwardSlash, tagBlockWrapper);
                }
                Accept(_bufferedOpenAngle);
                Accept(forwardSlash);

                OptionalBangEscape();

                AcceptUntil(HtmlTokenType.CloseAngle);

                // Accept the ">"
                return Optional(HtmlTokenType.CloseAngle);
            }
        }

        private void RecoverTextTag()
        {
            // We don't want to skip-to and parse because there shouldn't be anything in the body of text tags.
            AcceptUntil(HtmlTokenType.CloseAngle, HtmlTokenType.NewLine);

            // Include the close angle in the text tag block if it's there, otherwise just move on
            Optional(HtmlTokenType.CloseAngle);
        }

        private bool EndTextTag(HtmlToken solidus, IDisposable tagBlockWrapper)
        {
            Accept(_bufferedOpenAngle);
            Accept(solidus);

            var textLocation = CurrentStart;
            Assert(HtmlTokenType.Text);
            AcceptAndMoveNext();

            var seenCloseAngle = Optional(HtmlTokenType.CloseAngle);

            if (!seenCloseAngle)
            {
                Context.ErrorSink.OnError(
                    RazorDiagnosticFactory.CreateParsing_TextTagCannotContainAttributes(
                        new SourceSpan(textLocation, contentLength: 4 /* text */)));

                Span.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Any;
                RecoverTextTag();
            }
            else
            {
                Span.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
            }

            Span.ChunkGenerator = SpanChunkGenerator.Null;

            CompleteTagBlockWithSpan(tagBlockWrapper, Span.EditHandler.AcceptedCharacters, SpanKindInternal.Transition);

            return seenCloseAngle;
        }

        // Special tags include <!--, <!DOCTYPE, <![CDATA and <? tags
        private bool AtSpecialTag
        {
            get
            {
                if (At(HtmlTokenType.OpenAngle))
                {
                    if (NextIs(HtmlTokenType.Bang))
                    {
                        return !IsBangEscape(lookahead: 1);
                    }

                    return NextIs(HtmlTokenType.QuestionMark);
                }

                return false;
            }
        }

        private bool IsTagRecoveryStopPoint(HtmlToken token)
        {
            return token.Type == HtmlTokenType.CloseAngle ||
                   token.Type == HtmlTokenType.ForwardSlash ||
                   token.Type == HtmlTokenType.OpenAngle ||
                   token.Type == HtmlTokenType.SingleQuote ||
                   token.Type == HtmlTokenType.DoubleQuote;
        }

        private void TagContent()
        {
            if (!At(HtmlTokenType.WhiteSpace) && !At(HtmlTokenType.NewLine))
            {
                // We should be right after the tag name, so if there's no whitespace or new line, something is wrong
                RecoverToEndOfTag();
            }
            else
            {
                // We are here ($): <tag$ foo="bar" biz="~/Baz" />
                while (!EndOfFile && !IsEndOfTag())
                {
                    BeforeAttribute();
                }
            }
        }

        private bool IsEndOfTag()
        {
            if (At(HtmlTokenType.ForwardSlash))
            {
                if (NextIs(HtmlTokenType.CloseAngle))
                {
                    return true;
                }
                else
                {
                    AcceptAndMoveNext();
                }
            }
            return At(HtmlTokenType.CloseAngle) || At(HtmlTokenType.OpenAngle);
        }

        private void BeforeAttribute()
        {
            // http://dev.w3.org/html5/spec/tokenization.html#before-attribute-name-state
            // Capture whitespace
            var whitespace = ReadWhile(token => token.Type == HtmlTokenType.WhiteSpace || token.Type == HtmlTokenType.NewLine);

            if (At(HtmlTokenType.Transition))
            {
                // Transition outside of attribute value => Switch to recovery mode
                Accept(whitespace);
                RecoverToEndOfTag();
                return;
            }

            // http://dev.w3.org/html5/spec/tokenization.html#attribute-name-state
            // Read the 'name' (i.e. read until the '=' or whitespace/newline)
            var name = Enumerable.Empty<HtmlToken>();
            var whitespaceAfterAttributeName = Enumerable.Empty<HtmlToken>();
            if (IsValidAttributeNameToken(CurrentToken))
            {
                name = ReadWhile(token =>
                                 token.Type != HtmlTokenType.WhiteSpace &&
                                 token.Type != HtmlTokenType.NewLine &&
                                 token.Type != HtmlTokenType.Equals &&
                                 token.Type != HtmlTokenType.CloseAngle &&
                                 token.Type != HtmlTokenType.OpenAngle &&
                                 (token.Type != HtmlTokenType.ForwardSlash || !NextIs(HtmlTokenType.CloseAngle)));

                // capture whitespace after attribute name (if any)
                whitespaceAfterAttributeName = ReadWhile(
                    token => token.Type == HtmlTokenType.WhiteSpace || token.Type == HtmlTokenType.NewLine);
            }
            else
            {
                // Unexpected character in tag, enter recovery
                Accept(whitespace);
                RecoverToEndOfTag();
                return;
            }

            if (!At(HtmlTokenType.Equals))
            {
                // Minimized attribute

                // We are at the prefix of the next attribute or the end of tag. Put it back so it is parsed later.
                PutCurrentBack();
                PutBack(whitespaceAfterAttributeName);

                // Output anything prior to the attribute, in most cases this will be the tag name:
                // |<input| checked />. If in-between other attributes this will noop or output malformed attribute
                // content (if the previous attribute was malformed).
                Output(SpanKindInternal.Markup);

                using (Context.Builder.StartBlock(BlockKindInternal.Markup))
                {
                    Accept(whitespace);
                    Accept(name);
                    Output(SpanKindInternal.Markup);
                }

                return;
            }

            // Not a minimized attribute, parse as if it were well-formed (if attribute turns out to be malformed we
            // will go into recovery).
            Output(SpanKindInternal.Markup);

            // Start a new markup block for the attribute
            using (Context.Builder.StartBlock(BlockKindInternal.Markup))
            {
                AttributePrefix(whitespace, name, whitespaceAfterAttributeName);
            }
        }

        private void AttributePrefix(
            IEnumerable<HtmlToken> whitespace,
            IEnumerable<HtmlToken> nameTokens,
            IEnumerable<HtmlToken> whitespaceAfterAttributeName)
        {
            // First, determine if this is a 'data-' attribute (since those can't use conditional attributes)
            var name = string.Concat(nameTokens.Select(s => s.Content));
            var attributeCanBeConditional = 
                Context.FeatureFlags.EXPERIMENTAL_AllowConditionalDataDashAttributes ||
                !name.StartsWith("data-", StringComparison.OrdinalIgnoreCase);

            // Accept the whitespace and name
            Accept(whitespace);
            Accept(nameTokens);

            // Since this is not a minimized attribute, the whitespace after attribute name belongs to this attribute.
            Accept(whitespaceAfterAttributeName);
            Assert(HtmlTokenType.Equals); // We should be at "="
            AcceptAndMoveNext();

            var whitespaceAfterEquals = ReadWhile(token => token.Type == HtmlTokenType.WhiteSpace || token.Type == HtmlTokenType.NewLine);
            var quote = HtmlTokenType.Unknown;
            if (At(HtmlTokenType.SingleQuote) || At(HtmlTokenType.DoubleQuote))
            {
                // Found a quote, the whitespace belongs to this attribute.
                Accept(whitespaceAfterEquals);
                quote = CurrentToken.Type;
                AcceptAndMoveNext();
            }
            else if (whitespaceAfterEquals.Any())
            {
                // No quotes found after the whitespace. Put it back so that it can be parsed later.
                PutCurrentBack();
                PutBack(whitespaceAfterEquals);
            }

            // We now have the prefix: (i.e. '      foo="')
            var prefix = new LocationTagged<string>(string.Concat(Span.Tokens.Select(s => s.Content)), Span.Start);

            if (attributeCanBeConditional)
            {
                Span.ChunkGenerator = SpanChunkGenerator.Null; // The block chunk generator will render the prefix
                Output(SpanKindInternal.Markup);

                // Read the attribute value only if the value is quoted
                // or if there is no whitespace between '=' and the unquoted value.
                if (quote != HtmlTokenType.Unknown || !whitespaceAfterEquals.Any())
                {
                    // Read the attribute value.
                    while (!EndOfFile && !IsEndOfAttributeValue(quote, CurrentToken))
                    {
                        AttributeValue(quote);
                    }
                }

                // Capture the suffix
                var suffix = new LocationTagged<string>(string.Empty, CurrentStart);
                if (quote != HtmlTokenType.Unknown && At(quote))
                {
                    suffix = new LocationTagged<string>(CurrentToken.Content, CurrentStart);
                    AcceptAndMoveNext();
                }

                if (Span.Tokens.Count > 0)
                {
                    // Again, block chunk generator will render the suffix
                    Span.ChunkGenerator = SpanChunkGenerator.Null;
                    Output(SpanKindInternal.Markup);
                }

                // Create the block chunk generator
                Context.Builder.CurrentBlock.ChunkGenerator = new AttributeBlockChunkGenerator(
                    name, prefix, suffix);
            }
            else
            {
                // Output the attribute name, the equals and optional quote. Ex: foo="
                Output(SpanKindInternal.Markup);

                if (quote == HtmlTokenType.Unknown && whitespaceAfterEquals.Any())
                {
                    return;
                }

                // Not a "conditional" attribute, so just read the value
                SkipToAndParseCode(token => IsEndOfAttributeValue(quote, token));

                // Output the attribute value (will include everything in-between the attribute's quotes).
                Output(SpanKindInternal.Markup);

                if (quote != HtmlTokenType.Unknown)
                {
                    Optional(quote);
                }
                Output(SpanKindInternal.Markup);
            }
        }

        private void AttributeValue(HtmlTokenType quote)
        {
            var prefixStart = CurrentStart;
            var prefix = ReadWhile(token => token.Type == HtmlTokenType.WhiteSpace || token.Type == HtmlTokenType.NewLine);

            if (At(HtmlTokenType.Transition))
            {
                if (NextIs(HtmlTokenType.Transition))
                {
                    // Wrapping this in a block so that the ConditionalAttributeCollapser doesn't rewrite it.
                    using (Context.Builder.StartBlock(BlockKindInternal.Markup))
                    {
                        Accept(prefix);

                        // Render a single "@" in place of "@@".
                        Span.ChunkGenerator = new LiteralAttributeChunkGenerator(
                            new LocationTagged<string>(string.Concat(prefix.Select(s => s.Content)), prefixStart),
                            new LocationTagged<string>(CurrentToken.Content, CurrentStart));
                        AcceptAndMoveNext();
                        Output(SpanKindInternal.Markup, AcceptedCharactersInternal.None);

                        Span.ChunkGenerator = SpanChunkGenerator.Null;
                        AcceptAndMoveNext();
                        Output(SpanKindInternal.Markup, AcceptedCharactersInternal.None);
                    }
                }
                else
                {
                    Accept(prefix);
                    var valueStart = CurrentStart;
                    PutCurrentBack();

                    // Output the prefix but as a null-span. DynamicAttributeBlockChunkGenerator will render it
                    Span.ChunkGenerator = SpanChunkGenerator.Null;

                    // Dynamic value, start a new block and set the chunk generator
                    using (Context.Builder.StartBlock(BlockKindInternal.Markup))
                    {
                        Context.Builder.CurrentBlock.ChunkGenerator =
                            new DynamicAttributeBlockChunkGenerator(
                                new LocationTagged<string>(string.Concat(prefix.Select(s => s.Content)), prefixStart),
                                valueStart);

                        OtherParserBlock();
                    }
                }
            }
            else
            {
                Accept(prefix);

                // Literal value
                // 'quote' should be "Unknown" if not quoted and tokens coming from the tokenizer should never have
                // "Unknown" type.
                var valueStart = CurrentStart;
                var value = ReadWhile(token =>
                    // These three conditions find separators which break the attribute value into portions
                    token.Type != HtmlTokenType.WhiteSpace &&
                    token.Type != HtmlTokenType.NewLine &&
                    token.Type != HtmlTokenType.Transition &&
                    // This condition checks for the end of the attribute value (it repeats some of the checks above
                    // but for now that's ok)
                    !IsEndOfAttributeValue(quote, token));
                Accept(value);
                Span.ChunkGenerator = new LiteralAttributeChunkGenerator(
                    new LocationTagged<string>(string.Concat(prefix.Select(s => s.Content)), prefixStart),
                    new LocationTagged<string>(string.Concat(value.Select(s => s.Content)), valueStart));
            }
            Output(SpanKindInternal.Markup);
        }

        private bool IsEndOfAttributeValue(HtmlTokenType quote, HtmlToken token)
        {
            return EndOfFile || token == null ||
                   (quote != HtmlTokenType.Unknown
                        ? token.Type == quote // If quoted, just wait for the quote
                        : IsUnquotedEndOfAttributeValue(token));
        }

        private bool IsUnquotedEndOfAttributeValue(HtmlToken token)
        {
            // If unquoted, we have a larger set of terminating characters:
            // http://dev.w3.org/html5/spec/tokenization.html#attribute-value-unquoted-state
            // Also we need to detect "/" and ">"
            return token.Type == HtmlTokenType.DoubleQuote ||
                   token.Type == HtmlTokenType.SingleQuote ||
                   token.Type == HtmlTokenType.OpenAngle ||
                   token.Type == HtmlTokenType.Equals ||
                   (token.Type == HtmlTokenType.ForwardSlash && NextIs(HtmlTokenType.CloseAngle)) ||
                   token.Type == HtmlTokenType.CloseAngle ||
                   token.Type == HtmlTokenType.WhiteSpace ||
                   token.Type == HtmlTokenType.NewLine;
        }

        private void RecoverToEndOfTag()
        {
            // Accept until ">", "/" or "<", but parse code
            while (!EndOfFile)
            {
                SkipToAndParseCode(IsTagRecoveryStopPoint);
                if (!EndOfFile)
                {
                    EnsureCurrent();
                    switch (CurrentToken.Type)
                    {
                        case HtmlTokenType.SingleQuote:
                        case HtmlTokenType.DoubleQuote:
                            ParseQuoted();
                            break;
                        case HtmlTokenType.OpenAngle:
                        // Another "<" means this tag is invalid.
                        case HtmlTokenType.ForwardSlash:
                        // Empty tag
                        case HtmlTokenType.CloseAngle:
                            // End of tag
                            return;
                        default:
                            AcceptAndMoveNext();
                            break;
                    }
                }
            }
        }

        private void ParseQuoted()
        {
            var type = CurrentToken.Type;
            AcceptAndMoveNext();
            ParseQuoted(type);
        }

        private void ParseQuoted(HtmlTokenType type)
        {
            SkipToAndParseCode(type);
            if (!EndOfFile)
            {
                Assert(type);
                AcceptAndMoveNext();
            }
        }

        private bool StartTag(Stack<Tuple<HtmlToken, SourceLocation>> tags, IDisposable tagBlockWrapper)
        {
            HtmlToken bangToken = null;
            HtmlToken potentialTagNameToken;

            if (At(HtmlTokenType.Bang))
            {
                bangToken = CurrentToken;

                potentialTagNameToken = Lookahead(count: 1);
            }
            else
            {
                potentialTagNameToken = CurrentToken;
            }

            HtmlToken tagName;

            if (potentialTagNameToken == null || potentialTagNameToken.Type != HtmlTokenType.Text)
            {
                tagName = new HtmlToken(string.Empty, HtmlTokenType.Unknown);
            }
            else if (bangToken != null)
            {
                tagName = new HtmlToken("!" + potentialTagNameToken.Content, HtmlTokenType.Text);
            }
            else
            {
                tagName = potentialTagNameToken;
            }

            Tuple<HtmlToken, SourceLocation> tag = Tuple.Create(tagName, _lastTagStart);

            if (tags.Count == 0 &&
                // Note tagName may contain a '!' escape character. This ensures <!text> doesn't match here.
                // <!text> tags are treated like any other escaped HTML start tag.
                string.Equals(tag.Item1.Content, SyntaxConstants.TextTagName, StringComparison.OrdinalIgnoreCase))
            {
                Output(SpanKindInternal.Markup);
                Span.ChunkGenerator = SpanChunkGenerator.Null;

                Accept(_bufferedOpenAngle);
                var textLocation = CurrentStart;
                Assert(HtmlTokenType.Text);

                AcceptAndMoveNext();

                var bookmark = CurrentStart.AbsoluteIndex;
                IEnumerable<HtmlToken> tokens = ReadWhile(IsSpacingToken(includeNewLines: true));
                var empty = At(HtmlTokenType.ForwardSlash);
                if (empty)
                {
                    Accept(tokens);
                    Assert(HtmlTokenType.ForwardSlash);
                    AcceptAndMoveNext();
                    bookmark = CurrentStart.AbsoluteIndex;
                    tokens = ReadWhile(IsSpacingToken(includeNewLines: true));
                }

                if (!Optional(HtmlTokenType.CloseAngle))
                {
                    Context.Source.Position = bookmark;
                    NextToken();
                    Context.ErrorSink.OnError(
                        RazorDiagnosticFactory.CreateParsing_TextTagCannotContainAttributes(
                            new SourceSpan(textLocation, contentLength: 4 /* text */)));

                    RecoverTextTag();
                }
                else
                {
                    Accept(tokens);
                    Span.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
                }

                if (!empty)
                {
                    tags.Push(tag);
                }

                CompleteTagBlockWithSpan(tagBlockWrapper, Span.EditHandler.AcceptedCharacters, SpanKindInternal.Transition);

                return true;
            }

            Accept(_bufferedOpenAngle);
            OptionalBangEscape();
            Optional(HtmlTokenType.Text);
            return RestOfTag(tag, tags, tagBlockWrapper);
        }

        private bool RestOfTag(Tuple<HtmlToken, SourceLocation> tag,
                               Stack<Tuple<HtmlToken, SourceLocation>> tags,
                               IDisposable tagBlockWrapper)
        {
            TagContent();

            // We are now at a possible end of the tag
            // Found '<', so we just abort this tag.
            if (At(HtmlTokenType.OpenAngle))
            {
                return false;
            }

            var isEmpty = At(HtmlTokenType.ForwardSlash);
            // Found a solidus, so don't accept it but DON'T push the tag to the stack
            if (isEmpty)
            {
                AcceptAndMoveNext();
            }

            // Check for the '>' to determine if the tag is finished
            var seenClose = Optional(HtmlTokenType.CloseAngle);
            if (!seenClose)
            {
                Context.ErrorSink.OnError(
                    RazorDiagnosticFactory.CreateParsing_UnfinishedTag(
                        new SourceSpan(
                            SourceLocationTracker.Advance(tag.Item2, "<"),
                            Math.Max(tag.Item1.Content.Length, 1)),
                        tag.Item1.Content));
            }
            else
            {
                if (!isEmpty)
                {
                    // Is this a void element?
                    var tagName = tag.Item1.Content.Trim();
                    if (VoidElements.Contains(tagName))
                    {
                        CompleteTagBlockWithSpan(tagBlockWrapper, AcceptedCharactersInternal.None, SpanKindInternal.Markup);

                        // Technically, void elements like "meta" are not allowed to have end tags. Just in case they do,
                        // we need to look ahead at the next set of tokens. If we see "<", "/", tag name, accept it and the ">" following it
                        // Place a bookmark
                        var bookmark = CurrentStart.AbsoluteIndex;

                        // Skip whitespace
                        IEnumerable<HtmlToken> whiteSpace = ReadWhile(IsSpacingToken(includeNewLines: true));

                        // Open Angle
                        if (At(HtmlTokenType.OpenAngle) && NextIs(HtmlTokenType.ForwardSlash))
                        {
                            var openAngle = CurrentToken;
                            NextToken();
                            Assert(HtmlTokenType.ForwardSlash);
                            var solidus = CurrentToken;
                            NextToken();
                            if (At(HtmlTokenType.Text) && string.Equals(CurrentToken.Content, tagName, StringComparison.OrdinalIgnoreCase))
                            {
                                // Accept up to here
                                Accept(whiteSpace);
                                Output(SpanKindInternal.Markup); // Output the whitespace

                                using (Context.Builder.StartBlock(BlockKindInternal.Tag))
                                {
                                    Accept(openAngle);
                                    Accept(solidus);
                                    AcceptAndMoveNext();

                                    // Accept to '>', '<' or EOF
                                    AcceptUntil(HtmlTokenType.CloseAngle, HtmlTokenType.OpenAngle);
                                    // Accept the '>' if we saw it. And if we do see it, we're complete
                                    var complete = Optional(HtmlTokenType.CloseAngle);

                                    if (complete)
                                    {
                                        Span.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
                                    }

                                    // Output the closing void element
                                    Output(SpanKindInternal.Markup);

                                    return complete;
                                }
                            }
                        }

                        // Go back to the bookmark and just finish this tag at the close angle
                        Context.Source.Position = bookmark;
                        NextToken();
                    }
                    else if (string.Equals(tagName, ScriptTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (!CurrentScriptTagExpectsHtml())
                        {
                            CompleteTagBlockWithSpan(tagBlockWrapper, AcceptedCharactersInternal.None, SpanKindInternal.Markup);

                            SkipToEndScriptAndParseCode(endTagAcceptedCharacters: AcceptedCharactersInternal.None);
                        }
                        else
                        {
                            // Push the script tag onto the tag stack, it should be treated like all other HTML tags.
                            tags.Push(tag);
                        }
                    }
                    else
                    {
                        // Push the tag on to the stack
                        tags.Push(tag);
                    }
                }
            }
            return seenClose;
        }

        private void SkipToEndScriptAndParseCode(AcceptedCharactersInternal endTagAcceptedCharacters = AcceptedCharactersInternal.Any)
        {
            // Special case for <script>: Skip to end of script tag and parse code
            var seenEndScript = false;

            while (!seenEndScript && !EndOfFile)
            {
                SkipToAndParseCode(HtmlTokenType.OpenAngle);
                var tagStart = CurrentStart;

                if (NextIs(HtmlTokenType.ForwardSlash))
                {
                    var openAngle = CurrentToken;
                    NextToken(); // Skip over '<', current is '/'
                    var solidus = CurrentToken;
                    NextToken(); // Skip over '/', current should be text

                    if (At(HtmlTokenType.Text) &&
                        string.Equals(CurrentToken.Content, ScriptTagName, StringComparison.OrdinalIgnoreCase))
                    {
                        seenEndScript = true;
                    }

                    // We put everything back because we just wanted to look ahead to see if the current end tag that we're parsing is
                    // the script tag.  If so we'll generate correct code to encompass it.
                    PutCurrentBack(); // Put back whatever was after the solidus
                    PutBack(solidus); // Put back '/'
                    PutBack(openAngle); // Put back '<'

                    // We just looked ahead, this NextToken will set CurrentToken to an open angle bracket.
                    NextToken();
                }

                if (seenEndScript)
                {
                    Output(SpanKindInternal.Markup);

                    using (Context.Builder.StartBlock(BlockKindInternal.Tag))
                    {
                        Span.EditHandler.AcceptedCharacters = endTagAcceptedCharacters;

                        AcceptAndMoveNext(); // '<'
                        AcceptAndMoveNext(); // '/'
                        SkipToAndParseCode(HtmlTokenType.CloseAngle);
                        if (!Optional(HtmlTokenType.CloseAngle))
                        {
                            Context.ErrorSink.OnError(
                                RazorDiagnosticFactory.CreateParsing_UnfinishedTag(
                                    new SourceSpan(SourceLocationTracker.Advance(tagStart, "</"), ScriptTagName.Length),
                                    ScriptTagName));
                        }
                        Output(SpanKindInternal.Markup);
                    }
                }
                else
                {
                    AcceptAndMoveNext(); // Accept '<' (not the closing script tag's open angle)
                }
            }
        }

        private void CompleteTagBlockWithSpan(IDisposable tagBlockWrapper,
                                              AcceptedCharactersInternal acceptedCharacters,
                                              SpanKindInternal spanKind)
        {
            Debug.Assert(tagBlockWrapper != null,
                "Tag block wrapper should not be null when attempting to complete a block");

            Span.EditHandler.AcceptedCharacters = acceptedCharacters;
            // Write out the current span into the block before closing it.
            Output(spanKind);
            // Finish the tag block
            tagBlockWrapper.Dispose();
        }

        private bool AcceptUntilAll(params HtmlTokenType[] endSequence)
        {
            while (!EndOfFile)
            {
                SkipToAndParseCode(endSequence[0]);
                if (AcceptAll(endSequence))
                {
                    return true;
                }
            }
            Debug.Assert(EndOfFile);
            Span.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.Any;
            return false;
        }

        private bool RemoveTag(Stack<Tuple<HtmlToken, SourceLocation>> tags, string tagName, SourceLocation tagStart)
        {
            Tuple<HtmlToken, SourceLocation> currentTag = null;
            while (tags.Count > 0)
            {
                currentTag = tags.Pop();
                if (string.Equals(tagName, currentTag.Item1.Content, StringComparison.OrdinalIgnoreCase))
                {
                    // Matched the tag
                    return true;
                }
            }
            if (currentTag != null)
            {
                Context.ErrorSink.OnError(
                    RazorDiagnosticFactory.CreateParsing_MissingEndTag(
                        new SourceSpan(
                            SourceLocationTracker.Advance(currentTag.Item2, "<"),
                            currentTag.Item1.Content.Length),
                        currentTag.Item1.Content));
            }
            else
            {
                Context.ErrorSink.OnError(
                    RazorDiagnosticFactory.CreateParsing_UnexpectedEndTag(
                        new SourceSpan(SourceLocationTracker.Advance(tagStart, "</"), tagName.Length), tagName));
            }
            return false;
        }

        private void EndTagBlock(Stack<Tuple<HtmlToken, SourceLocation>> tags, bool complete)
        {
            if (tags.Count > 0)
            {
                // Ended because of EOF, not matching close tag.  Throw error for last tag
                while (tags.Count > 1)
                {
                    tags.Pop();
                }
                var tag = tags.Pop();
                Context.ErrorSink.OnError(
                    RazorDiagnosticFactory.CreateParsing_MissingEndTag(
                        new SourceSpan(
                            SourceLocationTracker.Advance(tag.Item2, "<"),
                            tag.Item1.Content.Length),
                        tag.Item1.Content));
            }
            else if (complete)
            {
                Span.EditHandler.AcceptedCharacters = AcceptedCharactersInternal.None;
            }
            tags.Clear();
            if (!Context.DesignTimeMode)
            {
                var shouldAcceptWhitespaceAndNewLine = true;

                if (Context.Builder.LastSpan.Kind == SpanKindInternal.Transition)
                {
                    var tokens = ReadWhile(
                        f => (f.Type == HtmlTokenType.WhiteSpace) || (f.Type == HtmlTokenType.NewLine));

                    // Make sure the current token is not markup, which can be html start tag or @:
                    if (!(At(HtmlTokenType.OpenAngle) ||
                        (At(HtmlTokenType.Transition) && Lookahead(count: 1).Content.StartsWith(":"))))
                    {
                        // Don't accept whitespace as markup if the end text tag is followed by csharp.
                        shouldAcceptWhitespaceAndNewLine = false;
                    }

                    PutCurrentBack();
                    PutBack(tokens);
                    EnsureCurrent();
                }

                if (shouldAcceptWhitespaceAndNewLine)
                {
                    // Accept whitespace and a single newline if present
                    AcceptWhile(HtmlTokenType.WhiteSpace);
                    Optional(HtmlTokenType.NewLine);
                }
            }
            else if (Span.EditHandler.AcceptedCharacters == AcceptedCharactersInternal.Any)
            {
                AcceptWhile(HtmlTokenType.WhiteSpace);
                Optional(HtmlTokenType.NewLine);
            }
            PutCurrentBack();

            if (!complete)
            {
                AddMarkerTokenIfNecessary();
            }
            Output(SpanKindInternal.Markup);
        }

        internal static bool IsValidAttributeNameToken(HtmlToken token)
        {
            if (token == null)
            {
                return false;
            }

            // These restrictions cover most of the spec defined: http://www.w3.org/TR/html5/syntax.html#attributes-0
            // However, it's not all of it. For instance we don't special case control characters or allow OpenAngle.
            // It also doesn't try to exclude Razor specific features such as the @ transition. This is based on the
            // expectation that the parser handles such scenarios prior to falling through to name resolution.
            var tokenType = token.Type;
            return tokenType != HtmlTokenType.WhiteSpace &&
                tokenType != HtmlTokenType.NewLine &&
                tokenType != HtmlTokenType.CloseAngle &&
                tokenType != HtmlTokenType.OpenAngle &&
                tokenType != HtmlTokenType.ForwardSlash &&
                tokenType != HtmlTokenType.DoubleQuote &&
                tokenType != HtmlTokenType.SingleQuote &&
                tokenType != HtmlTokenType.Equals &&
                tokenType != HtmlTokenType.Unknown;
        }

        public void ParseDocument()
        {
            if (Context == null)
            {
                throw new InvalidOperationException(Resources.Parser_Context_Not_Set);
            }

            using (PushSpanConfig(DefaultMarkupSpan))
            {
                using (Context.Builder.StartBlock(BlockKindInternal.Markup))
                {
                    Span.Start = CurrentLocation;

                    NextToken();
                    while (!EndOfFile)
                    {
                        SkipToAndParseCode(HtmlTokenType.OpenAngle);
                        ScanTagInDocumentContext();
                    }
                    AddMarkerTokenIfNecessary();
                    Output(SpanKindInternal.Markup);
                }
            }
        }

        /// <summary>
        /// Reads the content of a tag (if present) in the MarkupDocument (or MarkupSection) context,
        /// where we don't care about maintaining a stack of tags.
        /// </summary>
        private void ScanTagInDocumentContext()
        {
            if (At(HtmlTokenType.OpenAngle))
            {
                if (NextIs(HtmlTokenType.Bang))
                {
                    // Checking to see if we meet the conditions of a special '!' tag: <!DOCTYPE, <![CDATA[, <!--.
                    if (!IsBangEscape(lookahead: 1))
                    {
                        if (Lookahead(2)?.Type == HtmlTokenType.DoubleHyphen)
                        {
                            Output(SpanKindInternal.Markup);
                        }

                        AcceptAndMoveNext(); // Accept '<'
                        BangTag();

                        return;
                    }

                    // We should behave like a normal tag that has a parser escape, fall through to the normal
                    // tag logic.
                }
                else if (NextIs(HtmlTokenType.QuestionMark))
                {
                    AcceptAndMoveNext(); // Accept '<'
                    XmlPI();
                    return;
                }

                Output(SpanKindInternal.Markup);

                // Start tag block
                var tagBlock = Context.Builder.StartBlock(BlockKindInternal.Tag);

                AcceptAndMoveNext(); // Accept '<'

                if (!At(HtmlTokenType.ForwardSlash))
                {
                    OptionalBangEscape();

                    // Parsing a start tag
                    var scriptTag = At(HtmlTokenType.Text) &&
                                    string.Equals(CurrentToken.Content, "script", StringComparison.OrdinalIgnoreCase);
                    Optional(HtmlTokenType.Text);
                    TagContent(); // Parse the tag, don't care about the content
                    Optional(HtmlTokenType.ForwardSlash);
                    Optional(HtmlTokenType.CloseAngle);

                    // If the script tag expects javascript content then we should do minimal parsing until we reach
                    // the end script tag. Don't want to incorrectly parse a "var tag = '<input />';" as an HTML tag.
                    if (scriptTag && !CurrentScriptTagExpectsHtml())
                    {
                        Output(SpanKindInternal.Markup);
                        tagBlock.Dispose();

                        SkipToEndScriptAndParseCode();
                        return;
                    }
                }
                else
                {
                    // Parsing an end tag
                    // This section can accept things like: '</p  >' or '</p>' etc.
                    Optional(HtmlTokenType.ForwardSlash);

                    // Whitespace here is invalid (according to the spec)
                    OptionalBangEscape();
                    Optional(HtmlTokenType.Text);
                    Optional(HtmlTokenType.WhiteSpace);
                    Optional(HtmlTokenType.CloseAngle);
                }

                Output(SpanKindInternal.Markup);

                // End tag block
                tagBlock.Dispose();
            }
        }

        private bool CurrentScriptTagExpectsHtml()
        {
            var blockBuilder = Context.Builder.CurrentBlock;

            Debug.Assert(blockBuilder != null);

            var typeAttribute = blockBuilder.Children
                .OfType<Block>()
                .Where(block =>
                    block.ChunkGenerator is AttributeBlockChunkGenerator &&
                    block.Children.Count() >= 2)
                .FirstOrDefault(IsTypeAttribute);

            if (typeAttribute != null)
            {
                var contentValues = typeAttribute.Children
                    .OfType<Span>()
                    .Where(childSpan => childSpan.ChunkGenerator is LiteralAttributeChunkGenerator)
                    .Select(childSpan => childSpan.Content);

                var scriptType = string.Concat(contentValues).Trim();

                // Does not allow charset parameter (or any other parameters).
                return string.Equals(scriptType, "text/html", StringComparison.OrdinalIgnoreCase);
            }

            return false;
        }

        private static bool IsTypeAttribute(Block block)
        {
            var span = block.Children.First() as Span;

            if (span == null)
            {
                return false;
            }

            var trimmedStartContent = span.Content.TrimStart();
            if (trimmedStartContent.StartsWith("type", StringComparison.OrdinalIgnoreCase) &&
                (trimmedStartContent.Length == 4 ||
                ValidAfterTypeAttributeNameCharacters.Contains(trimmedStartContent[4])))
            {
                return true;
            }

            return false;
        }

        public void ParseRazorBlock(Tuple<string, string> nestingSequences, bool caseSensitive)
        {
            if (Context == null)
            {
                throw new InvalidOperationException(Resources.Parser_Context_Not_Set);
            }

            using (PushSpanConfig(DefaultMarkupSpan))
            {
                Span.Start = CurrentLocation;

                using (Context.Builder.StartBlock(BlockKindInternal.Markup))
                {
                    NextToken();
                    CaseSensitive = caseSensitive;
                    if (nestingSequences.Item1 == null)
                    {
                        NonNestingSection(nestingSequences.Item2.Split());
                    }
                    else
                    {
                        NestingSection(nestingSequences);
                    }
                    AddMarkerTokenIfNecessary();
                    Output(SpanKindInternal.Markup);
                }
            }
        }

        private void NonNestingSection(string[] nestingSequenceComponents)
        {
            do
            {
                SkipToAndParseCode(token => token.Type == HtmlTokenType.OpenAngle || AtEnd(nestingSequenceComponents));
                ScanTagInDocumentContext();
                if (!EndOfFile && AtEnd(nestingSequenceComponents))
                {
                    break;
                }
            }
            while (!EndOfFile);

            PutCurrentBack();
        }

        private void NestingSection(Tuple<string, string> nestingSequences)
        {
            var nesting = 1;
            while (nesting > 0 && !EndOfFile)
            {
                SkipToAndParseCode(token =>
                    token.Type == HtmlTokenType.Text ||
                    token.Type == HtmlTokenType.OpenAngle);
                if (At(HtmlTokenType.Text))
                {
                    nesting += ProcessTextToken(nestingSequences, nesting);
                    if (CurrentToken != null)
                    {
                        AcceptAndMoveNext();
                    }
                    else if (nesting > 0)
                    {
                        NextToken();
                    }
                }
                else
                {
                    ScanTagInDocumentContext();
                }
            }
        }

        private bool AtEnd(string[] nestingSequenceComponents)
        {
            EnsureCurrent();
            if (string.Equals(CurrentToken.Content, nestingSequenceComponents[0], Comparison))
            {
                var bookmark = Context.Source.Position - CurrentToken.Content.Length;
                try
                {
                    foreach (string component in nestingSequenceComponents)
                    {
                        if (!EndOfFile && !string.Equals(CurrentToken.Content, component, Comparison))
                        {
                            return false;
                        }
                        NextToken();
                        while (!EndOfFile && IsSpacingToken(includeNewLines: true)(CurrentToken))
                        {
                            NextToken();
                        }
                    }
                    return true;
                }
                finally
                {
                    Context.Source.Position = bookmark;
                    NextToken();
                }
            }
            return false;
        }

        private int ProcessTextToken(Tuple<string, string> nestingSequences, int currentNesting)
        {
            for (int i = 0; i < CurrentToken.Content.Length; i++)
            {
                var nestingDelta = HandleNestingSequence(nestingSequences.Item1, i, currentNesting, 1);
                if (nestingDelta == 0)
                {
                    nestingDelta = HandleNestingSequence(nestingSequences.Item2, i, currentNesting, -1);
                }

                if (nestingDelta != 0)
                {
                    return nestingDelta;
                }
            }
            return 0;
        }

        private int HandleNestingSequence(string sequence, int position, int currentNesting, int retIfMatched)
        {
            if (sequence != null &&
                CurrentToken.Content[position] == sequence[0] &&
                position + sequence.Length <= CurrentToken.Content.Length)
            {
                var possibleStart = CurrentToken.Content.Substring(position, sequence.Length);
                if (string.Equals(possibleStart, sequence, Comparison))
                {
                    // Capture the current token and "put it back" (really we just want to clear CurrentToken)
                    var bookmark = CurrentStart;
                    var token = CurrentToken;
                    PutCurrentBack();

                    // Carve up the token
                    Tuple<HtmlToken, HtmlToken> pair = Language.SplitToken(token, position, HtmlTokenType.Text);
                    var preSequence = pair.Item1;
                    Debug.Assert(pair.Item2 != null);
                    pair = Language.SplitToken(pair.Item2, sequence.Length, HtmlTokenType.Text);
                    var sequenceToken = pair.Item1;
                    var postSequence = pair.Item2;
                    var postSequenceBookmark = bookmark.AbsoluteIndex + preSequence.Content.Length + pair.Item1.Content.Length;

                    // Accept the first chunk (up to the nesting sequence we just saw)
                    if (!string.IsNullOrEmpty(preSequence.Content))
                    {
                        Accept(preSequence);
                    }

                    if (currentNesting + retIfMatched == 0)
                    {
                        // This is 'popping' the final entry on the stack of nesting sequences
                        // A caller higher in the parsing stack will accept the sequence token, so advance
                        // to it
                        Context.Source.Position = bookmark.AbsoluteIndex + preSequence.Content.Length;
                    }
                    else
                    {
                        // This isn't the end of the last nesting sequence, accept the token and keep going
                        Accept(sequenceToken);

                        // Position at the start of the postSequence token, which might be null.
                        Context.Source.Position = postSequenceBookmark;
                    }

                    // Return the value we were asked to return if matched, since we found a nesting sequence
                    return retIfMatched;
                }
            }
            return 0;
        }
    }
}
