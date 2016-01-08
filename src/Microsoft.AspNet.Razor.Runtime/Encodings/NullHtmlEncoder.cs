﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;

namespace System.Text.Encodings.Web
{
    /// <summary>
    /// A <see cref="HtmlEncoder"/> that does not encode. Should not be used when writing directly to a response
    /// expected to contain valid HTML.
    /// </summary>
    public class NullHtmlEncoder : HtmlEncoder
    {
        /// <summary>
        /// Initializes a <see cref="NullHtmlEncoder"/> instance.
        /// </summary>
        protected NullHtmlEncoder()
        {
        }

        /// <summary>
        /// A <see cref="HtmlEncoder"/> instance that does not encode. Should not be used when writing directly to a
        /// response expected to contain valid HTML.
        /// </summary>
        public static new NullHtmlEncoder Default { get; } = new NullHtmlEncoder();

        /// <inheritdoc />
        public override int MaxOutputCharactersPerInputCharacter
        {
            get
            {
                return 1;
            }
        }

        /// <inheritdoc />
        public override string Encode(string value)
        {
            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            return value;
        }

        /// <inheritdoc />
        public override void Encode(TextWriter output, char[] value, int startIndex, int characterCount)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (characterCount == 0)
            {
                return;
            }

            output.Write(value, startIndex, characterCount);
        }

        /// <inheritdoc />
        public override void Encode(TextWriter output, string value, int startIndex, int characterCount)
        {
            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (value == null)
            {
                throw new ArgumentNullException(nameof(value));
            }

            if (characterCount == 0)
            {
                return;
            }

            output.Write(value.Substring(startIndex, characterCount));
        }

        /// <inheritdoc />
        public override unsafe int FindFirstCharacterToEncode(char* text, int textLength)
        {
            return -1;
        }

        /// <inheritdoc />
        public override unsafe bool TryEncodeUnicodeScalar(
            int unicodeScalar,
            char* buffer,
            int bufferLength,
            out int numberOfCharactersWritten)
        {
            if (buffer == null)
            {
                throw new ArgumentNullException(nameof(buffer));
            }

            numberOfCharactersWritten = 0;

            return false;
        }

        /// <inheritdoc />
        public override bool WillEncode(int unicodeScalar)
        {
            return false;
        }
    }
}
