// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.Generator
{
    // TODO: Document
    public class GeneratedTagHelperRenderingContext
    {
        public static readonly GeneratedTagHelperRenderingContext Default =
            new GeneratedTagHelperRenderingContext();

        // TODO: Follow same pattern as GeneratedClassContext for constructors etc.
        // when code review names have been finalized.

        public GeneratedTagHelperRenderingContext()
        {
            InstantiateTagHelperMethodName = "InstantiateTagHelper";
            ExecuteTagHelpersAsyncMethodName = "ExecuteTagHelpersAsync";
            StartActiveTagHelpersMethodName = "StartActiveTagHelpers";
            EndTagHelpersMethodName = "EndTagHelpers";
            GenerateTagStartMethodName = "GenerateTagStart";
            GenerateTagContentMethodName = "GenerateTagContent";
            GenerateTagEndMethodName = "GenerateTagEnd";
            GetTagBodyBufferMethodName = "GetTagBodyBuffer";
            AddTagHelperAttributeMethodName = "AddTagHelperAttribute";
            AddHTMLAttributeMethodName = "AddHTMLAttribute";
            TagHelperManagerName = "TagHelperManager";
            NewWritingScopeMethodName = "NewWritingScope";
            EndWritingScopeMethodName = "EndWritingScope";
        }

        public string InstantiateTagHelperMethodName { get; private set; }
        public string ExecuteTagHelpersAsyncMethodName { get; private set; }
        public string StartActiveTagHelpersMethodName { get; private set; }
        public string EndTagHelpersMethodName { get; private set; }
        public string GenerateTagStartMethodName { get; private set; }
        public string GenerateTagContentMethodName { get; private set; }
        public string GenerateTagEndMethodName { get; private set; }
        public string GetTagBodyBufferMethodName { get; private set; }
        public string AddTagHelperAttributeMethodName { get; private set; }
        public string AddHTMLAttributeMethodName { get; private set; }
        public string TagHelperManagerName { get; private set; }
        public string NewWritingScopeMethodName { get; private set; }
        public string EndWritingScopeMethodName { get; private set; }
    }
}