﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Microsoft.AspNetCore.Razor.Evolution
{
    internal class LargeTextRazorSourceDocument : RazorSourceDocument
    {
        private readonly List<char[]> _chunks;

        private readonly int _chunkMaxLength;

        private readonly RazorSourceLineCollection _lines;

        private readonly int _length;

        public LargeTextRazorSourceDocument(StreamReader reader, int chunkMaxLength, Encoding encoding, string filename)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            if (encoding == null)
            {
                throw new ArgumentNullException(nameof(encoding));
            }

            _chunkMaxLength = chunkMaxLength;
            Encoding = encoding;
            Filename = filename;

            ReadChunks(reader, _chunkMaxLength, out _length, out _chunks);
            _lines = new DefaultRazorSourceLineCollection(this);
        }

        public override char this[int position]
        {
            get
            {
                var chunkIndex = position / _chunkMaxLength;
                var insideChunkPosition = position - chunkIndex * _chunkMaxLength;

                return _chunks[chunkIndex][insideChunkPosition];
            }
        }

        public override Encoding Encoding { get; }

        public override string Filename { get; }

        public override int Length => _length;

        public override RazorSourceLineCollection Lines => _lines;

        public override void CopyTo(int sourceIndex, char[] destination, int destinationIndex, int count)
        {
            if (destination == null)
            {
                throw new ArgumentNullException(nameof(destination));
            }

            if (sourceIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(sourceIndex));
            }

            if (destinationIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(destinationIndex));
            }

            if (count < 0 || count > Length - sourceIndex || count > destination.Length - destinationIndex)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            if (count == 0)
            {
                return;
            }

            var chunkIndex = sourceIndex / _chunkMaxLength;
            var insideChunkPosition = sourceIndex - chunkIndex * _chunkMaxLength;
            var remaining = count;
            var currentDestIndex = destinationIndex;

            while (remaining > 0)
            {
                var toCopy = Math.Min(remaining, _chunkMaxLength - insideChunkPosition);
                Array.Copy(_chunks[chunkIndex], insideChunkPosition, destination, currentDestIndex, toCopy);

                remaining -= toCopy;
                currentDestIndex += toCopy;
                chunkIndex++;
                insideChunkPosition = 0;
            }
        }

        private static void ReadChunks(StreamReader reader, int chunkMaxLength, out int length, out List<char[]> chunks)
        {
            length = 0;
            chunks = new List<char[]>();

            for (;;)
            {
                var chunk = new char[chunkMaxLength];
                var remaining = chunkMaxLength;
                var index = 0;

                while (remaining > 0)
                {
                    var charsRead = reader.ReadBlock(chunk, index, remaining);
                    if (charsRead == 0)
                    {
                        if (remaining != chunkMaxLength)
                        {
                            chunks.Add(chunk);
                        }
                        return;
                    }

                    length += charsRead;
                    remaining -= charsRead;
                    index += charsRead;
                }

                chunks.Add(chunk);
            }
        }
    }
}
