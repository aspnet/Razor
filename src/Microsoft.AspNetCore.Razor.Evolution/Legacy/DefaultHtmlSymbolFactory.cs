// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    internal class DefaultHtmlSymbolFactory : IHtmlSymbolFactory
    {
        public HtmlSymbol Create(string content, HtmlSymbolType type, IReadOnlyList<RazorError> errors)
        {
            Debug.Assert(errors == null || errors.Count == 0);

            switch (type)
            {
                case HtmlSymbolType.OpenAngle:
                    return new OpenAngleHtmlSymbol();
                case HtmlSymbolType.CloseAngle:
                    return new CloseAngleHtmlSymbol();
                case HtmlSymbolType.ForwardSlash:
                    return new ForwardSlashHtmlSymbol();
                case HtmlSymbolType.DoubleQuote:
                    return new DoubleQuoteHtmlSymbol();
                default:
                    return new GeneralHtmlSymbol(content, type);
            }
        }

#region HTML Symbol derived classes

        // Perf: Optimize memory usage

        private class GeneralHtmlSymbol : HtmlSymbol
        {
            internal GeneralHtmlSymbol(string content, HtmlSymbolType type)
            {
                if (content == null)
                {
                    throw new ArgumentNullException(nameof(content));
                }

                Content = content;
                Type = type;
            }

            public override string Content { get; }

            public override HtmlSymbolType Type { get; }
        }

        private class OpenAngleHtmlSymbol : HtmlSymbol
        {
            public override string Content => "<";

            public override HtmlSymbolType Type => HtmlSymbolType.OpenAngle;
        }

        private class CloseAngleHtmlSymbol : HtmlSymbol
        {
            public override string Content => ">";

            public override HtmlSymbolType Type => HtmlSymbolType.CloseAngle;
        }

        private class ForwardSlashHtmlSymbol : HtmlSymbol
        {
            public override string Content => "/";

            public override HtmlSymbolType Type => HtmlSymbolType.ForwardSlash;
        }

        private class DoubleQuoteHtmlSymbol : HtmlSymbol
        {
            public override string Content => "\"";

            public override HtmlSymbolType Type => HtmlSymbolType.DoubleQuote;
        }

        #endregion
    }
}
