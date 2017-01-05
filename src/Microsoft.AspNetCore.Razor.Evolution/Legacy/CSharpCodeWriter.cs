// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Linq;

namespace Microsoft.AspNetCore.Razor.Evolution.Legacy
{
    internal class CSharpCodeWriter : CodeWriter
    {
        private const string InstanceMethodFormat = "{0}.{1}";

        private static readonly char[] CStyleStringLiteralEscapeChars = {
            '\r',
            '\t',
            '\"',
            '\'',
            '\\',
            '\0',
            '\n',
            '\u2028',
            '\u2029',
        };

        public new CSharpCodeWriter Write(string data)
        {
            return (CSharpCodeWriter)base.Write(data);
        }

        public new CSharpCodeWriter Indent(int size)
        {
            return (CSharpCodeWriter)base.Indent(size);
        }

        public new CSharpCodeWriter ResetIndent()
        {
            return (CSharpCodeWriter)base.ResetIndent();
        }

        public new CSharpCodeWriter SetIndent(int size)
        {
            return (CSharpCodeWriter)base.SetIndent(size);
        }

        public new CSharpCodeWriter IncreaseIndent(int size)
        {
            return (CSharpCodeWriter)base.IncreaseIndent(size);
        }

        public new CSharpCodeWriter DecreaseIndent(int size)
        {
            return (CSharpCodeWriter)base.DecreaseIndent(size);
        }

        public new CSharpCodeWriter WriteLine(string data)
        {
            return (CSharpCodeWriter)base.WriteLine(data);
        }

        public new CSharpCodeWriter WriteLine()
        {
            return (CSharpCodeWriter)base.WriteLine();
        }

        public CSharpCodeWriter WriteVariableDeclaration(string type, string name, string value)
        {
            Write(type).Write(" ").Write(name);
            if (!string.IsNullOrEmpty(value))
            {
                Write(" = ").Write(value);
            }
            else
            {
                Write(" = null");
            }

            WriteLine(";");

            return this;
        }

        public CSharpCodeWriter WriteComment(string comment)
        {
            return Write("// ").WriteLine(comment);
        }

        public CSharpCodeWriter WriteBooleanLiteral(bool value)
        {
            return Write(value.ToString().ToLowerInvariant());
        }

        public CSharpCodeWriter WriteStartAssignment(string name)
        {
            return Write(name).Write(" = ");
        }

        public CSharpCodeWriter WriteParameterSeparator()
        {
            return Write(", ");
        }

        public CSharpCodeWriter WriteStartNewObject(string typeName)
        {
            return Write("new ").Write(typeName).Write("(");
        }

        public CSharpCodeWriter WriteLocationTaggedString(LocationTagged<string> value)
        {
            WriteStringLiteral(value.Value);
            WriteParameterSeparator();
            Write(value.Location.AbsoluteIndex.ToString(CultureInfo.InvariantCulture));

            return this;
        }

        public CSharpCodeWriter WriteStringLiteral(string literal)
        {
            if (literal.Length >= 256 && literal.Length <= 1500 && literal.IndexOf('\0') == -1)
            {
                WriteVerbatimStringLiteral(literal);
            }
            else
            {
                WriteCStyleStringLiteral(literal);
            }

            return this;
        }

        public CSharpCodeWriter WriteLineHiddenDirective()
        {
            return WriteLine("#line hidden");
        }

        public CSharpCodeWriter WritePragma(string value)
        {
            return Write("#pragma ").WriteLine(value);
        }

        public CSharpCodeWriter WriteUsing(string name)
        {
            return WriteUsing(name, endLine: true);
        }

        public CSharpCodeWriter WriteUsing(string name, bool endLine)
        {
            Write("using ");
            Write(name);

            if (endLine)
            {
                WriteLine(";");
            }

            return this;
        }

        public CSharpCodeWriter WriteLineDefaultDirective()
        {
            return WriteLine("#line default");
        }

        public CSharpCodeWriter WriteStartReturn()
        {
            return Write("return ");
        }

        public CSharpCodeWriter WriteReturn(string value)
        {
            return WriteReturn(value, endLine: true);
        }

        public CSharpCodeWriter WriteReturn(string value, bool endLine)
        {
            Write("return ").Write(value);

            if (endLine)
            {
                Write(";");
            }

            return WriteLine();
        }

        /// <summary>
        /// Writes a <c>#line</c> pragma directive for the line number at the specified <paramref name="location"/>.
        /// </summary>
        /// <param name="location">The location to generate the line pragma for.</param>
        /// <param name="file">The file to generate the line pragma for.</param>
        /// <returns>The current instance of <see cref="CSharpCodeWriter"/>.</returns>
        public CSharpCodeWriter WriteLineNumberDirective(SourceSpan location, string file)
        {
            if (location.FilePath != null)
            {
                file = location.FilePath;
            }

            if (Builder.Length >= NewLine.Length && !IsAfterNewLine)
            {
                WriteLine();
            }

            var lineNumberAsString = (location.LineIndex + 1).ToString(CultureInfo.InvariantCulture);
            return Write("#line ").Write(lineNumberAsString).Write(" \"").Write(file).WriteLine("\"");
        }

        public CSharpCodeWriter WriteStartMethodInvocation(string methodName)
        {
            return WriteStartMethodInvocation(methodName, new string[0]);
        }

        public CSharpCodeWriter WriteStartMethodInvocation(string methodName, params string[] genericArguments)
        {
            Write(methodName);

            if (genericArguments.Length > 0)
            {
                Write("<").Write(string.Join(", ", genericArguments)).Write(">");
            }

            return Write("(");
        }

        public CSharpCodeWriter WriteEndMethodInvocation()
        {
            return WriteEndMethodInvocation(endLine: true);
        }

        public CSharpCodeWriter WriteEndMethodInvocation(bool endLine)
        {
            Write(")");
            if (endLine)
            {
                WriteLine(";");
            }

            return this;
        }

        // Writes a method invocation for the given instance name.
        public CSharpCodeWriter WriteInstanceMethodInvocation(string instanceName,
                                                              string methodName,
                                                              params string[] parameters)
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException(nameof(instanceName));
            }

            if (methodName == null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            return WriteInstanceMethodInvocation(instanceName, methodName, endLine: true, parameters: parameters);
        }

        // Writes a method invocation for the given instance name.
        public CSharpCodeWriter WriteInstanceMethodInvocation(string instanceName,
                                                              string methodName,
                                                              bool endLine,
                                                              params string[] parameters)
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException(nameof(instanceName));
            }

            if (methodName == null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            return WriteMethodInvocation(
                string.Format(CultureInfo.InvariantCulture, InstanceMethodFormat, instanceName, methodName),
                endLine,
                parameters);
        }

        public CSharpCodeWriter WriteStartInstanceMethodInvocation(string instanceName,
                                                                   string methodName)
        {
            if (instanceName == null)
            {
                throw new ArgumentNullException(nameof(instanceName));
            }

            if (methodName == null)
            {
                throw new ArgumentNullException(nameof(methodName));
            }

            return WriteStartMethodInvocation(
                string.Format(CultureInfo.InvariantCulture, InstanceMethodFormat, instanceName, methodName));
        }

        public CSharpCodeWriter WriteMethodInvocation(string methodName, params string[] parameters)
        {
            return WriteMethodInvocation(methodName, endLine: true, parameters: parameters);
        }

        public CSharpCodeWriter WriteMethodInvocation(string methodName, bool endLine, params string[] parameters)
        {
            return WriteStartMethodInvocation(methodName)
                .Write(string.Join(", ", parameters))
                .WriteEndMethodInvocation(endLine);
        }

        public CSharpCodeWriter WriteAutoPropertyDeclaration(string accessibility, string typeName, string name)
        {
            return Write(accessibility)
                .Write(" ")
                .Write(typeName)
                .Write(" ")
                .Write(name)
                .Write(" { get; set; }")
                .WriteLine();
        }

        public CSharpDisableWarningScope BuildDisableWarningScope(int warning)
        {
            return new CSharpDisableWarningScope(this, warning);
        }

        public CSharpCodeWritingScope BuildScope()
        {
            return new CSharpCodeWritingScope(this);
        }

        public CSharpCodeWritingScope BuildLambda(bool endLine, params string[] parameterNames)
        {
            return BuildLambda(endLine, async: false, parameterNames: parameterNames);
        }

        public CSharpCodeWritingScope BuildAsyncLambda(bool endLine, params string[] parameterNames)
        {
            return BuildLambda(endLine, async: true, parameterNames: parameterNames);
        }

        private CSharpCodeWritingScope BuildLambda(bool endLine, bool async, string[] parameterNames)
        {
            if (async)
            {
                Write("async");
            }

            Write("(").Write(string.Join(", ", parameterNames)).Write(") => ");

            var scope = new CSharpCodeWritingScope(this);

            if (endLine)
            {
                // End the lambda with a semicolon
                scope.OnClose += () =>
                {
                    WriteLine(";");
                };
            }

            return scope;
        }

        public CSharpCodeWritingScope BuildNamespace(string name)
        {
            Write("namespace ").WriteLine(name);

            return new CSharpCodeWritingScope(this);
        }

        public CSharpCodeWritingScope BuildClassDeclaration(string accessibility, string name)
        {
            return BuildClassDeclaration(accessibility, name, Enumerable.Empty<string>());
        }

        public CSharpCodeWritingScope BuildClassDeclaration(string accessibility, string name, string baseType)
        {
            return BuildClassDeclaration(accessibility, name, new string[] { baseType });
        }

        public CSharpCodeWritingScope BuildClassDeclaration(
            string accessibility,
            string name,
            IEnumerable<string> baseTypes)
        {
            Write(accessibility).Write(" class ").Write(name);

            if (baseTypes.Count() > 0)
            {
                Write(" : ");
                Write(string.Join(", ", baseTypes));
            }

            WriteLine();

            return new CSharpCodeWritingScope(this);
        }

        public CSharpCodeWritingScope BuildConstructor(string name)
        {
            return BuildConstructor("public", name);
        }

        public CSharpCodeWritingScope BuildConstructor(string accessibility, string name)
        {
            return BuildConstructor(accessibility, name, Enumerable.Empty<KeyValuePair<string, string>>());
        }

        public CSharpCodeWritingScope BuildConstructor(
            string accessibility,
            string name,
            IEnumerable<KeyValuePair<string, string>> parameters)
        {
            Write(accessibility)
                .Write(" ")
                .Write(name)
                .Write("(")
                .Write(string.Join(", ", parameters.Select(p => p.Key + " " + p.Value)))
                .WriteLine(")");

            return new CSharpCodeWritingScope(this);
        }

        public CSharpCodeWritingScope BuildMethodDeclaration(string accessibility, string returnType, string name)
        {
            return BuildMethodDeclaration(accessibility, returnType, name, Enumerable.Empty<KeyValuePair<string, string>>());
        }

        public CSharpCodeWritingScope BuildMethodDeclaration(
            string accessibility,
            string returnType,
            string name,
            IEnumerable<KeyValuePair<string, string>> parameters)
        {
            Write(accessibility)
                .Write(" ")
                .Write(returnType)
                .Write(" ")
                .Write(name)
                .Write("(")
                .Write(string.Join(", ", parameters.Select(p => p.Key + " " + p.Value)))
                .WriteLine(")");

            return new CSharpCodeWritingScope(this);
        }

        private void WriteVerbatimStringLiteral(string literal)
        {
            Write("@\"");

            // We need to find the index of each '"' (double-quote) to escape it.
            var start = 0;
            int end;
            while ((end = literal.IndexOf('\"', start)) > -1)
            {
                Write(literal, start, end - start);

                Write("\"\"");

                start = end + 1;
            }

            Debug.Assert(end == -1); // We've hit all of the double-quotes.

            // Write the remainder after the last double-quote.
            Write(literal, start, literal.Length - start);

            Write("\"");
        }

        private void WriteCStyleStringLiteral(string literal)
        {
            // From CSharpCodeGenerator.QuoteSnippetStringCStyle in CodeDOM
            Write("\"");

            // We need to find the index of each escapable character to escape it.
            var start = 0;
            int end;
            while ((end = literal.IndexOfAny(CStyleStringLiteralEscapeChars, start)) > -1)
            {
                Write(literal, start, end - start);

                switch (literal[end])
                {
                    case '\r':
                        Write("\\r");
                        break;
                    case '\t':
                        Write("\\t");
                        break;
                    case '\"':
                        Write("\\\"");
                        break;
                    case '\'':
                        Write("\\\'");
                        break;
                    case '\\':
                        Write("\\\\");
                        break;
                    case '\0':
                        Write("\\\0");
                        break;
                    case '\n':
                        Write("\\n");
                        break;
                    case '\u2028':
                    case '\u2029':
                        Write("\\u");
                        Write(((int)literal[end]).ToString("X4", CultureInfo.InvariantCulture));
                        break;
                    default:
                        Debug.Assert(false, "Unknown escape character.");
                        break;
                }

                start = end + 1;
            }

            Debug.Assert(end == -1); // We've hit all of chars that need escaping.

            // Write the remainder after the last escaped char.
            Write(literal, start, literal.Length - start);

            Write("\"");
        }
    }
}
