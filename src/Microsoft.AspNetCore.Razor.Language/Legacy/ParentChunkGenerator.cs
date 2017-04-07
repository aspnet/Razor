// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNetCore.Razor.Language.Legacy
{
    internal abstract class ParentChunkGenerator : IParentChunkGenerator
    {
        private static readonly int TypeHashCode = typeof(ParentChunkGenerator).GetHashCode();

        public static readonly IParentChunkGenerator Null = new NullParentChunkGenerator();

        public abstract void Accept(ParserVisitor visitor, Block block);

        public virtual void GenerateStartParentChunk(Block target, ChunkGeneratorContext context)
        {
        }

        public virtual void GenerateEndParentChunk(Block target, ChunkGeneratorContext context)
        {
        }

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
            public void GenerateStartParentChunk(Block target, ChunkGeneratorContext context)
            {
            }

            public void GenerateEndParentChunk(Block target, ChunkGeneratorContext context)
            {
            }

            public override string ToString()
            {
                return "None";
            }

            public void Accept(ParserVisitor visitor, Block block)
            {
                for (var i = 0; i < block.Children.Count; i++)
                {
                    block.Children[i].Accept(visitor);
                }
            }
        }
    }
}
