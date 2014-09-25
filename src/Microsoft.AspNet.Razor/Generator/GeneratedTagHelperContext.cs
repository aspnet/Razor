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
            CreateTagHelperMethodName = "CreateTagHelper";
            TagHelperRunnerRunAsyncMethodName = "RunAsync";
            TagHelperScopeManagerBeginMethodName = "Begin";
            TagHelperScopeManagerEndMethodName = "End";
            TagHelperOutputGenerateTagStartMethodName = "GenerateTagStart";
            TagHelperOutputGenerateTagContentMethodName = "GenerateTagContent";
            TagHelperOutputGenerateTagEndMethodName = "GenerateTagEnd";
            ExecutionContextAddMethodName = "Add";
            ExecutionContextAddTagHelperAttributeMethodName = "AddTagHelperAttribute";
            ExecutionContextAddHtmlAttributeMethodName = "AddHtmlAttribute";
            TagHelperRunnerTypeName = "ITagHelperRunner";
            TagHelperScopeManagerTypeName = "ITagHelperScopeManager";
            TagHelperExecutionContextTypeName = "TagHelperExecutionContext";
            ExecutionContextOutputPropertyName = "Output";
            StartWritingScopeMethodName = "StartWritingScope";
            EndWritingScopeMethodName = "EndWritingScope";
        }

        public string CreateTagHelperMethodName { get; set; }
        public string TagHelperRunnerRunAsyncMethodName { get; set; }
        public string TagHelperScopeManagerBeginMethodName { get; set; }
        public string TagHelperScopeManagerEndMethodName { get; set; }
        public string TagHelperOutputGenerateTagStartMethodName { get; set; }
        public string TagHelperOutputGenerateTagContentMethodName { get; set; }
        public string TagHelperOutputGenerateTagEndMethodName { get; set; }
        public string ExecutionContextAddTagHelperAttributeMethodName { get; set; }
        public string ExecutionContextAddHtmlAttributeMethodName { get; set; }
        public string TagHelperRunnerTypeName { get; set; }
        public string ExecutionContextAddMethodName { get; set; }
        public string TagHelperScopeManagerTypeName { get; set; }
        public string TagHelperExecutionContextTypeName { get; set; }
        public string ExecutionContextOutputPropertyName { get; set; }
        public string StartWritingScopeMethodName { get; set; }
        public string EndWritingScopeMethodName { get; set; }
    }
}