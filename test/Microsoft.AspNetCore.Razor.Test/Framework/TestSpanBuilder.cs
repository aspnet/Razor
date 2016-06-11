// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.Editor;
using Microsoft.AspNetCore.Razor.Parser;
using Microsoft.AspNetCore.Razor.Parser.SyntaxTree;
using Microsoft.AspNetCore.Razor.Text;
using Microsoft.AspNetCore.Razor.Tokenizer;
using Microsoft.AspNetCore.Razor.Tokenizer.Internal;
using Microsoft.AspNetCore.Razor.Tokenizer.Symbols;
using Microsoft.AspNetCore.Razor.Tokenizer.Symbols.Internal;

namespace Microsoft.AspNetCore.Razor.Test.Framework
{
    public static class SpanFactoryExtensions
    {
        public static UnclassifiedCodeSpanConstructor EmptyCSharp(this SpanFactory self)
        {
            return new UnclassifiedCodeSpanConstructor(
                self.Span(
                    SpanKind.Code,
                    new CSharpSymbol(self.LocationTracker.CurrentLocation, string.Empty, CSharpSymbolType.Unknown)));
        }

        public static SpanConstructor EmptyHtml(this SpanFactory self)
        {
            return self
                .Span(
                    SpanKind.Markup,
                    new HtmlSymbol(self.LocationTracker.CurrentLocation, string.Empty, HtmlSymbolType.Unknown))
                .With(new MarkupChunkGenerator());
        }

        public static UnclassifiedCodeSpanConstructor Code(this SpanFactory self, string content)
        {
            return new UnclassifiedCodeSpanConstructor(
                self.Span(SpanKind.Code, content, markup: false));
        }

        public static SpanConstructor CodeTransition(this SpanFactory self)
        {
            return self
                .Span(SpanKind.Transition, SyntaxConstants.TransitionString, markup: false)
                .Accepts(AcceptedCharacters.None);
        }

        public static SpanConstructor CodeTransition(this SpanFactory self, string content)
        {
            return self.Span(SpanKind.Transition, content, markup: false).Accepts(AcceptedCharacters.None);
        }

        public static SpanConstructor CodeTransition(this SpanFactory self, CSharpSymbolType type)
        {
            return self
                .Span(SpanKind.Transition, SyntaxConstants.TransitionString, type)
                .Accepts(AcceptedCharacters.None);
        }

        public static SpanConstructor CodeTransition(this SpanFactory self, string content, CSharpSymbolType type)
        {
            return self.Span(SpanKind.Transition, content, type).Accepts(AcceptedCharacters.None);
        }

        public static SpanConstructor MarkupTransition(this SpanFactory self)
        {
            return self
                .Span(SpanKind.Transition, SyntaxConstants.TransitionString, markup: true)
                .Accepts(AcceptedCharacters.None);
        }

        public static SpanConstructor MarkupTransition(this SpanFactory self, string content)
        {
            return self.Span(SpanKind.Transition, content, markup: true).Accepts(AcceptedCharacters.None);
        }

        public static SpanConstructor MarkupTransition(this SpanFactory self, HtmlSymbolType type)
        {
            return self
                .Span(SpanKind.Transition, SyntaxConstants.TransitionString, type)
                .Accepts(AcceptedCharacters.None);
        }

        public static SpanConstructor MarkupTransition(this SpanFactory self, string content, HtmlSymbolType type)
        {
            return self.Span(SpanKind.Transition, content, type).Accepts(AcceptedCharacters.None);
        }

        public static SpanConstructor MetaCode(this SpanFactory self, string content)
        {
            return self.Span(SpanKind.MetaCode, content, markup: false);
        }

        public static SpanConstructor MetaCode(this SpanFactory self, string content, CSharpSymbolType type)
        {
            return self.Span(SpanKind.MetaCode, content, type);
        }

        public static SpanConstructor MetaMarkup(this SpanFactory self, string content)
        {
            return self.Span(SpanKind.MetaCode, content, markup: true);
        }

        public static SpanConstructor MetaMarkup(this SpanFactory self, string content, HtmlSymbolType type)
        {
            return self.Span(SpanKind.MetaCode, content, type);
        }

        public static SpanConstructor Comment(this SpanFactory self, string content, CSharpSymbolType type)
        {
            return self.Span(SpanKind.Comment, content, type);
        }

        public static SpanConstructor Comment(this SpanFactory self, string content, HtmlSymbolType type)
        {
            return self.Span(SpanKind.Comment, content, type);
        }

        public static SpanConstructor BangEscape(this SpanFactory self)
        {
            return self
                .Span(SpanKind.MetaCode, "!", markup: true)
                .With(SpanChunkGenerator.Null)
                .Accepts(AcceptedCharacters.None);
        }

        public static SpanConstructor Markup(this SpanFactory self, string content)
        {
            return self.Span(SpanKind.Markup, content, markup: true).With(new MarkupChunkGenerator());
        }

        public static SpanConstructor Markup(this SpanFactory self, params string[] content)
        {
            return self.Span(SpanKind.Markup, content, markup: true).With(new MarkupChunkGenerator());
        }

        public static SpanConstructor CodeMarkup(this SpanFactory self, params string[] content)
        {
            return self.Span(SpanKind.Code, content, markup: true).With(new MarkupChunkGenerator());
        }

        public static SourceLocation GetLocationAndAdvance(this SourceLocationTracker self, string content)
        {
            var ret = self.CurrentLocation;
            self.UpdateLocation(content);
            return ret;
        }
    }

    public class SpanFactory
    {
        public Func<ITextDocument, ITokenizer> MarkupTokenizerFactory { get; set; }
        public Func<ITextDocument, ITokenizer> CodeTokenizerFactory { get; set; }
        public SourceLocationTracker LocationTracker { get; private set; }

        public static SpanFactory CreateCsHtml()
        {
            return new SpanFactory()
            {
                MarkupTokenizerFactory = doc => new HtmlTokenizer(doc),
                CodeTokenizerFactory = doc => new CSharpTokenizer(doc)
            };
        }

        public SpanFactory()
        {
            LocationTracker = new SourceLocationTracker();
        }

        public SpanConstructor Span(SpanKind kind, string content, CSharpSymbolType type)
        {
            return CreateSymbolSpan(kind, content, st => new CSharpSymbol(st, content, type));
        }

        public SpanConstructor Span(SpanKind kind, string content, HtmlSymbolType type)
        {
            return CreateSymbolSpan(kind, content, st => new HtmlSymbol(st, content, type));
        }

        public SpanConstructor Span(SpanKind kind, string content, bool markup)
        {
            return new SpanConstructor(kind, Tokenize(new[] { content }, markup));
        }

        public SpanConstructor Span(SpanKind kind, string[] content, bool markup)
        {
            return new SpanConstructor(kind, Tokenize(content, markup));
        }

        public SpanConstructor Span(SpanKind kind, params ISymbol[] symbols)
        {
            return new SpanConstructor(kind, symbols);
        }

        private SpanConstructor CreateSymbolSpan(SpanKind kind, string content, Func<SourceLocation, ISymbol> ctor)
        {
            var start = LocationTracker.CurrentLocation;
            LocationTracker.UpdateLocation(content);
            return new SpanConstructor(kind, new[] { ctor(start) });
        }

        public void Reset()
        {
            LocationTracker.CurrentLocation = SourceLocation.Zero;
        }

        private IEnumerable<ISymbol> Tokenize(IEnumerable<string> contentFragments, bool markup)
        {
            return contentFragments.SelectMany(fragment => Tokenize(fragment, markup));
        }

        private IEnumerable<ISymbol> Tokenize(string content, bool markup)
        {
            var tok = MakeTokenizer(markup, new SeekableTextReader(content));
            ISymbol sym;
            ISymbol last = null;
            while ((sym = tok.NextSymbol()) != null)
            {
                OffsetStart(sym, LocationTracker.CurrentLocation);
                last = sym;
                yield return sym;
            }
            LocationTracker.UpdateLocation(content);
        }

        private ITokenizer MakeTokenizer(bool markup, SeekableTextReader seekableTextReader)
        {
            if (markup)
            {
                return MarkupTokenizerFactory(seekableTextReader);
            }
            else
            {
                return CodeTokenizerFactory(seekableTextReader);
            }
        }

        private void OffsetStart(ISymbol sym, SourceLocation sourceLocation)
        {
            sym.OffsetStart(sourceLocation);
        }
    }

    public static class SpanConstructorExtensions
    {
        public static SpanConstructor Accepts(this SpanConstructor self, AcceptedCharacters accepted)
        {
            return self.With(eh => eh.AcceptedCharacters = accepted);
        }

        public static SpanConstructor AutoCompleteWith(this SpanConstructor self, string autoCompleteString)
        {
            return AutoCompleteWith(self, autoCompleteString, atEndOfSpan: false);
        }

        public static SpanConstructor AutoCompleteWith(
            this SpanConstructor self,
            string autoCompleteString,
            bool atEndOfSpan)
        {
            return self.With(new AutoCompleteEditHandler(
                SpanConstructor.TestTokenizer,
                autoCompleteAtEndOfSpan: atEndOfSpan)
            {
                AutoCompleteString = autoCompleteString
            });
        }

        public static SpanConstructor WithEditorHints(this SpanConstructor self, EditorHints hints)
        {
            return self.With(eh => eh.EditorHints = hints);
        }
    }

    public class UnclassifiedCodeSpanConstructor
    {
        SpanConstructor _self;

        public UnclassifiedCodeSpanConstructor(SpanConstructor self)
        {
            _self = self;
        }

        public SpanConstructor AsMetaCode()
        {
            _self.Builder.Kind = SpanKind.MetaCode;
            return _self;
        }

        public SpanConstructor AsStatement()
        {
            return _self.With(new StatementChunkGenerator());
        }

        public SpanConstructor AsExpression()
        {
            return _self.With(new ExpressionChunkGenerator());
        }

        public SpanConstructor AsImplicitExpression(ISet<string> keywords)
        {
            return AsImplicitExpression(keywords, acceptTrailingDot: false);
        }

        public SpanConstructor AsImplicitExpression(ISet<string> keywords, bool acceptTrailingDot)
        {
            return _self
                .With(new ImplicitExpressionEditHandler(SpanConstructor.TestTokenizer, keywords, acceptTrailingDot))
                .With(new ExpressionChunkGenerator());
        }

        public SpanConstructor AsFunctionsBody()
        {
            return _self.With(new TypeMemberChunkGenerator());
        }

        public SpanConstructor AsNamespaceImport(string ns)
        {
            return _self.With(new AddImportChunkGenerator(ns));
        }

        public SpanConstructor Hidden()
        {
            return _self.With(SpanChunkGenerator.Null);
        }

        public SpanConstructor AsBaseType(string baseType)
        {
            return _self
                .With(new SetBaseTypeChunkGenerator(baseType))
                .Accepts(AcceptedCharacters.AnyExceptNewline);
        }

        public SpanConstructor AsAddTagHelper(string lookupText)
        {
            return _self
                .With(new AddTagHelperChunkGenerator(lookupText))
                .Accepts(AcceptedCharacters.AnyExceptNewline);
        }

        public SpanConstructor AsRemoveTagHelper(string lookupText)
        {
            return _self
                .With(new RemoveTagHelperChunkGenerator(lookupText))
                .Accepts(AcceptedCharacters.AnyExceptNewline);
        }

        public SpanConstructor AsTagHelperPrefixDirective(string prefix)
        {
            return _self
                .With(new TagHelperPrefixDirectiveChunkGenerator(prefix))
                .Accepts(AcceptedCharacters.AnyExceptNewline);
        }

        public SpanConstructor As(ISpanChunkGenerator chunkGenerator)
        {
            return _self.With(chunkGenerator);
        }
    }

    public class SpanConstructor
    {
        public SpanBuilder Builder { get; private set; }

        internal static IEnumerable<ISymbol> TestTokenizer(string str)
        {
            yield return new RawTextSymbol(SourceLocation.Zero, str);
        }

        public SpanConstructor(SpanKind kind, IEnumerable<ISymbol> symbols)
        {
            Builder = new SpanBuilder();
            Builder.Kind = kind;
            Builder.EditHandler = SpanEditHandler.CreateDefault(TestTokenizer);
            foreach (ISymbol sym in symbols)
            {
                Builder.Accept(sym);
            }
        }

        private Span Build()
        {
            return Builder.Build();
        }

        public SpanConstructor As(SpanKind spanKind)
        {
            Builder.Kind = spanKind;
            return this;
        }

        public SpanConstructor With(ISpanChunkGenerator generator)
        {
            Builder.ChunkGenerator = generator;
            return this;
        }

        public SpanConstructor With(SpanEditHandler handler)
        {
            Builder.EditHandler = handler;
            return this;
        }

        public SpanConstructor With(Action<ISpanChunkGenerator> generatorConfigurer)
        {
            generatorConfigurer(Builder.ChunkGenerator);
            return this;
        }

        public SpanConstructor With(Action<SpanEditHandler> handlerConfigurer)
        {
            handlerConfigurer(Builder.EditHandler);
            return this;
        }

        public static implicit operator Span(SpanConstructor self)
        {
            return self.Build();
        }

        public SpanConstructor Hidden()
        {
            Builder.ChunkGenerator = SpanChunkGenerator.Null;
            return this;
        }
    }
}
