// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// Defines a contract that is used to manage <see cref="TagHelper"/> <see cref="TagHelpersExecutionContext"/>
    /// scopes.
    /// </summary>
    public interface ITagHelperScopeManager
    {
        /// <summary>
        /// Starts a new <see cref="TagHelpersExecutionContext"/> scope.
        /// </summary>
        /// <param name="tagName">The HTML tag name that the scope is associated with.</param>
        /// <returns>A <see cref="TagHelpersExecutionContext"/> to use.</returns>
        TagHelpersExecutionContext Begin(string tagName);

        /// <summary>
        /// Ends the <see cref="TagHelpersExecutionContext"/> scope that was initially started by calling 
        /// <see cref="Begin(string)"/>.
        /// </summary>
        /// <returns>If the current scope is nested, returns the parent <see cref="TagHelpersExecutionContext"/>,
        /// <c>null</c> otherwise.</returns>
        TagHelpersExecutionContext End();
    }
}