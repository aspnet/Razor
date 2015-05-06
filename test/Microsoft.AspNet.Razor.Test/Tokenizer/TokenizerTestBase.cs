// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using Microsoft.AspNet.Razor.Text;
using Microsoft.AspNet.Razor.Tokenizer;
using Microsoft.AspNet.Razor.Tokenizer.Symbols;
using Xunit;

namespace Microsoft.AspNet.Razor.Test.Tokenizer
{
    public abstract class TokenizerTestBase<TSymbol, TSymbolType>
        where TSymbolType : struct
        where TSymbol : SymbolBase<TSymbolType>
    {
        protected abstract TSymbol IgnoreRemaining { get; }
        protected abstract Tokenizer<TSymbol, TSymbolType> CreateTokenizer(ITextDocument source);

        protected void TestTokenizer(string input, params TSymbol[] expectedSymbols)
        {
            // Arrange
            var success = true;
            var output = new StringBuilder();
            using (StringReader reader = new StringReader(input))
            {
                using (SeekableTextReader source = new SeekableTextReader(reader))
                {
                    Tokenizer<TSymbol, TSymbolType> tokenizer = CreateTokenizer(source);
                    var counter = 0;
                    TSymbol current = null;
                    while ((current = tokenizer.NextSymbol()) != null)
                    {
                        if (counter >= expectedSymbols.Length)
                        {
                            output.AppendLine(string.Format("F: Expected: << Nothing >>; Actual: {0}", current));
                            success = false;
                        }
                        else if (ReferenceEquals(expectedSymbols[counter], IgnoreRemaining))
                        {
                            output.AppendLine(string.Format("P: Ignored {0}", current));
                        }
                        else
                        {
                            if (!Equals(expectedSymbols[counter], current))
                            {
                                output.AppendLine(string.Format("F: Expected: {0}; Actual: {1}", expectedSymbols[counter], current));
                                success = false;
                            }
                            else
                            {
                                output.AppendLine(string.Format("P: Expected: {0}", expectedSymbols[counter]));
                            }
                            counter++;
                        }
                    }
                    if (counter < expectedSymbols.Length && !ReferenceEquals(expectedSymbols[counter], IgnoreRemaining))
                    {
                        success = false;
                        for (; counter < expectedSymbols.Length; counter++)
                        {
                            output.AppendLine(string.Format("F: Expected: {0}; Actual: << None >>", expectedSymbols[counter]));
                        }
                    }
                }
            }
            Assert.True(success, Environment.NewLine + output.ToString());
            WriteTraceLine(output.Replace("{", "{{").Replace("}", "}}").ToString());
        }

        [Conditional("PARSER_TRACE")]
        private static void WriteTraceLine(string format, params object[] args)
        {
            Trace.WriteLine(string.Format(format, args));
        }
    }
}
