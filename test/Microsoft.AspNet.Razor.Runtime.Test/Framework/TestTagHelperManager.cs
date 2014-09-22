// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.IO;
using Microsoft.AspNet.Razor.Runtime.TagHelpers;

namespace Microsoft.AspNet.Razor.Runtime.Test.Framework
{
    public class TestTagHelperManager : TagHelperManager<TestTagHelpersExecutionContext, ITestTagHelper>
    {
        public TestTagHelpersExecutionContext ExposedCurrentContext
        {
            get
            {
                return CurrentContext;
            }
        }

        public override void ExecuteTagHelpers()
        {
            throw new NotImplementedException();
        }

        public override string GenerateTagContent()
        {
            throw new NotImplementedException();
        }

        public override string GenerateTagEnd()
        {
            throw new NotImplementedException();
        }

        public override string GenerateTagStart()
        {
            throw new NotImplementedException();
        }

        public override TextWriter GetTagBodyBuffer()
        {
            throw new NotImplementedException();
        }

        protected override void PrepareActiveTagHelpers(string tagName)
        {
        }

        protected override TTagHelper CreateTagHelper<TTagHelper>()
        {
            return Activator.CreateInstance<TTagHelper>();
        }
    }
}