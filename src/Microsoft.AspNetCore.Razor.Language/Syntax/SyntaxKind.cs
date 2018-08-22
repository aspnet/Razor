// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language
{
    internal enum SyntaxKind : byte
    {
        #region Nodes
        // HTML
        HtmlTextLiteral,
        HtmlDocument,
        HtmlBlock,
        HtmlMarkupBlock,
        HtmlTagBlock,
        HtmlAttributeBlock,
        HtmlCommentBlock,
        HtmlMinimizedAttributeBlock,
        HtmlLiteralAttributeValue,
        HtmlDynamicAttributeValue,
        HtmlTransition,

        // CSharp
        CSharpStatement,
        CSharpStatementBody,
        CSharpExpression,
        CSharpExpressionBody,
        CSharpImplicitExpression,
        CSharpImplicitExpressionBody,
        CSharpDirective,
        CSharpDirectiveBody,
        CSharpCodeBlock,
        CSharpTemplateBlock,
        CSharpStatementLiteral,
        CSharpExpressionLiteral,
        CSharpHiddenLiteral,
        CSharpNoneLiteral,
        CSharpTransition,

        // Common
        RazorComment,
        RazorMetaCode,
        #endregion

        #region Tokens
        // Common
        Unknown,
        List,
        Whitespace,
        NewLine,
        Colon,
        QuestionMark,
        RightBracket,
        LeftBracket,
        Equals,
        Transition,

        // HTML
        Text,
        OpenAngle,
        Bang,
        ForwardSlash,
        DoubleHyphen,
        CloseAngle,
        DoubleQuote,
        SingleQuote,

        // CSharp literals
        Identifier,
        Keyword,
        IntegerLiteral,
        CSharpComment,
        RealLiteral,
        CharacterLiteral,
        StringLiteral,

        // CSharp operators
        Arrow,
        Minus,
        Decrement,
        MinusAssign,
        NotEqual,
        Not,
        Modulo,
        ModuloAssign,
        AndAssign,
        And,
        DoubleAnd,
        LeftParenthesis,
        RightParenthesis,
        Star,
        MultiplyAssign,
        Comma,
        Dot,
        Slash,
        DivideAssign,
        DoubleColon,
        Semicolon,
        NullCoalesce,
        XorAssign,
        Xor,
        LeftBrace,
        OrAssign,
        DoubleOr,
        Or,
        RightBrace,
        Tilde,
        Plus,
        PlusAssign,
        Increment,
        LessThan,
        LessThanEqual,
        LeftShift,
        LeftShiftAssign,
        Assign,
        GreaterThan,
        GreaterThanEqual,
        RightShift,
        RightShiftAssign,
        Hash,

        // Razor specific
        RazorCommentLiteral,
        RazorCommentStar,
        RazorCommentTransition,
        #endregion
    }
}
