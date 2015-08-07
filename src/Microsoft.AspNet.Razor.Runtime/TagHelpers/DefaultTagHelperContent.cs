// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Text;
using Microsoft.AspNet.Html.Abstractions;
using Microsoft.Framework.Internal;
using Microsoft.Framework.WebEncoders;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Default concrete <see cref="TagHelperContent"/>.
    /// </summary>
    public class DefaultTagHelperContent : TagHelperContent
    {
        private BufferedHtmlContent _buffer;

        // internal for testing
        internal BufferedHtmlContent Buffer
        {
            get
            {
                if (_buffer == null)
                {
                    _buffer = new BufferedHtmlContent();
                }

                return _buffer;
            }
        }

        /// <inheritdoc />
        public override bool IsModified => _buffer != null;

        /// <inheritdoc />
        /// <remarks>Returns <c>true</c> for a cleared <see cref="TagHelperContent"/>.</remarks>
        public override bool IsWhiteSpace
        {
            get
            {
                if (_buffer == null)
                {
                    return true;
                }

                using (var writer = new EmptyOrWhitespaceWriter())
                {
                    WriteTo(writer, HtmlEncoder.Default);
                    return writer.IsWhitespace;
                }
            }
        }

        /// <inheritdoc />
        public override bool IsEmpty
        {
            get
            {
                if (_buffer == null)
                {
                    return true;
                }

                using (var writer = new EmptyOrWhitespaceWriter())
                {
                    WriteTo(writer, HtmlEncoder.Default);
                    return writer.IsEmpty;
                }
            }
        }

        /// <inheritdoc />
        public override TagHelperContent Append(string value)
        {
            Buffer.Append(value);
            return this;
        }

        /// <inheritdoc />
        public override TagHelperContent AppendFormat([NotNull] string format, object arg0)
        {
            Buffer.Append(string.Format(format, arg0));
            return this;
        }

        /// <inheritdoc />
        public override TagHelperContent AppendFormat([NotNull] string format, object arg0, object arg1)
        {
            Buffer.Append(string.Format(format, arg0, arg1));
            return this;
        }

        /// <inheritdoc />
        public override TagHelperContent AppendFormat([NotNull] string format, object arg0, object arg1, object arg2)
        {
            Buffer.Append(string.Format(format, arg0, arg1, arg2));
            return this;
        }

        /// <inheritdoc />
        public override TagHelperContent AppendFormat([NotNull] string format, params object[] args)
        {
            Buffer.Append(string.Format(format, args));
            return this;
        }

        /// <inheritdoc />
        public override TagHelperContent AppendFormat(
            [NotNull] IFormatProvider provider,
            [NotNull] string format,
            object arg0)
        {
            Buffer.Append(string.Format(provider, format, arg0));
            return this;
        }

        /// <inheritdoc />
        public override TagHelperContent AppendFormat(
            [NotNull] IFormatProvider provider,
            [NotNull] string format,
            object arg0,
            object arg1)
        {
            Buffer.Append(string.Format(provider, format, arg0, arg1));
            return this;
        }

        /// <inheritdoc />
        public override TagHelperContent AppendFormat(
            [NotNull] IFormatProvider provider,
            [NotNull] string format,
            object arg0,
            object arg1,
            object arg2)
        {
            Buffer.Append(string.Format(provider, format, arg0, arg1, arg2));
            return this;
        }

        /// <inheritdoc />
        public override TagHelperContent AppendFormat(
            [NotNull] IFormatProvider provider,
            [NotNull] string format,
            params object[] args)
        {
            Buffer.Append(string.Format(provider, format, args));
            return this;
        }

        /// <inheritdoc />
        public override TagHelperContent Append(IHtmlContent htmlContent)
        {
            Buffer.Append(htmlContent);
            return this;
        }

        /// <inheritdoc />
        public override TagHelperContent Clear()
        {
            Buffer.Clear();
            return this;
        }

        /// <inheritdoc />
        public override string GetContent()
        {
            if (_buffer == null)
            {
                return string.Empty;
            }

            using (var writer = new StringWriter())
            {
                WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }

        /// <inheritdoc />
        public override void WriteTo([NotNull] TextWriter writer, [NotNull] IHtmlEncoder encoder)
        {
            Buffer.WriteTo(writer, encoder);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return GetContent();
        }

        // Overrides Write(string) to find if the content written is empty/whitespace.
        private class EmptyOrWhitespaceWriter : TextWriter
        {
            public override Encoding Encoding
            {
                get
                {
                    throw new NotImplementedException();
                }
            }

            public bool IsEmpty { get; private set; } = true;

            public bool IsWhitespace { get; private set; } = true;

#if DNXCORE50
            // This is an abstract method in DNXCore
            public override void Write(char value)
            {
                throw new NotImplementedException();
            }
#endif

            public override void Write(string value)
            {
                if (IsEmpty && !string.IsNullOrEmpty(value))
                {
                    IsEmpty = false;
                }

                if (IsWhitespace && !string.IsNullOrWhiteSpace(value))
                {
                    IsWhitespace = false;
                }
            }
        }
    }
}