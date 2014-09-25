// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <inheritdoc />
    public class TagHelperScopeManager : ITagHelperScopeManager
    {
        private Stack<TagHelpersExecutionContext> _executionScopes;

        public TagHelperScopeManager()
        {
            _executionScopes = new Stack<TagHelpersExecutionContext>();
        }

        /// <inheritdoc />
        public TagHelpersExecutionContext Begin(string tagName)
        {
            var executionContext = new TagHelpersExecutionContext(tagName);

            _executionScopes.Push(executionContext);

            return executionContext;
        }

        /// <inheritdoc />
        public TagHelpersExecutionContext End()
        {
            if (_executionScopes.Count == 0)
            {
                throw new InvalidOperationException(
                    Resources.FormatScopeManager_EndCannotBeCalledWithoutACallToBegin(nameof(End), nameof(Begin)));
            }

            _executionScopes.Pop();

            if (_executionScopes.Count > 0)
            {
                return _executionScopes.Peek();
            }
            else
            {
                return null;
            }
        }
    }
}