// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNetCore.Razor.Language.Syntax.InternalSyntax;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal class TokenizerView<TTokenizer>
        where TTokenizer : Tokenizer
    {
        public TokenizerView(TTokenizer tokenizer)
        {
            Tokenizer = tokenizer;
        }

        public TTokenizer Tokenizer { get; private set; }
        public bool EndOfFile { get; private set; }
        public SyntaxToken Current { get; private set; }

        public ITextDocument Source
        {
            get { return Tokenizer.Source; }
        }

        public bool Next()
        {
            Current = Tokenizer.NextToken();
            EndOfFile = (Current == null);
            return !EndOfFile;
        }

        public void PutBack(SyntaxToken token)
        {
            Source.Position -= token.Content.Length;
            Current = null;
            EndOfFile = Source.Position >= Source.Length;
            Tokenizer.Reset();
        }
    }
}
