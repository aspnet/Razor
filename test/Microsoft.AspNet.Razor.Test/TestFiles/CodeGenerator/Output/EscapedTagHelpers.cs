#pragma checksum "EscapedTagHelpers.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "dcdd88b4f52fa03367f94849021a84a290cb3c1e"
namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class EscapedTagHelpers
    {
        #line hidden
        #pragma warning disable 0414
        private global::Microsoft.AspNet.Razor.TagHelperContent __tagHelperStringValueBuffer = null;
        #pragma warning restore 0414
        private global::Microsoft.AspNet.Razor.Runtime.TagHelperExecutionContext __tagHelperExecutionContext = null;
        private global::Microsoft.AspNet.Razor.Runtime.TagHelperRunner __tagHelperRunner = null;
        private global::Microsoft.AspNet.Razor.Runtime.TagHelperScopeManager __tagHelperScopeManager = new global::Microsoft.AspNet.Razor.Runtime.TagHelperScopeManager();
        private global::InputTagHelper __InputTagHelper = null;
        private global::InputTagHelper2 __InputTagHelper2 = null;
        #line hidden
        public EscapedTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __tagHelperRunner = __tagHelperRunner ?? new global::Microsoft.AspNet.Razor.Runtime.TagHelperRunner();
            Instrumentation.BeginContext(27, 72, true);
            WriteLiteral("\r\n<div class=\"randomNonTagHelperAttribute\">\r\n    <p class=\"Hello World\" ");
            Instrumentation.EndContext();
            Instrumentation.BeginContext(102, 12, false);
#line 4 "EscapedTagHelpers.cshtml"
                       Write(DateTime.Now);

#line default
#line hidden
            Instrumentation.EndContext();
            Instrumentation.BeginContext(114, 69, true);
            WriteLiteral(">\r\n        <input type=\"text\" />\r\n        <em>Not a TagHelper: </em> ");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNet.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper = CreateTagHelper<global::InputTagHelper>();
            __tagHelperExecutionContext.Add(__InputTagHelper);
            __InputTagHelper2 = CreateTagHelper<global::InputTagHelper2>();
            __tagHelperExecutionContext.Add(__InputTagHelper2);
            StartTagHelperWritingScope();
#line 6 "EscapedTagHelpers.cshtml"
                                      WriteLiteral(DateTime.Now);

#line default
#line hidden
            __tagHelperStringValueBuffer = EndTagHelperWritingScope();
            __InputTagHelper.Type = __tagHelperStringValueBuffer.GetContent(HtmlEncoder);
            __tagHelperExecutionContext.AddTagHelperAttribute("type", __InputTagHelper.Type);
            __InputTagHelper2.Type = __InputTagHelper.Type;
#line 6 "EscapedTagHelpers.cshtml"
                                              __InputTagHelper2.Checked = true;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("checked", __InputTagHelper2.Checked);
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(186, 45, false);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(231, 18, true);
            WriteLiteral("\r\n    </p>\r\n</div>");
            Instrumentation.EndContext();
        }
        #pragma warning restore 1998
    }
}
