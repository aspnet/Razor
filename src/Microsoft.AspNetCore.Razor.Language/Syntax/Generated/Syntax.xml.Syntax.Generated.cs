﻿// <auto-generated />

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;


namespace Microsoft.AspNetCore.Razor.Language.Syntax
{
  internal abstract partial class RazorSyntaxNode : SyntaxNode
  {
    internal RazorSyntaxNode(GreenNode green, SyntaxNode parent, int position)
      : base(green, parent, position)
    {
    }
  }

  internal sealed partial class RazorCommentBlockSyntax : RazorSyntaxNode
  {
    private SyntaxToken _startCommentTransition;
    private SyntaxToken _startCommentStar;
    private SyntaxToken _comment;
    private SyntaxToken _endCommentStar;
    private SyntaxToken _endCommentTransition;

    internal RazorCommentBlockSyntax(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public SyntaxToken StartCommentTransition 
    {
        get
        {
            return GetRedAtZero(ref _startCommentTransition);
        }
    }

    public SyntaxToken StartCommentStar 
    {
        get
        {
            return GetRed(ref _startCommentStar, 1);
        }
    }

    public SyntaxToken Comment 
    {
        get
        {
            return GetRed(ref _comment, 2);
        }
    }

    public SyntaxToken EndCommentStar 
    {
        get
        {
            return GetRed(ref _endCommentStar, 3);
        }
    }

    public SyntaxToken EndCommentTransition 
    {
        get
        {
            return GetRed(ref _endCommentTransition, 4);
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _startCommentTransition);
            case 1: return GetRed(ref _startCommentStar, 1);
            case 2: return GetRed(ref _comment, 2);
            case 3: return GetRed(ref _endCommentStar, 3);
            case 4: return GetRed(ref _endCommentTransition, 4);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _startCommentTransition;
            case 1: return _startCommentStar;
            case 2: return _comment;
            case 3: return _endCommentStar;
            case 4: return _endCommentTransition;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitRazorCommentBlock(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitRazorCommentBlock(this);
    }

    public RazorCommentBlockSyntax Update(SyntaxToken startCommentTransition, SyntaxToken startCommentStar, SyntaxToken comment, SyntaxToken endCommentStar, SyntaxToken endCommentTransition)
    {
        if (startCommentTransition != StartCommentTransition || startCommentStar != StartCommentStar || comment != Comment || endCommentStar != EndCommentStar || endCommentTransition != EndCommentTransition)
        {
            var newNode = SyntaxFactory.RazorCommentBlock(startCommentTransition, startCommentStar, comment, endCommentStar, endCommentTransition);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    public RazorCommentBlockSyntax WithStartCommentTransition(SyntaxToken startCommentTransition)
    {
        return Update(startCommentTransition, _startCommentStar, _comment, _endCommentStar, _endCommentTransition);
    }

    public RazorCommentBlockSyntax WithStartCommentStar(SyntaxToken startCommentStar)
    {
        return Update(_startCommentTransition, startCommentStar, _comment, _endCommentStar, _endCommentTransition);
    }

    public RazorCommentBlockSyntax WithComment(SyntaxToken comment)
    {
        return Update(_startCommentTransition, _startCommentStar, comment, _endCommentStar, _endCommentTransition);
    }

    public RazorCommentBlockSyntax WithEndCommentStar(SyntaxToken endCommentStar)
    {
        return Update(_startCommentTransition, _startCommentStar, _comment, endCommentStar, _endCommentTransition);
    }

    public RazorCommentBlockSyntax WithEndCommentTransition(SyntaxToken endCommentTransition)
    {
        return Update(_startCommentTransition, _startCommentStar, _comment, _endCommentStar, endCommentTransition);
    }
  }

  internal abstract partial class HtmlSyntaxNode : RazorSyntaxNode
  {
    internal HtmlSyntaxNode(GreenNode green, SyntaxNode parent, int position)
      : base(green, parent, position)
    {
    }
  }

  internal sealed partial class HtmlTextSyntax : HtmlSyntaxNode
  {
    private SyntaxNode _textTokens;

    internal HtmlTextSyntax(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public SyntaxList<SyntaxToken> TextTokens 
    {
        get
        {
            return new SyntaxList<SyntaxToken>(GetRed(ref _textTokens, 0));
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _textTokens);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _textTokens;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitHtmlText(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitHtmlText(this);
    }

    public HtmlTextSyntax Update(SyntaxList<SyntaxToken> textTokens)
    {
        if (textTokens != TextTokens)
        {
            var newNode = SyntaxFactory.HtmlText(textTokens);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    public HtmlTextSyntax WithTextTokens(SyntaxList<SyntaxToken> textTokens)
    {
        return Update(textTokens);
    }

    public HtmlTextSyntax AddTextTokens(params SyntaxToken[] items)
    {
        return WithTextTokens(this.TextTokens.AddRange(items));
    }
  }

  internal abstract partial class CSharpSyntaxNode : RazorSyntaxNode
  {
    internal CSharpSyntaxNode(GreenNode green, SyntaxNode parent, int position)
      : base(green, parent, position)
    {
    }
  }

  internal sealed partial class CSharpTransitionSyntax : CSharpSyntaxNode
  {
    private SyntaxToken _transition;

    internal CSharpTransitionSyntax(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public SyntaxToken Transition 
    {
        get
        {
            return GetRedAtZero(ref _transition);
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _transition);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _transition;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCSharpTransition(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitCSharpTransition(this);
    }

    public CSharpTransitionSyntax Update(SyntaxToken transition)
    {
        if (transition != Transition)
        {
            var newNode = SyntaxFactory.CSharpTransition(transition);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    public CSharpTransitionSyntax WithTransition(SyntaxToken transition)
    {
        return Update(transition);
    }
  }

  internal sealed partial class CSharpMetaCodeSyntax : CSharpSyntaxNode
  {
    private SyntaxNode _metaCode;

    internal CSharpMetaCodeSyntax(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public SyntaxList<SyntaxToken> MetaCode 
    {
        get
        {
            return new SyntaxList<SyntaxToken>(GetRed(ref _metaCode, 0));
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _metaCode);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _metaCode;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCSharpMetaCode(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitCSharpMetaCode(this);
    }

    public CSharpMetaCodeSyntax Update(SyntaxList<SyntaxToken> metaCode)
    {
        if (metaCode != MetaCode)
        {
            var newNode = SyntaxFactory.CSharpMetaCode(metaCode);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    public CSharpMetaCodeSyntax WithMetaCode(SyntaxList<SyntaxToken> metaCode)
    {
        return Update(metaCode);
    }

    public CSharpMetaCodeSyntax AddMetaCode(params SyntaxToken[] items)
    {
        return WithMetaCode(this.MetaCode.AddRange(items));
    }
  }

  internal sealed partial class CSharpCodeLiteralSyntax : CSharpSyntaxNode
  {
    private SyntaxNode _cSharpTokens;

    internal CSharpCodeLiteralSyntax(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public SyntaxList<SyntaxToken> CSharpTokens 
    {
        get
        {
            return new SyntaxList<SyntaxToken>(GetRed(ref _cSharpTokens, 0));
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _cSharpTokens);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _cSharpTokens;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCSharpCodeLiteral(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitCSharpCodeLiteral(this);
    }

    public CSharpCodeLiteralSyntax Update(SyntaxList<SyntaxToken> cSharpTokens)
    {
        if (cSharpTokens != CSharpTokens)
        {
            var newNode = SyntaxFactory.CSharpCodeLiteral(cSharpTokens);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    public CSharpCodeLiteralSyntax WithCSharpTokens(SyntaxList<SyntaxToken> cSharpTokens)
    {
        return Update(cSharpTokens);
    }

    public CSharpCodeLiteralSyntax AddCSharpTokens(params SyntaxToken[] items)
    {
        return WithCSharpTokens(this.CSharpTokens.AddRange(items));
    }
  }

  internal sealed partial class CSharpCodeBlockSyntax : CSharpSyntaxNode
  {
    private SyntaxNode _children;

    internal CSharpCodeBlockSyntax(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public SyntaxList<RazorSyntaxNode> Children 
    {
        get
        {
            return new SyntaxList<RazorSyntaxNode>(GetRed(ref _children, 0));
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _children);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _children;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCSharpCodeBlock(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitCSharpCodeBlock(this);
    }

    public CSharpCodeBlockSyntax Update(SyntaxList<RazorSyntaxNode> children)
    {
        if (children != Children)
        {
            var newNode = SyntaxFactory.CSharpCodeBlock(children);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    public CSharpCodeBlockSyntax WithChildren(SyntaxList<RazorSyntaxNode> children)
    {
        return Update(children);
    }

    public CSharpCodeBlockSyntax AddChildren(params RazorSyntaxNode[] items)
    {
        return WithChildren(this.Children.AddRange(items));
    }
  }

  internal abstract partial class CSharpBlockSyntax : CSharpSyntaxNode
  {
    internal CSharpBlockSyntax(GreenNode green, SyntaxNode parent, int position)
      : base(green, parent, position)
    {
    }

    public abstract CSharpTransitionSyntax Transition { get; }
    public CSharpBlockSyntax WithTransition(CSharpTransitionSyntax _transition) => WithTransitionCore(_transition);
    internal abstract CSharpBlockSyntax WithTransitionCore(CSharpTransitionSyntax _transition);

    public abstract CSharpSyntaxNode Body { get; }
    public CSharpBlockSyntax WithBody(CSharpSyntaxNode _body) => WithBodyCore(_body);
    internal abstract CSharpBlockSyntax WithBodyCore(CSharpSyntaxNode _body);
  }

  internal sealed partial class CSharpStatement : CSharpBlockSyntax
  {
    private CSharpTransitionSyntax _transition;
    private CSharpSyntaxNode _body;

    internal CSharpStatement(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public override CSharpTransitionSyntax Transition 
    {
        get
        {
            return GetRedAtZero(ref _transition);
        }
    }

    public override CSharpSyntaxNode Body 
    {
        get
        {
            return GetRed(ref _body, 1);
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _transition);
            case 1: return GetRed(ref _body, 1);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _transition;
            case 1: return _body;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCSharpStatement(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitCSharpStatement(this);
    }

    public CSharpStatement Update(CSharpTransitionSyntax transition, CSharpSyntaxNode body)
    {
        if (transition != Transition || body != Body)
        {
            var newNode = SyntaxFactory.CSharpStatement(transition, body);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    internal override CSharpBlockSyntax WithTransitionCore(CSharpTransitionSyntax transition) => WithTransition(transition);
    public new CSharpStatement WithTransition(CSharpTransitionSyntax transition)
    {
        return Update(transition, _body);
    }

    internal override CSharpBlockSyntax WithBodyCore(CSharpSyntaxNode body) => WithBody(body);
    public new CSharpStatement WithBody(CSharpSyntaxNode body)
    {
        return Update(_transition, body);
    }
  }

  internal sealed partial class CSharpStatementBodySyntax : CSharpSyntaxNode
  {
    private CSharpMetaCodeSyntax _openBrace;
    private CSharpCodeBlockSyntax _cSharpCode;
    private CSharpMetaCodeSyntax _closeBrace;

    internal CSharpStatementBodySyntax(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public CSharpMetaCodeSyntax OpenBrace 
    {
        get
        {
            return GetRedAtZero(ref _openBrace);
        }
    }

    public CSharpCodeBlockSyntax CSharpCode 
    {
        get
        {
            return GetRed(ref _cSharpCode, 1);
        }
    }

    public CSharpMetaCodeSyntax CloseBrace 
    {
        get
        {
            return GetRed(ref _closeBrace, 2);
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _openBrace);
            case 1: return GetRed(ref _cSharpCode, 1);
            case 2: return GetRed(ref _closeBrace, 2);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _openBrace;
            case 1: return _cSharpCode;
            case 2: return _closeBrace;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCSharpStatementBody(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitCSharpStatementBody(this);
    }

    public CSharpStatementBodySyntax Update(CSharpMetaCodeSyntax openBrace, CSharpCodeBlockSyntax cSharpCode, CSharpMetaCodeSyntax closeBrace)
    {
        if (openBrace != OpenBrace || cSharpCode != CSharpCode || closeBrace != CloseBrace)
        {
            var newNode = SyntaxFactory.CSharpStatementBody(openBrace, cSharpCode, closeBrace);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    public CSharpStatementBodySyntax WithOpenBrace(CSharpMetaCodeSyntax openBrace)
    {
        return Update(openBrace, _cSharpCode, _closeBrace);
    }

    public CSharpStatementBodySyntax WithCSharpCode(CSharpCodeBlockSyntax cSharpCode)
    {
        return Update(_openBrace, cSharpCode, _closeBrace);
    }

    public CSharpStatementBodySyntax WithCloseBrace(CSharpMetaCodeSyntax closeBrace)
    {
        return Update(_openBrace, _cSharpCode, closeBrace);
    }

    public CSharpStatementBodySyntax AddOpenBraceMetaCode(params SyntaxToken[] items)
    {
        return this.WithOpenBrace(this.OpenBrace.WithMetaCode(this.OpenBrace.MetaCode.AddRange(items)));
    }

    public CSharpStatementBodySyntax AddCSharpCodeChildren(params RazorSyntaxNode[] items)
    {
        return this.WithCSharpCode(this.CSharpCode.WithChildren(this.CSharpCode.Children.AddRange(items)));
    }

    public CSharpStatementBodySyntax AddCloseBraceMetaCode(params SyntaxToken[] items)
    {
        return this.WithCloseBrace(this.CloseBrace.WithMetaCode(this.CloseBrace.MetaCode.AddRange(items)));
    }
  }

  internal sealed partial class CSharpExpression : CSharpBlockSyntax
  {
    private CSharpTransitionSyntax _transition;
    private CSharpSyntaxNode _body;

    internal CSharpExpression(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public override CSharpTransitionSyntax Transition 
    {
        get
        {
            return GetRedAtZero(ref _transition);
        }
    }

    public override CSharpSyntaxNode Body 
    {
        get
        {
            return GetRed(ref _body, 1);
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _transition);
            case 1: return GetRed(ref _body, 1);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _transition;
            case 1: return _body;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCSharpExpression(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitCSharpExpression(this);
    }

    public CSharpExpression Update(CSharpTransitionSyntax transition, CSharpSyntaxNode body)
    {
        if (transition != Transition || body != Body)
        {
            var newNode = SyntaxFactory.CSharpExpression(transition, body);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    internal override CSharpBlockSyntax WithTransitionCore(CSharpTransitionSyntax transition) => WithTransition(transition);
    public new CSharpExpression WithTransition(CSharpTransitionSyntax transition)
    {
        return Update(transition, _body);
    }

    internal override CSharpBlockSyntax WithBodyCore(CSharpSyntaxNode body) => WithBody(body);
    public new CSharpExpression WithBody(CSharpSyntaxNode body)
    {
        return Update(_transition, body);
    }
  }

  internal sealed partial class CSharpExpressionBodySyntax : CSharpSyntaxNode
  {
    private CSharpMetaCodeSyntax _openParen;
    private CSharpCodeBlockSyntax _cSharpCode;
    private CSharpMetaCodeSyntax _closeParen;

    internal CSharpExpressionBodySyntax(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public CSharpMetaCodeSyntax OpenParen 
    {
        get
        {
            return GetRedAtZero(ref _openParen);
        }
    }

    public CSharpCodeBlockSyntax CSharpCode 
    {
        get
        {
            return GetRed(ref _cSharpCode, 1);
        }
    }

    public CSharpMetaCodeSyntax CloseParen 
    {
        get
        {
            return GetRed(ref _closeParen, 2);
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _openParen);
            case 1: return GetRed(ref _cSharpCode, 1);
            case 2: return GetRed(ref _closeParen, 2);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _openParen;
            case 1: return _cSharpCode;
            case 2: return _closeParen;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCSharpExpressionBody(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitCSharpExpressionBody(this);
    }

    public CSharpExpressionBodySyntax Update(CSharpMetaCodeSyntax openParen, CSharpCodeBlockSyntax cSharpCode, CSharpMetaCodeSyntax closeParen)
    {
        if (openParen != OpenParen || cSharpCode != CSharpCode || closeParen != CloseParen)
        {
            var newNode = SyntaxFactory.CSharpExpressionBody(openParen, cSharpCode, closeParen);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    public CSharpExpressionBodySyntax WithOpenParen(CSharpMetaCodeSyntax openParen)
    {
        return Update(openParen, _cSharpCode, _closeParen);
    }

    public CSharpExpressionBodySyntax WithCSharpCode(CSharpCodeBlockSyntax cSharpCode)
    {
        return Update(_openParen, cSharpCode, _closeParen);
    }

    public CSharpExpressionBodySyntax WithCloseParen(CSharpMetaCodeSyntax closeParen)
    {
        return Update(_openParen, _cSharpCode, closeParen);
    }

    public CSharpExpressionBodySyntax AddOpenParenMetaCode(params SyntaxToken[] items)
    {
        var _openParen = this.OpenParen ?? SyntaxFactory.CSharpMetaCode();
        return this.WithOpenParen(_openParen.WithMetaCode(_openParen.MetaCode.AddRange(items)));
    }

    public CSharpExpressionBodySyntax AddCSharpCodeChildren(params RazorSyntaxNode[] items)
    {
        return this.WithCSharpCode(this.CSharpCode.WithChildren(this.CSharpCode.Children.AddRange(items)));
    }

    public CSharpExpressionBodySyntax AddCloseParenMetaCode(params SyntaxToken[] items)
    {
        var _closeParen = this.CloseParen ?? SyntaxFactory.CSharpMetaCode();
        return this.WithCloseParen(_closeParen.WithMetaCode(_closeParen.MetaCode.AddRange(items)));
    }
  }

  internal sealed partial class CSharpDirectiveSyntax : CSharpBlockSyntax
  {
    private CSharpTransitionSyntax _transition;
    private CSharpSyntaxNode _body;

    internal CSharpDirectiveSyntax(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public override CSharpTransitionSyntax Transition 
    {
        get
        {
            return GetRedAtZero(ref _transition);
        }
    }

    public override CSharpSyntaxNode Body 
    {
        get
        {
            return GetRed(ref _body, 1);
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _transition);
            case 1: return GetRed(ref _body, 1);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _transition;
            case 1: return _body;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCSharpDirective(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitCSharpDirective(this);
    }

    public CSharpDirectiveSyntax Update(CSharpTransitionSyntax transition, CSharpSyntaxNode body)
    {
        if (transition != Transition || body != Body)
        {
            var newNode = SyntaxFactory.CSharpDirective(transition, body);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    internal override CSharpBlockSyntax WithTransitionCore(CSharpTransitionSyntax transition) => WithTransition(transition);
    public new CSharpDirectiveSyntax WithTransition(CSharpTransitionSyntax transition)
    {
        return Update(transition, _body);
    }

    internal override CSharpBlockSyntax WithBodyCore(CSharpSyntaxNode body) => WithBody(body);
    public new CSharpDirectiveSyntax WithBody(CSharpSyntaxNode body)
    {
        return Update(_transition, body);
    }
  }

  internal sealed partial class CSharpDirectiveBodySyntax : CSharpSyntaxNode
  {
    private CSharpMetaCodeSyntax _keyword;
    private CSharpCodeBlockSyntax _cSharpCode;

    internal CSharpDirectiveBodySyntax(GreenNode green, SyntaxNode parent, int position)
        : base(green, parent, position)
    {
    }

    public CSharpMetaCodeSyntax Keyword 
    {
        get
        {
            return GetRedAtZero(ref _keyword);
        }
    }

    public CSharpCodeBlockSyntax CSharpCode 
    {
        get
        {
            return GetRed(ref _cSharpCode, 1);
        }
    }

    internal override SyntaxNode GetNodeSlot(int index)
    {
        switch (index)
        {
            case 0: return GetRedAtZero(ref _keyword);
            case 1: return GetRed(ref _cSharpCode, 1);
            default: return null;
        }
    }
    internal override SyntaxNode GetCachedSlot(int index)
    {
        switch (index)
        {
            case 0: return _keyword;
            case 1: return _cSharpCode;
            default: return null;
        }
    }

    public override TResult Accept<TResult>(SyntaxVisitor<TResult> visitor)
    {
        return visitor.VisitCSharpDirectiveBody(this);
    }

    public override void Accept(SyntaxVisitor visitor)
    {
        visitor.VisitCSharpDirectiveBody(this);
    }

    public CSharpDirectiveBodySyntax Update(CSharpMetaCodeSyntax keyword, CSharpCodeBlockSyntax cSharpCode)
    {
        if (keyword != Keyword || cSharpCode != CSharpCode)
        {
            var newNode = SyntaxFactory.CSharpDirectiveBody(keyword, cSharpCode);
            var annotations = GetAnnotations();
            if (annotations != null && annotations.Length > 0)
               return newNode.WithAnnotations(annotations);
            return newNode;
        }

        return this;
    }

    public CSharpDirectiveBodySyntax WithKeyword(CSharpMetaCodeSyntax keyword)
    {
        return Update(keyword, _cSharpCode);
    }

    public CSharpDirectiveBodySyntax WithCSharpCode(CSharpCodeBlockSyntax cSharpCode)
    {
        return Update(_keyword, cSharpCode);
    }

    public CSharpDirectiveBodySyntax AddKeywordMetaCode(params SyntaxToken[] items)
    {
        return this.WithKeyword(this.Keyword.WithMetaCode(this.Keyword.MetaCode.AddRange(items)));
    }

    public CSharpDirectiveBodySyntax AddCSharpCodeChildren(params RazorSyntaxNode[] items)
    {
        return this.WithCSharpCode(this.CSharpCode.WithChildren(this.CSharpCode.Children.AddRange(items)));
    }
  }
}
