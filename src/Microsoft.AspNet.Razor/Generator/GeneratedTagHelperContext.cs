// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.AspNet.Razor.Generator
{
    // TODO: Document
    public class GeneratedTagHelperContext
    {
        public static readonly GeneratedTagHelperContext Default =
            new GeneratedTagHelperContext();

        // TODO: Follow same pattern as GeneratedClassContext for constructors etc.
        // when code review names have been finalized.

        public GeneratedTagHelperContext()
        {
            InstantiateTagHelperMethodName = "InstantiateTagHelper";
            ExecuteTagHelpersAsyncMethodName = "ExecuteTagHelpersAsync";
            StartTagHelpersScope = "StartTagHelpersScope";
            EndTagHelpersScope = "EndTagHelpersScope";
            GenerateTagStartMethodName = "GenerateTagStart";
            GenerateTagContentMethodName = "GenerateTagContent";
            GenerateTagEndMethodName = "GenerateTagEnd";
            GetContentBuffer = "GetContentBuffer";
            AddTagHelperAttributeMethodName = "AddTagHelperAttribute";
            AddHtmlAttributeMethodName = "AddHtmlAttribute";
            TagHelperManagerName = "TagHelperManager";
            NewWritingScopeMethodName = "NewWritingScope";
            EndWritingScopeMethodName = "EndWritingScope";
        }

        public string InstantiateTagHelperMethodName { get; set; }
        public string ExecuteTagHelpersAsyncMethodName { get; set; }
        public string StartTagHelpersScope { get; set; }
        public string EndTagHelpersScope { get; set; }
        public string GenerateTagStartMethodName { get; set; }
        public string GenerateTagContentMethodName { get; set; }
        public string GenerateTagEndMethodName { get; set; }
        public string GetContentBuffer { get; set; }
        public string AddTagHelperAttributeMethodName { get; set; }
        public string AddHtmlAttributeMethodName { get; set; }
        public string TagHelperManagerName { get; set; }
        public string NewWritingScopeMethodName { get; set; }
        public string EndWritingScopeMethodName { get; set; }
    }
}