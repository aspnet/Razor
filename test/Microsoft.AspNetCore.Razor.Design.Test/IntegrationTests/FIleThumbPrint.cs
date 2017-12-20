// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace Microsoft.AspNetCore.Razor.Design.IntegrationTests
{
    public class FileThumbPrint : IEquatable<FileThumbPrint>
    {
        private FileThumbPrint(DateTime lastWriteTimeUtc, string hash)
        {
            LastWriteTimeUtc = lastWriteTimeUtc;
            Hash = hash;
        }

        public DateTime LastWriteTimeUtc { get; }

        public string Hash { get; }

        public static FileThumbPrint Create(string path)
        {
            byte[] hashBytes;
            using (var sha1 = SHA1.Create())
            using (var fileStream = File.OpenRead(path))
            {
                hashBytes = sha1.ComputeHash(fileStream);
            }

            var hash = Encoding.UTF8.GetString(hashBytes);
            var lastWriteTimeUtc = File.GetLastWriteTimeUtc(path);
            return new FileThumbPrint(lastWriteTimeUtc, hash);
        }

        public bool Equals(FileThumbPrint other)
        {
            return LastWriteTimeUtc == other.LastWriteTimeUtc &&
                string.Equals(Hash, other.Hash, StringComparison.Ordinal);
        }

        public override int GetHashCode() => LastWriteTimeUtc.GetHashCode();
    }
}
