#pragma checksum "EscapedTagHelpers.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "93da17a7091c4d218cfc54282dec1b7b7beac072"
namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class EscapedTagHelpers
    {
        #line hidden
        #pragma warning disable 0414
        private string __tagHelperStringValueBuffer = null;
        #pragma warning restore 0414
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelperExecutionContext __tagHelperExecutionContext = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelperRunner __tagHelperRunner = null;
        private global::Microsoft.AspNetCore.Razor.Runtime.TagHelperScopeManager __tagHelperScopeManager = null;
        private global::TestNamespace.InputTagHelper __TestNamespace_InputTagHelper = null;
        private global::TestNamespace.InputTagHelper2 __TestNamespace_InputTagHelper2 = null;
        #line hidden
        public EscapedTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async System.Threading.Tasks.Task ExecuteAsync()
        {
            __tagHelperRunner = __tagHelperRunner ?? new global::Microsoft.AspNetCore.Razor.Runtime.TagHelperRunner();
            __tagHelperScopeManager = __tagHelperScopeManager ?? new global::Microsoft.AspNetCore.Razor.Runtime.TagHelperScopeManager(StartTagHelperWritingScope, EndTagHelperWritingScope);
            Instrumentation.BeginContext(25, 72, true);
            WriteLiteral("\r\n<div class=\"randomNonTagHelperAttribute\">\r\n    <p class=\"Hello World\" ");
            Instrumentation.EndContext();
            Instrumentation.BeginContext(100, 12, false);
#line 4 "EscapedTagHelpers.cshtml"
                       Write(DateTime.Now);

#line default
#line hidden
            Instrumentation.EndContext();
            Instrumentation.BeginContext(112, 69, true);
            WriteLiteral(">\r\n        <input type=\"text\" />\r\n        <em>Not a TagHelper: </em> ");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNetCore.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
            }
            );
            __TestNamespace_InputTagHelper = CreateTagHelper<global::TestNamespace.InputTagHelper>();
            __tagHelperExecutionContext.Add(__TestNamespace_InputTagHelper);
            __TestNamespace_InputTagHelper2 = CreateTagHelper<global::TestNamespace.InputTagHelper2>();
            __tagHelperExecutionContext.Add(__TestNamespace_InputTagHelper2);
            BeginWriteTagHelperAttribute();
#line 6 "EscapedTagHelpers.cshtml"
                                      WriteLiteral(DateTime.Now);

#line default
#line hidden
            __tagHelperStringValueBuffer = EndWriteTagHelperAttribute();
            __TestNamespace_InputTagHelper.Type = __tagHelperStringValueBuffer;
            __tagHelperExecutionContext.AddTagHelperAttribute("type", __TestNamespace_InputTagHelper.Type, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            __TestNamespace_InputTagHelper2.Type = __TestNamespace_InputTagHelper.Type;
#line 6 "EscapedTagHelpers.cshtml"
                                __TestNamespace_InputTagHelper2.Checked = true;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("checked", __TestNamespace_InputTagHelper2.Checked, global::Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeValueStyle.DoubleQuotes);
            await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(184, 45, false);
            Write(__tagHelperExecutionContext.Output);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(229, 18, true);
            WriteLiteral("\r\n    </p>\r\n</div>");
            Instrumentation.EndContext();
        }
        #pragma warning restore 1998
    }
}
