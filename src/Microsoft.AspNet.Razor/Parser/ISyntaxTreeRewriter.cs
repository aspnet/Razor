// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.AspNet.Razor.Parser.SyntaxTree;

namespace Microsoft.AspNet.Razor.Parser
{
    /// <summary>
    /// Defines a class that is used to re-write a parse tree.
    /// </summary>
    public interface ISyntaxTreeRewriter
    {
        /// <summary>
        /// Rewrites the the provided <paramref name="input"/>.
        /// </summary>
        /// <remarks>
        /// If you choose not to modify the parse tree you can always return <paramref name="input"/>.
        /// </remarks>
        /// <param name="input">The current parse tree.</param>
        /// <returns>A parse tree to be used instead of <paramref name="input"/>.</returns>
        Block Rewrite(Block input);
    }
}
