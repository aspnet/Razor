// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.AspNet.Razor.Chunks.Generators;
using Microsoft.AspNet.Razor.Parser.SyntaxTree;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;

namespace Microsoft.AspNet.Razor.Parser
{
    public partial class CSharpCodeParser
    {
        private static readonly Func<CSharpSymbol, bool> IsValidStatementSpacingSymbol =
            IsSpacingToken(includeNewLines: true, includeComments: true);

        private void SetUpKeywords()
        {
            MapKeywords(
                ConditionalBlock,
                CSharpKeyword.For,
                CSharpKeyword.Foreach,
                CSharpKeyword.While,
                CSharpKeyword.Switch,
                CSharpKeyword.Lock);
            MapKeywords(CaseStatement, false, CSharpKeyword.Case, CSharpKeyword.Default);
            MapKeywords(IfStatement, CSharpKeyword.If);
            MapKeywords(TryStatement, CSharpKeyword.Try);
            MapKeywords(UsingKeyword, CSharpKeyword.Using);
            MapKeywords(DoStatement, CSharpKeyword.Do);
            MapKeywords(ReservedDirective, CSharpKeyword.Namespace, CSharpKeyword.Class);
        }

        protected virtual void ReservedDirective(bool topLevel)
        {
            Context.OnError(
                CurrentLocation,
                RazorResources.FormatParseError_ReservedWord(CurrentSymbol.Content),
                CurrentSymbol.Content.Length);
            AcceptAndMoveNext();
            Span.EditHandler.AcceptedCharacters = AcceptedCharacters.None;
            Span.ChunkGenerator = SpanChunkGenerator.Null;
            Context.CurrentBlock.Type = BlockType.Directive;
            CompleteBlock();
            Output(SpanKind.MetaCode);
        }

        private void KeywordBlock(bool topLevel)
        {
            HandleKeyword(topLevel, () =>
            {
                Context.CurrentBlock.Type = BlockType.Expression;
                Context.CurrentBlock.ChunkGenerator = new ExpressionChunkGenerator();
                ImplicitExpression();
            });
        }

        private void CaseStatement(bool topLevel)
        {
            Assert(CSharpSymbolType.Keyword);
            Debug.Assert(CurrentSymbol.Keyword != null &&
                         (CurrentSymbol.Keyword.Value == CSharpKeyword.Case ||
                          CurrentSymbol.Keyword.Value == CSharpKeyword.Default));
            AcceptUntil(CSharpSymbolType.Colon);
            Optional(CSharpSymbolType.Colon);
        }

        private void DoStatement(bool topLevel)
        {
            Assert(CSharpKeyword.Do);
            UnconditionalBlock();
            WhileClause();
            if (topLevel)
            {
                CompleteBlock();
            }
        }

        private void WhileClause()
        {
            Span.EditHandler.AcceptedCharacters = AcceptedCharacters.Any;
            IEnumerable<CSharpSymbol> ws = SkipToNextImportantToken();

            if (At(CSharpKeyword.While))
            {
                Accept(ws);
                Assert(CSharpKeyword.While);
                AcceptAndMoveNext();
                AcceptWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
                if (AcceptCondition() && Optional(CSharpSymbolType.Semicolon))
                {
                    Span.EditHandler.AcceptedCharacters = AcceptedCharacters.None;
                }
            }
            else
            {
                PutCurrentBack();
                PutBack(ws);
            }
        }

        private void UsingKeyword(bool topLevel)
        {
            Assert(CSharpKeyword.Using);
            var block = new Block(CurrentSymbol);
            AcceptAndMoveNext();
            AcceptWhile(IsSpacingToken(includeNewLines: false, includeComments: true));

            if (At(CSharpSymbolType.LeftParenthesis))
            {
                // using ( ==> Using Statement
                UsingStatement(block);
            }
            else if (At(CSharpSymbolType.Identifier) || At(CSharpKeyword.Static))
            {
                // using Identifier ==> Using Declaration
                if (!topLevel)
                {
                    Context.OnError(
                        block.Start,
                        RazorResources.ParseError_NamespaceImportAndTypeAlias_Cannot_Exist_Within_CodeBlock,
                        block.Name.Length);
                    StandardStatement();
                }
                else
                {
                    UsingDeclaration();
                }
            }

            if (topLevel)
            {
                CompleteBlock();
            }
        }

        private void UsingDeclaration()
        {
            // Set block type to directive
            Context.CurrentBlock.Type = BlockType.Directive;

            if (At(CSharpSymbolType.Identifier))
            {
                // non-static using
                NamespaceOrTypeName();
                var whitespace = ReadWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
                if (At(CSharpSymbolType.Assign))
                {
                    // Alias
                    Accept(whitespace);
                    Assert(CSharpSymbolType.Assign);
                    AcceptAndMoveNext();

                    AcceptWhile(IsSpacingToken(includeNewLines: true, includeComments: true));

                    // One more namespace or type name
                    NamespaceOrTypeName();
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
                AcceptAndMoveNext();
                AcceptWhile(IsSpacingToken(includeNewLines: false, includeComments: true));
                NamespaceOrTypeName();
            }

            Span.EditHandler.AcceptedCharacters = AcceptedCharacters.AnyExceptNewline;
            Span.ChunkGenerator = new AddImportChunkGenerator(
                Span.GetContent(symbols => symbols.Skip(1)));

            // Optional ";"
            if (EnsureCurrent())
            {
                Optional(CSharpSymbolType.Semicolon);
            }
        }

        protected bool NamespaceOrTypeName()
        {
            if (Optional(CSharpSymbolType.Identifier) || Optional(CSharpSymbolType.Keyword))
            {
                Optional(CSharpSymbolType.QuestionMark); // Nullable
                if (Optional(CSharpSymbolType.DoubleColon))
                {
                    if (!Optional(CSharpSymbolType.Identifier))
                    {
                        Optional(CSharpSymbolType.Keyword);
                    }
                }
                if (At(CSharpSymbolType.LessThan))
                {
                    TypeArgumentList();
                }
                if (Optional(CSharpSymbolType.Dot))
                {
                    NamespaceOrTypeName();
                }
                while (At(CSharpSymbolType.LeftBracket))
                {
                    Balance(BalancingModes.None);
                    Optional(CSharpSymbolType.RightBracket);
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        private void TypeArgumentList()
        {
            Assert(CSharpSymbolType.LessThan);
            Balance(BalancingModes.None);
            Optional(CSharpSymbolType.GreaterThan);
        }

        private void UsingStatement(Block block)
        {
            Assert(CSharpSymbolType.LeftParenthesis);

            // Parse condition
            if (AcceptCondition())
            {
                AcceptWhile(IsSpacingToken(includeNewLines: true, includeComments: true));

                // Parse code block
                ExpectCodeBlock(block);
            }
        }

        private void TryStatement(bool topLevel)
        {
            Assert(CSharpKeyword.Try);
            UnconditionalBlock();
            AfterTryClause();
            if (topLevel)
            {
                CompleteBlock();
            }
        }

        private void IfStatement(bool topLevel)
        {
            Assert(CSharpKeyword.If);
            ConditionalBlock(topLevel: false);
            AfterIfClause();
            if (topLevel)
            {
                CompleteBlock();
            }
        }

        private void AfterTryClause()
        {
            // Grab whitespace
            var whitespace = SkipToNextImportantToken();

            // Check for a catch or finally part
            if (At(CSharpKeyword.Catch))
            {
                Accept(whitespace);
                Assert(CSharpKeyword.Catch);
                FilterableCatchBlock();
                AfterTryClause();
            }
            else if (At(CSharpKeyword.Finally))
            {
                Accept(whitespace);
                Assert(CSharpKeyword.Finally);
                UnconditionalBlock();
            }
            else
            {
                // Return whitespace and end the block
                PutCurrentBack();
                PutBack(whitespace);
                Span.EditHandler.AcceptedCharacters = AcceptedCharacters.Any;
            }
        }

        private void AfterIfClause()
        {
            // Grab whitespace and razor comments
            IEnumerable<CSharpSymbol> ws = SkipToNextImportantToken();

            // Check for an else part
            if (At(CSharpKeyword.Else))
            {
                Accept(ws);
                Assert(CSharpKeyword.Else);
                ElseClause();
            }
            else
            {
                // No else, return whitespace
                PutCurrentBack();
                PutBack(ws);
                Span.EditHandler.AcceptedCharacters = AcceptedCharacters.Any;
            }
        }

        private void ElseClause()
        {
            if (!At(CSharpKeyword.Else))
            {
                return;
            }
            var block = new Block(CurrentSymbol);

            AcceptAndMoveNext();
            AcceptWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
            if (At(CSharpKeyword.If))
            {
                // ElseIf
                block.Name = SyntaxConstants.CSharp.ElseIfKeyword;
                ConditionalBlock(block);
                AfterIfClause();
            }
            else if (!EndOfFile)
            {
                // Else
                ExpectCodeBlock(block);
            }
        }

        private void ExpectCodeBlock(Block block)
        {
            if (!EndOfFile)
            {
                // Check for "{" to make sure we're at a block
                if (!At(CSharpSymbolType.LeftBrace))
                {
                    Context.OnError(
                        CurrentLocation,
                        RazorResources.FormatParseError_SingleLine_ControlFlowStatements_Not_Allowed(
                            Language.GetSample(CSharpSymbolType.LeftBrace),
                            CurrentSymbol.Content),
                        CurrentSymbol.Content.Length);
                }

                // Parse the statement and then we're done
                Statement(block);
            }
        }

        private void UnconditionalBlock()
        {
            Assert(CSharpSymbolType.Keyword);
            var block = new Block(CurrentSymbol);
            AcceptAndMoveNext();
            AcceptWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
            ExpectCodeBlock(block);
        }

        private void FilterableCatchBlock()
        {
            Assert(CSharpKeyword.Catch);

            var block = new Block(CurrentSymbol);

            // Accept "catch"
            AcceptAndMoveNext();
            AcceptWhile(IsValidStatementSpacingSymbol);

            // Parse the catch condition if present. If not present, let the C# compiler complain.
            if (AcceptCondition())
            {
                AcceptWhile(IsValidStatementSpacingSymbol);

                if (At(CSharpKeyword.When))
                {
                    // Accept "when".
                    AcceptAndMoveNext();
                    AcceptWhile(IsValidStatementSpacingSymbol);

                    // Parse the filter condition if present. If not present, let the C# compiler complain.
                    if (!AcceptCondition())
                    {
                        // Incomplete condition.
                        return;
                    }

                    AcceptWhile(IsValidStatementSpacingSymbol);
                }

                ExpectCodeBlock(block);
            }
        }

        private void ConditionalBlock(bool topLevel)
        {
            Assert(CSharpSymbolType.Keyword);
            var block = new Block(CurrentSymbol);
            ConditionalBlock(block);
            if (topLevel)
            {
                CompleteBlock();
            }
        }

        private void ConditionalBlock(Block block)
        {
            AcceptAndMoveNext();
            AcceptWhile(IsSpacingToken(includeNewLines: true, includeComments: true));

            // Parse the condition, if present (if not present, we'll let the C# compiler complain)
            if (AcceptCondition())
            {
                AcceptWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
                ExpectCodeBlock(block);
            }
        }

        private bool AcceptCondition()
        {
            if (At(CSharpSymbolType.LeftParenthesis))
            {
                var complete = Balance(BalancingModes.BacktrackOnFailure | BalancingModes.AllowCommentsAndTemplates);
                if (!complete)
                {
                    AcceptUntil(CSharpSymbolType.NewLine);
                }
                else
                {
                    Optional(CSharpSymbolType.RightParenthesis);
                }
                return complete;
            }
            return true;
        }

        private void Statement()
        {
            Statement(null);
        }

        private void Statement(Block block)
        {
            Span.EditHandler.AcceptedCharacters = AcceptedCharacters.Any;

            // Accept whitespace but always keep the last whitespace node so we can put it back if necessary
            var lastWhitespace = AcceptWhiteSpaceInLines();
            Debug.Assert(lastWhitespace == null ||
                (lastWhitespace.Start.AbsoluteIndex + lastWhitespace.Content.Length == CurrentLocation.AbsoluteIndex));

            if (EndOfFile)
            {
                if (lastWhitespace != null)
                {
                    Accept(lastWhitespace);
                }
                return;
            }

            var type = CurrentSymbol.Type;
            var loc = CurrentLocation;

            var isSingleLineMarkup = type == CSharpSymbolType.Transition && NextIs(CSharpSymbolType.Colon);
            var isMarkup = isSingleLineMarkup ||
                type == CSharpSymbolType.LessThan ||
                (type == CSharpSymbolType.Transition && NextIs(CSharpSymbolType.LessThan));

            if (Context.DesignTimeMode || !isMarkup)
            {
                // CODE owns whitespace, MARKUP owns it ONLY in DesignTimeMode.
                if (lastWhitespace != null)
                {
                    Accept(lastWhitespace);
                }
            }
            else
            {
                var nextSymbol = Lookahead(1);

                // MARKUP owns whitespace EXCEPT in DesignTimeMode.
                PutCurrentBack();

                // Put back the whitespace unless it precedes a '<text>' tag.
                if (nextSymbol != null &&
                    !string.Equals(nextSymbol.Content, SyntaxConstants.TextTagName, StringComparison.Ordinal))
                {
                    PutBack(lastWhitespace);
                }
            }

            if (isMarkup)
            {
                if (type == CSharpSymbolType.Transition && !isSingleLineMarkup)
                {
                    Context.OnError(
                        loc,
                        RazorResources.ParseError_AtInCode_Must_Be_Followed_By_Colon_Paren_Or_Identifier_Start,
                        length: 1 /* @ */);
                }

                // Markup block
                Output(SpanKind.Code);
                if (Context.DesignTimeMode && CurrentSymbol != null &&
                    (CurrentSymbol.Type == CSharpSymbolType.LessThan || CurrentSymbol.Type == CSharpSymbolType.Transition))
                {
                    PutCurrentBack();
                }
                OtherParserBlock();
            }
            else
            {
                // What kind of statement is this?
                HandleStatement(block, type);
            }
        }

        private void HandleStatement(Block block, CSharpSymbolType type)
        {
            switch (type)
            {
                case CSharpSymbolType.RazorCommentTransition:
                    Output(SpanKind.Code);
                    RazorComment();
                    Statement(block);
                    break;
                case CSharpSymbolType.LeftBrace:
                    // Verbatim Block
                    block = block ?? new Block(RazorResources.BlockName_Code, CurrentLocation);
                    AcceptAndMoveNext();
                    CodeBlock(block);
                    break;
                case CSharpSymbolType.Keyword:
                    // Keyword block
                    HandleKeyword(false, StandardStatement);
                    break;
                case CSharpSymbolType.Transition:
                    // Embedded Expression block
                    EmbeddedExpression();
                    break;
                case CSharpSymbolType.RightBrace:
                    // Possible end of Code Block, just run the continuation
                    break;
                case CSharpSymbolType.Comment:
                    AcceptAndMoveNext();
                    break;
                default:
                    // Other statement
                    StandardStatement();
                    break;
            }
        }

        private void EmbeddedExpression()
        {
            // First, verify the type of the block
            Assert(CSharpSymbolType.Transition);
            var transition = CurrentSymbol;
            NextToken();

            if (At(CSharpSymbolType.Transition))
            {
                // Escaped "@"
                Output(SpanKind.Code);

                // Output "@" as hidden span
                Accept(transition);
                Span.ChunkGenerator = SpanChunkGenerator.Null;
                Output(SpanKind.Code);

                Assert(CSharpSymbolType.Transition);
                AcceptAndMoveNext();
                StandardStatement();
            }
            else
            {
                // Throw errors as necessary, but continue parsing
                if (At(CSharpSymbolType.LeftBrace))
                {
                    Context.OnError(
                        CurrentLocation,
                        RazorResources.ParseError_Unexpected_Nested_CodeBlock,
                        length: 1  /* { */);
                }

                // @( or @foo - Nested expression, parse a child block
                PutCurrentBack();
                PutBack(transition);

                // Before exiting, add a marker span if necessary
                AddMarkerSymbolIfNecessary();

                NestedBlock();
            }
        }

        private void StandardStatement()
        {
            while (!EndOfFile)
            {
                var bookmark = CurrentLocation.AbsoluteIndex;
                IEnumerable<CSharpSymbol> read = ReadWhile(sym => sym.Type != CSharpSymbolType.Semicolon &&
                                                                  sym.Type != CSharpSymbolType.RazorCommentTransition &&
                                                                  sym.Type != CSharpSymbolType.Transition &&
                                                                  sym.Type != CSharpSymbolType.LeftBrace &&
                                                                  sym.Type != CSharpSymbolType.LeftParenthesis &&
                                                                  sym.Type != CSharpSymbolType.LeftBracket &&
                                                                  sym.Type != CSharpSymbolType.RightBrace);
                if (At(CSharpSymbolType.LeftBrace) ||
                    At(CSharpSymbolType.LeftParenthesis) ||
                    At(CSharpSymbolType.LeftBracket))
                {
                    Accept(read);
                    if (Balance(BalancingModes.AllowCommentsAndTemplates | BalancingModes.BacktrackOnFailure))
                    {
                        Optional(CSharpSymbolType.RightBrace);
                    }
                    else
                    {
                        // Recovery
                        AcceptUntil(CSharpSymbolType.LessThan, CSharpSymbolType.RightBrace);
                        return;
                    }
                }
                else if (At(CSharpSymbolType.Transition) && (NextIs(CSharpSymbolType.LessThan, CSharpSymbolType.Colon)))
                {
                    Accept(read);
                    Output(SpanKind.Code);
                    Template();
                }
                else if (At(CSharpSymbolType.RazorCommentTransition))
                {
                    Accept(read);
                    RazorComment();
                }
                else if (At(CSharpSymbolType.Semicolon))
                {
                    Accept(read);
                    AcceptAndMoveNext();
                    return;
                }
                else if (At(CSharpSymbolType.RightBrace))
                {
                    Accept(read);
                    return;
                }
                else
                {
                    Context.Source.Position = bookmark;
                    NextToken();
                    AcceptUntil(CSharpSymbolType.LessThan, CSharpSymbolType.RightBrace);
                    return;
                }
            }
        }

        private void CodeBlock(Block block)
        {
            CodeBlock(true, block);
        }

        private void CodeBlock(bool acceptTerminatingBrace, Block block)
        {
            EnsureCurrent();
            while (!EndOfFile && !At(CSharpSymbolType.RightBrace))
            {
                // Parse a statement, then return here
                Statement();
                EnsureCurrent();
            }

            if (EndOfFile)
            {
                Context.OnError(
                    block.Start,
                    RazorResources.FormatParseError_Expected_EndOfBlock_Before_EOF(block.Name, '}', '{'),
                    length: 1  /* { OR } */);
            }
            else if (acceptTerminatingBrace)
            {
                Assert(CSharpSymbolType.RightBrace);
                Span.EditHandler.AcceptedCharacters = AcceptedCharacters.None;
                AcceptAndMoveNext();
            }
        }

        private void HandleKeyword(bool topLevel, Action fallback)
        {
            Debug.Assert(CurrentSymbol.Type == CSharpSymbolType.Keyword && CurrentSymbol.Keyword != null);
            Action<bool> handler;
            if (_keywordParsers.TryGetValue(CurrentSymbol.Keyword.Value, out handler))
            {
                handler(topLevel);
            }
            else
            {
                fallback();
            }
        }

        private IEnumerable<CSharpSymbol> SkipToNextImportantToken()
        {
            while (!EndOfFile)
            {
                IEnumerable<CSharpSymbol> ws = ReadWhile(IsSpacingToken(includeNewLines: true, includeComments: true));
                if (At(CSharpSymbolType.RazorCommentTransition))
                {
                    Accept(ws);
                    Span.EditHandler.AcceptedCharacters = AcceptedCharacters.Any;
                    RazorComment();
                }
                else
                {
                    return ws;
                }
            }
            return Enumerable.Empty<CSharpSymbol>();
        }

        // Common code for Parsers, but FxCop REALLY doesn't like it in the base class.. moving it here for now.
        protected override void OutputSpanBeforeRazorComment()
        {
            AddMarkerSymbolIfNecessary();
            Output(SpanKind.Code);
        }

        protected class Block
        {
            public Block(string name, SourceLocation start)
            {
                Name = name;
                Start = start;
            }

            public Block(CSharpSymbol symbol)
                : this(GetName(symbol), symbol.Start)
            {
            }

            public string Name { get; set; }
            public SourceLocation Start { get; set; }

            private static string GetName(CSharpSymbol sym)
            {
                if (sym.Type == CSharpSymbolType.Keyword)
                {
                    return CSharpLanguageCharacteristics.GetKeyword(sym.Keyword.Value);
                }
                return sym.Content;
            }
        }
    }
}
