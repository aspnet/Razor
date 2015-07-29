// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
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
        public override bool IsModified
        {
            get
            {
                return (_buffer != null);
            }
        }

        /// <inheritdoc />
        public override bool IsWhiteSpace
        {
            get
            {
                if (_buffer == null)
                {
                    return true;
                }

                return CheckIfBufferedHtmlContentIsWhiteSpace(Buffer);
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

                return CheckIfBufferedHtmlContentIsEmpty(Buffer);
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

            return string.Join(string.Empty, Buffer);
        }

        /// <inheritdoc />
        public override string ToString()
        {
            using (var writer = new StringWriter())
            {
                WriteTo(writer, new HtmlEncoder());
                return writer.ToString();
            }
        }

        /// <inheritdoc />
        public override void WriteTo([NotNull] TextWriter writer, [NotNull] IHtmlEncoder encoder)
        {
            Buffer.WriteTo(writer, encoder);
        }

        private bool CheckIfBufferedHtmlContentIsEmpty(BufferedHtmlContent buffer)
        {
            foreach (var value in buffer)
            {
                var valueAsString = value as string;
                if (valueAsString != null)
                {
                    if (!string.IsNullOrEmpty(valueAsString))
                    {
                        return false;
                    }
                }
                else
                {
                    var valueAsBufferedHtmlContent = value as BufferedHtmlContent;
                    if (valueAsBufferedHtmlContent != null)
                    {
                        if (!CheckIfBufferedHtmlContentIsEmpty(valueAsBufferedHtmlContent))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(value.ToString()))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }

        private bool CheckIfBufferedHtmlContentIsWhiteSpace(BufferedHtmlContent buffer)
        {
            foreach (var value in buffer)
            {
                var valueAsString = value as string;
                if (valueAsString != null)
                {
                    if (!string.IsNullOrWhiteSpace(valueAsString))
                    {
                        return false;
                    }
                }
                else
                {
                    var valueAsBufferedHtmlContent = value as BufferedHtmlContent;
                    if (valueAsBufferedHtmlContent != null)
                    {
                        if (!CheckIfBufferedHtmlContentIsWhiteSpace(valueAsBufferedHtmlContent))
                        {
                            return false;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrWhiteSpace(value.ToString()))
                        {
                            return false;
                        }
                    }
                }
            }

            return true;
        }
    }
}