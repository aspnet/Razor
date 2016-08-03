// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using Microsoft.AspNetCore.Razor.Chunks.Generators;
using Microsoft.AspNetCore.Razor.CodeGenerators;
using Microsoft.AspNetCore.Razor.Parser;
using Microsoft.AspNetCore.Razor.Text;

namespace Microsoft.AspNetCore.Razor
{
    /// <summary>
    /// Entry-point to the Razor Template Engine
    /// </summary>
    public class RazorTemplateEngine
    {
        private const int BufferSize = 1024;
        public static readonly string DefaultClassName = "Template";
        public static readonly string DefaultNamespace = string.Empty;

        /// <summary>
        /// Constructs a new RazorTemplateEngine with the specified host
        /// </summary>
        /// <param name="host">
        /// The host which defines the environment in which the generated template code will live.
        /// </param>
        public RazorTemplateEngine(RazorEngineHost host)
        {
            if (host == null)
            {
                throw new ArgumentNullException(nameof(host));
            }

            Host = host;
        }

        /// <summary>
        /// The RazorEngineHost which defines the environment in which the generated template code will live
        /// </summary>
        public RazorEngineHost Host { get; }

        public ParserResults ParseTemplate(ITextBuffer input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return ParseTemplate(input, cancelToken: null);
        }

        /// <summary>
        /// Parses the template specified by the TextBuffer and returns it's result
        /// </summary>
        /// <remarks>
        /// <para>
        /// IMPORTANT: This does NOT need to be called before GeneratedCode! GenerateCode will automatically
        /// parse the document first.
        /// </para>
        /// <para>
        /// The cancel token provided can be used to cancel the parse.  However, please note
        /// that the parse occurs _synchronously_, on the callers thread.  This parameter is
        /// provided so that if the caller is in a background thread with a CancellationToken,
        /// it can pass it along to the parser.
        /// </para>
        /// </remarks>
        /// <param name="input">The input text to parse.</param>
        /// <param name="cancelToken">A token used to cancel the parser.</param>
        /// <returns>The resulting parse tree.</returns>
        public ParserResults ParseTemplate(ITextBuffer input, CancellationToken? cancelToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return ParseTemplateCore(input.ToDocument(), sourceFileName: null, cancelToken: cancelToken);
        }

        // See ParseTemplate(ITextBuffer, CancellationToken?),
        // this overload simply wraps a TextReader in a TextBuffer (see ITextBuffer)
        public ParserResults ParseTemplate(TextReader input, string sourceFileName)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return ParseTemplateCore(new SeekableTextReader(input), sourceFileName, cancelToken: null);
        }

        public ParserResults ParseTemplate(TextReader input, CancellationToken? cancelToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return ParseTemplateCore(new SeekableTextReader(input), sourceFileName: null, cancelToken: cancelToken);
        }

        protected internal virtual ParserResults ParseTemplateCore(
            ITextDocument input,
            string sourceFileName,
            CancellationToken? cancelToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            // Construct the parser
            var parser = CreateParser(sourceFileName);
            Debug.Assert(parser != null);
            return parser.Parse(input);
        }

        public GeneratorResults GenerateCode(ITextBuffer input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return GenerateCode(input, className: null, rootNamespace: null, sourceFileName: null, cancelToken: null);
        }

        public GeneratorResults GenerateCode(ITextBuffer input, CancellationToken? cancelToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return GenerateCode(
                input,
                className: null,
                rootNamespace: null,
                sourceFileName: null,
                cancelToken: cancelToken);
        }

        public GeneratorResults GenerateCode(
            ITextBuffer input,
            string className,
            string rootNamespace,
            string sourceFileName)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return GenerateCode(input, className, rootNamespace, sourceFileName, cancelToken: null);
        }

        /// <summary>
        /// Parses the template specified by the TextBuffer, generates code for it, and returns the constructed code.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The cancel token provided can be used to cancel the parse.  However, please note
        /// that the parse occurs _synchronously_, on the callers thread.  This parameter is
        /// provided so that if the caller is in a background thread with a CancellationToken,
        /// it can pass it along to the parser.
        /// </para>
        /// <para>
        /// The className, rootNamespace and sourceFileName parameters are optional and override the default
        /// specified by the Host.  For example, the WebPageRazorHost in System.Web.WebPages.Razor configures the
        /// Class Name, Root Namespace and Source File Name based on the virtual path of the page being compiled.
        /// However, the built-in RazorEngineHost class uses constant defaults, so the caller will likely want to
        /// change them using these parameters.
        /// </para>
        /// </remarks>
        /// <param name="input">The input text to parse.</param>
        /// <param name="cancelToken">A token used to cancel the parser.</param>
        /// <param name="className">
        /// The name of the generated class, overriding whatever is specified in the Host.  The default value (defined
        /// in the Host) can be used by providing null for this argument.
        /// </param>
        /// <param name="rootNamespace">The namespace in which the generated class will reside, overriding whatever is
        /// specified in the Host.  The default value (defined in the Host) can be used by providing null for this
        /// argument.
        /// </param>
        /// <param name="sourceFileName">
        /// The file name to use in line pragmas, usually the original Razor file, overriding whatever is specified in
        /// the Host.  The default value (defined in the Host) can be used by providing null for this argument.
        /// </param>
        /// <returns>The resulting parse tree AND generated code.</returns>
        public GeneratorResults GenerateCode(
            ITextBuffer input,
            string className,
            string rootNamespace,
            string sourceFileName,
            CancellationToken? cancelToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return GenerateCodeCore(
                input.ToDocument(),
                className,
                rootNamespace,
                sourceFileName,
                checksum: null,
                cancelToken: cancelToken);
        }

        // See GenerateCode override which takes ITextBuffer, and BufferingTextReader for details.
        public GeneratorResults GenerateCode(TextReader input)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return GenerateCode(input, className: null, rootNamespace: null, sourceFileName: null, cancelToken: null);
        }

        public GeneratorResults GenerateCode(TextReader input, CancellationToken? cancelToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return GenerateCode(
                input,
                className: null,
                rootNamespace: null,
                sourceFileName: null,
                cancelToken: cancelToken);
        }

        public GeneratorResults GenerateCode(
            TextReader input,
            string className,

            string rootNamespace, string sourceFileName)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return GenerateCode(input, className, rootNamespace, sourceFileName, cancelToken: null);
        }

        /// <summary>
        /// Parses the contents specified by the <paramref name="inputStream"/> and returns the generated code.
        /// </summary>
        /// <param name="inputStream">A <see cref="Stream"/> that represents the contents to be parsed.</param>
        /// <param name="className">The name of the generated class. When <c>null</c>, defaults to
        /// <see cref="RazorEngineHost.DefaultClassName"/> (<c>Host.DefaultClassName</c>).</param>
        /// <param name="rootNamespace">The namespace in which the generated class will reside. When <c>null</c>,
        /// defaults to <see cref="RazorEngineHost.DefaultNamespace"/> (<c>Host.DefaultNamespace</c>).</param>
        /// <param name="sourceFileName">
        /// The file name to use in line pragmas, usually the original Razor file.
        /// </param>
        /// <returns>A <see cref="GeneratorResults"/> that represents the results of parsing the content.</returns>
        /// <remarks>
        /// This overload calculates the checksum of the contents of <paramref name="inputStream"/> prior to code
        /// generation. The checksum is used for producing the <c>#pragma checksum</c> line pragma required for
        /// debugging.
        /// </remarks>
        public GeneratorResults GenerateCode(
            Stream inputStream,
            string className,
            string rootNamespace,
            string sourceFileName)
        {
            if (inputStream == null)
            {
                throw new ArgumentNullException(nameof(inputStream));
            }

            var checksumResult = CalculateChecksum(inputStream);
            using (checksumResult.CopiedStream)
            {
                inputStream = checksumResult.CopiedStream ?? inputStream;
                using (var reader = CreateNonDisposingStreamReader(inputStream))
                {
                    var seekableStream = new SeekableTextReader(reader);
                    return GenerateCodeCore(
                        seekableStream,
                        className,
                        rootNamespace,
                        sourceFileName,
                        checksumResult.Checksum,
                        cancelToken: null);
                }
            }
        }

        public GeneratorResults GenerateCode(TemplateEngineContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (context.InputStream == null)
            {
                throw new ArgumentNullException(nameof(context.InputStream));
            }

            var checksumResult = CalculateChecksum(context.InputStream);
            using (checksumResult.CopiedStream)
            {
                var inputStream = checksumResult.CopiedStream ?? context.InputStream;
                using (var reader = CreateNonDisposingStreamReader(inputStream))
                {
                    var seekableReader = new SeekableTextReader(reader);
                    return GenerateCodeCore(seekableReader, context, checksumResult.Checksum);
                }
            }
        }

        private ChecksumResult CalculateChecksum(Stream inputStream)
        {
            if (Host.DesignTimeMode)
            {
                // We don't need to calculate the checksum in design time.
                return new ChecksumResult();
            }

            MemoryStream memoryStream = null;
            if (!inputStream.CanSeek)
            {
                memoryStream = new MemoryStream();
                inputStream.CopyTo(memoryStream);

                // We don't have to dispose the input stream since it is owned externally.
                inputStream = memoryStream;
            }

            inputStream.Position = 0;
            var checksum = ComputeChecksum(inputStream);
            inputStream.Position = 0;

            return new ChecksumResult(checksum, memoryStream);
        }

        public GeneratorResults GenerateCode(
            TextReader input,
            string className,
            string rootNamespace,
            string sourceFileName,
            CancellationToken? cancelToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            return GenerateCodeCore(
                new SeekableTextReader(input),
                className,
                rootNamespace,
                sourceFileName,
                checksum: null,
                cancelToken: cancelToken);
        }

        protected internal virtual GeneratorResults GenerateCodeCore(
            ITextDocument input,
            string className,
            string rootNamespace,
            string sourceFileName,
            string checksum,
            CancellationToken? cancelToken)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            className = (className ?? Host.DefaultClassName) ?? DefaultClassName;
            rootNamespace = (rootNamespace ?? Host.DefaultNamespace) ?? DefaultNamespace;

            // Run the parser
            var parser = CreateParser(sourceFileName);
            Debug.Assert(parser != null);
            var results = parser.Parse(input);

            return GenerateCodeCore(
                className,
                rootNamespace,
                sourceFileName,
                checksum,
                results);
        }

        protected GeneratorResults GenerateCodeCore(
            ITextDocument input,
            TemplateEngineContext context,
            string checksum)
        {
            if (input == null)
            {
                throw new ArgumentNullException(nameof(input));
            }

            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var className = (context.ClassName ?? Host.DefaultClassName) ?? DefaultClassName;
            var rootNamespace = (context.RootNamespace ?? Host.DefaultNamespace) ?? DefaultNamespace;

            // Run the parser
            var parser = CreateParser(context);
            Debug.Assert(parser != null);
            var parserResults = parser.Parse(input);

            return GenerateCodeCore(
                className,
                rootNamespace,
                context.FilePath,
                checksum,
                parserResults);
        }

        private GeneratorResults GenerateCodeCore(
            string className,
            string rootNamespace,
            string filePath,
            string checksum,
            ParserResults results)
        {
            // Generate code
            var chunkGenerator = CreateChunkGenerator(className, rootNamespace, filePath);
            chunkGenerator.DesignTimeMode = Host.DesignTimeMode;
            chunkGenerator.Visit(results);

            var codeGeneratorContext = new CodeGeneratorContext(chunkGenerator.Context, results.ErrorSink);
            codeGeneratorContext.Checksum = checksum;
            var codeGenerator = CreateCodeGenerator(codeGeneratorContext);
            var codeGeneratorResult = codeGenerator.Generate();

            // Collect results and return
            return new GeneratorResults(results, codeGeneratorResult, codeGeneratorContext.ChunkTreeBuilder.Root);
        }

        protected internal virtual RazorChunkGenerator CreateChunkGenerator(
            string className,
            string rootNamespace,
            string sourceFileName)
        {
            return Host.DecorateChunkGenerator(
                Host.CodeLanguage.CreateChunkGenerator(className, rootNamespace, sourceFileName, Host));
        }

        protected internal virtual RazorParser CreateParser(string sourceFileName)
        {
            var codeParser = Host.CodeLanguage.CreateCodeParser();
            var markupParser = Host.CreateMarkupParser();

            var parser = new RazorParser(
                Host.DecorateCodeParser(codeParser),
                Host.DecorateMarkupParser(markupParser),
                Host.TagHelperDescriptorResolver)
            {
                DesignTimeMode = Host.DesignTimeMode
            };

            return Host.DecorateRazorParser(parser, sourceFileName);
        }

        private RazorParser CreateParser(TemplateEngineContext context)
        {
            var codeParser = Host.CodeLanguage.CreateCodeParser();
            var markupParser = Host.CreateMarkupParser();

            var parser = new RazorParser(
                Host.DecorateCodeParser(codeParser),
                Host.DecorateMarkupParser(markupParser),
                Host.TagHelperDescriptorResolver)
            {
                DesignTimeMode = Host.DesignTimeMode
            };

            return Host.DecorateRazorParser(parser, context);
        }

        protected internal virtual CodeGenerator CreateCodeGenerator(CodeGeneratorContext context)
        {
            return Host.DecorateCodeGenerator(Host.CodeLanguage.CreateCodeGenerator(context), context);
        }

        private static string ComputeChecksum(Stream inputStream)
        {
            byte[] hashedBytes;
            using (var hashAlgorithm = SHA1.Create())
            {
                hashedBytes = hashAlgorithm.ComputeHash(inputStream);
            }

            var fileHashBuilder = new StringBuilder(hashedBytes.Length * 2);
            foreach (var value in hashedBytes)
            {
                fileHashBuilder.Append(value.ToString("x2"));
            }
            return fileHashBuilder.ToString();
        }

        private static StreamReader CreateNonDisposingStreamReader(Stream inputStream)
        {
            return new StreamReader(
                inputStream,
                Encoding.UTF8,
                detectEncodingFromByteOrderMarks: true,
                bufferSize: BufferSize,
                leaveOpen: true);
        }

        private struct ChecksumResult
        {
            public ChecksumResult(string checksum, MemoryStream copiedStream)
            {
                Checksum = checksum;
                CopiedStream = copiedStream;
            }

            public string Checksum { get; }

            public Stream CopiedStream { get; }
        }
    }
}
