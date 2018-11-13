// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal abstract class ParentChunkGenerator : IParentChunkGenerator
    {
        private static readonly int TypeHashCode = typeof(ParentChunkGenerator).GetHashCode();

        public static readonly IParentChunkGenerator Null = new NullParentChunkGenerator();

        public override bool Equals(object obj)
        {
            return obj != null &&
                GetType() == obj.GetType();
        }

        public override int GetHashCode()
        {
            return TypeHashCode;
        }

        private class NullParentChunkGenerator : IParentChunkGenerator
        {
            public override string ToString()
            {
                return "None";
            }
        }
    }
}
