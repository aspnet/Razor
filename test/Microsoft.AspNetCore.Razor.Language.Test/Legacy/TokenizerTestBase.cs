// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.Text;
using Microsoft.AspNetCore.Razor.Language.Syntax.InternalSyntax;
using Xunit;

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    public abstract class TokenizerTestBase
    {
        internal abstract object IgnoreRemaining { get; }
        internal abstract object CreateTokenizer(ITextDocument source);

        internal void TestTokenizer(string input, params SyntaxToken[] expectedSymbols)
        {
            // Arrange
            var success = true;
            var output = new StringBuilder();
            using (var source = new SeekableTextReader(input, filePath: null))
            {
                var tokenizer = (Tokenizer)CreateTokenizer(source);
                var counter = 0;
                SyntaxToken current = null;
                while ((current = tokenizer.NextToken()) != null)
                {
                    if (counter >= expectedSymbols.Length)
                    {
                        output.AppendLine(string.Format("F: Expected: << Nothing >>; Actual: {0}", current));
                        success = false;
                    }
                    else if (ReferenceEquals(expectedSymbols[counter], IgnoreRemaining))
                    {
                        output.AppendLine(string.Format("P: Ignored |{0}|", current));
                    }
                    else
                    {
                        if (!expectedSymbols[counter].IsEquivalentTo(current))
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
