#pragma checksum "DuplicateTargetTagHelper.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "77e70e938d77f71e489354b1e7c351c588001690"
namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class DuplicateTargetTagHelper
    {
        #line hidden
        #pragma warning disable 0414
        private Microsoft.AspNet.Razor.TagHelperContent __tagHelperStringValueBuffer = null;
        #pragma warning restore 0414
        private Microsoft.AspNet.Razor.Runtime.TagHelperExecutionContext __tagHelperExecutionContext = null;
        private Microsoft.AspNet.Razor.Runtime.TagHelperRunner __tagHelperRunner = null;
        private Microsoft.AspNet.Razor.Runtime.TagHelperScopeManager __tagHelperScopeManager = new Microsoft.AspNet.Razor.Runtime.TagHelperScopeManager();
        private InputTagHelper __InputTagHelper = null;
        private CatchAllTagHelper __CatchAllTagHelper = null;
        #line hidden
        public DuplicateTargetTagHelper()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __tagHelperRunner = __tagHelperRunner ?? new Microsoft.AspNet.Razor.Runtime.TagHelperRunner();
            Instrumentation.BeginContext(33, 2, true);
            WriteLiteral("\r\n");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", Microsoft.AspNet.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __tagHelperExecutionContext.Add(__InputTagHelper);
            __CatchAllTagHelper = CreateTagHelper<CatchAllTagHelper>();
            __tagHelperExecutionContext.Add(__CatchAllTagHelper);
            __InputTagHelper.Type = "checkbox";
            __tagHelperExecutionContext.AddTagHelperAttribute("type", __InputTagHelper.Type);
            __CatchAllTagHelper.Type = __InputTagHelper.Type;
#line 3 "DuplicateTargetTagHelper.cshtml"
     __InputTagHelper.Checked = true;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("checked", __InputTagHelper.Checked);
            __CatchAllTagHelper.Checked = __InputTagHelper.Checked;
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(35, 40, false);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
        }
        #pragma warning restore 1998
    }
}
