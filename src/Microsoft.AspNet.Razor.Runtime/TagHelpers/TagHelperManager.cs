// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    /// <summary>
    /// A class used to manage <see cref="TagHelper"/>s during runtime.
    /// </summary>
    public class TagHelperManager : ITagHelperManager
    {
        private Stack<TagHelpersExecutionContext> _executionContexts;
        private TagHelpersExecutionContext _currentExecutionContext;
        private bool _executionContextComplete;

        /// <summary>
        /// Instantiates a new instance of <see cref="TagHelperManager"/>.
        /// </summary>
        public TagHelperManager()
        {
            _executionContexts = new Stack<TagHelpersExecutionContext>();
            _executionContextComplete = true;
        }

        /// <summary>
        /// The current execution context.
        /// </summary>
        protected TagHelpersExecutionContext CurrentContext
        {
            get
            {
                return _currentExecutionContext;
            }
        }

        /// <inheritdoc />
        public TTagHelper InstantiateTagHelper<TTagHelper>() where TTagHelper : TagHelper
        {
            if (_executionContextComplete)
            {
                _executionContextComplete = false;

                _currentExecutionContext = new TagHelpersExecutionContext();
                _executionContexts.Push(_currentExecutionContext);
            }

            var tagHelper = CreateTagHelper<TTagHelper>();

            CurrentContext.ActiveTagHelpers.Add(tagHelper);

            return tagHelper;
        }

        /// <inheritdoc />
        public void StartTagHelpersScope(string tagName)
        {
            _executionContextComplete = true;
            CurrentContext.CreateTagHelperOutput(tagName, CurrentContext.HTMLAttributes);
        }

        /// <inheritdoc />
        public void EndTagHelpersScope()
        {
            _executionContextComplete = true;
            _executionContexts.Pop();

            if (_executionContexts.Count > 0)
            {
                _currentExecutionContext = _executionContexts.Peek();
            }
            else
            {
                _currentExecutionContext = null;
            }
        }

        /// <inheritdoc />
        public void AddHtmlAttribute(string name, string value)
        {
            _currentExecutionContext.HTMLAttributes.Add(name, value);
            _currentExecutionContext.AllAttributes.Add(name, value);
        }

        /// <inheritdoc />
        public void AddTagHelperAttribute(string name, object value)
        {
            _currentExecutionContext.AllAttributes.Add(name, value);
        }

        /// <inheritdoc />
        public TextWriter GetContentBuffer()
        {
            return CurrentContext.TagHelperOutput.GetContentWriter();
        }

        /// <inheritdoc />
        public string GenerateTagStart()
        {
            return CurrentContext.TagHelperOutput.GenerateTagStart();
        }

        /// <inheritdoc />
        public string GenerateTagContent()
        {
            return CurrentContext.TagHelperOutput.GenerateTagContent();
        }

        /// <inheritdoc />
        public string GenerateTagEnd()
        {
            return CurrentContext.TagHelperOutput.GenerateTagEnd();
        }

        /// <inheritdoc />
        public virtual async Task ExecuteTagHelpersAsync()
        {
            var context = new TagHelperContext(CurrentContext.AllAttributes);

            foreach (var tagHelper in CurrentContext.ActiveTagHelpers)
            {
                await tagHelper.ProcessAsync(CurrentContext.TagHelperOutput, context);
            }
        }

        /// <summary>
        /// Creates a <see cref="TagHelper"/> of type <typeparamref name="TTagHelper"/>.
        /// </summary>
        /// <typeparam name="TTagHelper">The <see cref="Type"/> of <see cref="TagHelper"/>
        /// to create.</typeparam>
        /// <returns>A <see cref="TagHelper"/> of type <typeparamref name="TTagHelper"/>.</returns>
        protected virtual TTagHelper CreateTagHelper<TTagHelper>() where TTagHelper : TagHelper
        {
            return Activator.CreateInstance<TTagHelper>();
        }
    }
}