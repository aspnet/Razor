// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;

namespace Microsoft.AspNetCore.Razor.Language.Syntax
{
    internal class PunctuationSyntax : SyntaxToken
    {
        internal PunctuationSyntax(GreenNode green, SyntaxNode parent, int position)
            : base(green, parent, position)
        {
        }

        internal new InternalSyntax.PunctuationSyntax Green => (InternalSyntax.PunctuationSyntax)base.Green;

        public string Punctuation => Text;

        internal override SyntaxToken WithLeadingTriviaCore(SyntaxNode trivia)
        {
            //return (PunctuationSyntax)new Green(Kind, Text, trivia?.Green, GetTrailingTrivia().Node?.GreenNode).CreateRed(Parent, Start);
            throw new NotImplementedException();
        }

        internal override SyntaxToken WithTrailingTriviaCore(SyntaxNode trivia)
        {
            //return (PunctuationSyntax)new Green(Kind, Text, GetLeadingTrivia().Node?.Green, trivia?.Green).CreateRed(Parent, Start);
            throw new NotImplementedException();
        }
    }
}
