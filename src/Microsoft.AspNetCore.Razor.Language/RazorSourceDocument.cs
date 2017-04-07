﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;

namespace Microsoft.AspNetCore.Razor.Language
{
    /// <summary>
    /// The Razor template source.
    /// </summary>
    public abstract class RazorSourceDocument
    {
        internal const int LargeObjectHeapLimitInChars = 40 * 1024; // 40K Unicode chars is 80KB which is less than the large object heap limit.

        internal static readonly RazorSourceDocument[] EmptyArray = new RazorSourceDocument[0];

        /// <summary>
        /// Encoding of the file that the text was read from.
        /// </summary>
        public abstract Encoding Encoding { get; }

        /// <summary>
        /// Path of the file the content was read from.
        /// </summary>
        public abstract string FileName { get; }

        /// <summary>
        /// Gets a character at given position.
        /// </summary>
        /// <param name="position">The position to get the character from.</param>
        public abstract char this[int position] { get; }

        /// <summary>
        /// Gets the length of the text in characters.
        /// </summary>
        public abstract int Length { get; }

        /// <summary>
        /// Gets the <see cref="RazorSourceLineCollection"/>.
        /// </summary>
        public abstract RazorSourceLineCollection Lines { get; }

        /// <summary>
        /// Copies a range of characters from the <see cref="RazorSourceDocument"/> to the specified <paramref name="destination"/>.
        /// </summary>
        /// <param name="sourceIndex">The index of the first character in this instance to copy.</param>
        /// <param name="destination">The destination buffer.</param>
        /// <param name="destinationIndex">The index in destination at which the copy operation begins.</param>
        /// <param name="count">The number of characters in this instance to copy to destination.</param>
        public abstract void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count);

        /// <summary>
        /// Reads the <see cref="RazorSourceDocument"/> from the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        /// <param name="fileName">The file name of the template.</param>
        /// <returns>The <see cref="RazorSourceDocument"/>.</returns>
        public static RazorSourceDocument ReadFrom(Stream stream, string fileName)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            return ReadFromInternal(stream, fileName, encoding: null);
        }

        /// <summary>
        /// Reads the <see cref="RazorSourceDocument"/> from the specified <paramref name="stream"/>.
        /// </summary>
        /// <param name="stream">The <see cref="Stream"/> to read from.</param>
        /// <param name="fileName">The file name of the template.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/> to use to read the <paramref name="stream"/>.</param>
        /// <returns>The <see cref="RazorSourceDocument"/>.</returns>
        public static RazorSourceDocument ReadFrom(Stream stream, string fileName, Encoding encoding)
        {
            if (stream == null)
            {
                throw new ArgumentNullException(nameof(stream));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return ReadFromInternal(stream, fileName, encoding);
        }

        /// <summary>
        /// Reads the <see cref="RazorSourceDocument"/> from the specified <paramref name="projectItem"/>.
        /// </summary>
        /// <param name="projectItem">The <see cref="RazorProjectItem"/> to read from.</param>
        /// <returns>The <see cref="RazorSourceDocument"/>.</returns>
        public static RazorSourceDocument ReadFrom(RazorProjectItem projectItem)
        {
            if (projectItem == null)
            {
                throw new ArgumentNullException(nameof(projectItem));
            }

            var path = projectItem.PhysicalPath;
            if (string.IsNullOrEmpty(path))
            {
                path = projectItem.Path;
            }

            using (var inputStream = projectItem.Read())
            {
                return ReadFrom(inputStream, path);
            }
        }

        /// <summary>
        /// Creates a <see cref="RazorSourceDocument"/> from the specified <paramref name="content"/>.
        /// </summary>
        /// <param name="content">The template content.</param>
        /// <param name="fileName">The file name of the <see cref="RazorSourceDocument"/>.</param>
        /// <returns>The <see cref="RazorSourceDocument"/>.</returns>
        /// <remarks>Uses <see cref="System.Text.Encoding.UTF8" /></remarks>
        public static RazorSourceDocument Create(string content, string fileName)
            => Create(content, fileName, Encoding.UTF8);

        /// <summary>
        /// Creates a <see cref="RazorSourceDocument"/> from the specified <paramref name="content"/>.
        /// </summary>
        /// <param name="content">The template content.</param>
        /// <param name="fileName">The file name of the <see cref="RazorSourceDocument"/>.</param>
        /// <param name="encoding">The <see cref="System.Text.Encoding"/> of the file <paramref name="content"/> was read from.</param>
        /// <returns>The <see cref="RazorSourceDocument"/>.</returns>
        public static RazorSourceDocument Create(string content, string fileName, Encoding encoding)
        {
            if (content == null)
            {
                throw new ArgumentNullException(nameof(content));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            return new DefaultRazorSourceDocument(content, encoding, fileName);
        }

        private static RazorSourceDocument ReadFromInternal(Stream stream, string fileName, Encoding encoding)
        {
            var streamLength = (int)stream.Length;
            var content = string.Empty;
            var contentEncoding = encoding ?? Encoding.UTF8;

            if (streamLength > 0)
            {
                var bufferSize = Math.Min(streamLength, LargeObjectHeapLimitInChars);

                var reader = new StreamReader(
                    stream,
                    contentEncoding,
                    detectEncodingFromByteOrderMarks: true,
                    bufferSize: bufferSize,
                    leaveOpen: true);

                using (reader)
                {
                    reader.Peek();      // Just to populate the encoding

                    if (encoding == null)
                    {
                        contentEncoding = reader.CurrentEncoding;
                    }
                    else if (encoding != reader.CurrentEncoding)
                    {
                        throw new InvalidOperationException(
                            Resources.FormatMismatchedContentEncoding(
                                encoding.EncodingName,
                                reader.CurrentEncoding.EncodingName));
                    }

                    if (streamLength > LargeObjectHeapLimitInChars)
                    {
                        // If the resulting string would end up on the large object heap, then use LargeTextRazorSourceDocument.
                        return new LargeTextRazorSourceDocument(
                            reader,
                            LargeObjectHeapLimitInChars,
                            contentEncoding,
                            fileName);
                    }

                    content = reader.ReadToEnd();
                }
            }

            return new DefaultRazorSourceDocument(content, contentEncoding, fileName);
        }
    }
}
