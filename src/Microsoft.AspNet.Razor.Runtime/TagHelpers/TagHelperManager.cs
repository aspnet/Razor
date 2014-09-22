// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.IO;

namespace Microsoft.AspNet.Razor.Runtime.TagHelpers
{
    public abstract class TagHelperManager<TExecutionContext, TTagHelperInterface>
        where TExecutionContext : TagHelpersExecutionContext<TTagHelperInterface>, new()
    {
        private Stack<TExecutionContext> _executionContexts;
        private TExecutionContext _currentExecutionContext;
        private bool _executionContextComplete;

        public TagHelperManager()
        {
            _executionContexts = new Stack<TExecutionContext>();
            _executionContextComplete = true;
        }

        protected TExecutionContext CurrentContext
        {
            get
            {
                return _currentExecutionContext;
            }
        }

        public TTagHelper StartTagHelper<TTagHelper>() where TTagHelper : TTagHelperInterface
        {
            if (_executionContextComplete)
            {
                _executionContextComplete = false;

                _currentExecutionContext = new TExecutionContext();
                _executionContexts.Push(_currentExecutionContext);
            }

            var tagHelper = CreateTagHelper<TTagHelper>();

            CurrentContext.ActiveTagHelpers.Add(tagHelper);

            return tagHelper;
        }

        public void EndTagHelpers()
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

        public void AddHTMLAttribute(string name, string value)
        {
            _currentExecutionContext.HTMLAttributes.Add(name, value);
            _currentExecutionContext.AllAttributes.Add(name, value);
        }

        public void AddTagHelperAttribute(string name, object value)
        {
            _currentExecutionContext.AllAttributes.Add(name, value);
        }

        public void StartActiveTagHelpers(string tagName)
        {
            _executionContextComplete = true;
            PrepareActiveTagHelpers(tagName);
        }

        public abstract void ExecuteTagHelpers();

        public abstract TextWriter GetTagBodyBuffer();

        public abstract string GenerateTagStart();

        public abstract string GenerateTagContent();

        public abstract string GenerateTagEnd();

        protected abstract void PrepareActiveTagHelpers(string tagName);

        protected abstract TTagHelper CreateTagHelper<TTagHelper>() where TTagHelper : TTagHelperInterface;
    }
}