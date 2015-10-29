#pragma checksum "AttributeTargetingTagHelpers.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "edaa271f830648a99892d01bb55f49e328fa621c"
namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class AttributeTargetingTagHelpers
    {
        #line hidden
        #pragma warning disable 0414
        private Microsoft.AspNet.Razor.TagHelperContent __tagHelperStringValueBuffer = null;
        #pragma warning restore 0414
        private Microsoft.AspNet.Razor.Runtime.TagHelperExecutionContext __tagHelperExecutionContext = null;
        private Microsoft.AspNet.Razor.Runtime.TagHelperRunner __tagHelperRunner = null;
        private Microsoft.AspNet.Razor.Runtime.TagHelperScopeManager __tagHelperScopeManager = new Microsoft.AspNet.Razor.Runtime.TagHelperScopeManager();
        private PTagHelper __PTagHelper = null;
        private CatchAllTagHelper __CatchAllTagHelper = null;
        private InputTagHelper __InputTagHelper = null;
        private InputTagHelper2 __InputTagHelper2 = null;
        #line hidden
        public AttributeTargetingTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __tagHelperRunner = __tagHelperRunner ?? new Microsoft.AspNet.Razor.Runtime.TagHelperRunner();
            Instrumentation.BeginContext(30, 2, true);
            WriteLiteral("\r\n");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("p", Microsoft.AspNet.Razor.TagHelpers.TagMode.StartTagAndEndTag, "test", async() => {
                Instrumentation.BeginContext(47, 9, true);
                WriteLiteral("\r\n    <p>");
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("strong", Microsoft.AspNet.Razor.TagHelpers.TagMode.StartTagAndEndTag, "test", async() => {
                    Instrumentation.BeginContext(78, 5, true);
                    WriteLiteral("Hello");
                    Instrumentation.EndContext();
                }
                , StartTagHelperWritingScope, EndTagHelperWritingScope);
                __CatchAllTagHelper = CreateTagHelper<CatchAllTagHelper>();
                __tagHelperExecutionContext.Add(__CatchAllTagHelper);
                __tagHelperExecutionContext.AddHtmlAttribute("catchAll", Html.Raw("hi"));
                __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                Instrumentation.BeginContext(56, 36, false);
                await WriteTagHelperAsync(__tagHelperExecutionContext);
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                Instrumentation.BeginContext(92, 62, true);
                WriteLiteral("<strong>World</strong></p>\r\n    <input checked=\"true\" />\r\n    ");
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", Microsoft.AspNet.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
                }
                , StartTagHelperWritingScope, EndTagHelperWritingScope);
                __InputTagHelper = CreateTagHelper<InputTagHelper>();
                __tagHelperExecutionContext.Add(__InputTagHelper);
                __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
                __tagHelperExecutionContext.Add(__InputTagHelper2);
                __InputTagHelper.Type = "checkbox";
                __tagHelperExecutionContext.AddTagHelperAttribute("type", __InputTagHelper.Type);
                __InputTagHelper2.Type = __InputTagHelper.Type;
#line 6 "AttributeTargetingTagHelpers.cshtml"
        __InputTagHelper2.Checked = true;

#line default
#line hidden
                __tagHelperExecutionContext.AddTagHelperAttribute("checked", __InputTagHelper2.Checked);
                __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                Instrumentation.BeginContext(154, 40, false);
                await WriteTagHelperAsync(__tagHelperExecutionContext);
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                Instrumentation.BeginContext(194, 6, true);
                WriteLiteral("\r\n    ");
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", Microsoft.AspNet.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
                }
                , StartTagHelperWritingScope, EndTagHelperWritingScope);
                __InputTagHelper = CreateTagHelper<InputTagHelper>();
                __tagHelperExecutionContext.Add(__InputTagHelper);
                __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
                __tagHelperExecutionContext.Add(__InputTagHelper2);
                __CatchAllTagHelper = CreateTagHelper<CatchAllTagHelper>();
                __tagHelperExecutionContext.Add(__CatchAllTagHelper);
                __InputTagHelper.Type = "checkbox";
                __tagHelperExecutionContext.AddTagHelperAttribute("type", __InputTagHelper.Type);
                __InputTagHelper2.Type = __InputTagHelper.Type;
#line 7 "AttributeTargetingTagHelpers.cshtml"
        __InputTagHelper2.Checked = true;

#line default
#line hidden
                __tagHelperExecutionContext.AddTagHelperAttribute("checked", __InputTagHelper2.Checked);
                __tagHelperExecutionContext.AddHtmlAttribute("catchAll", Html.Raw("hi"));
                __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                Instrumentation.BeginContext(200, 54, false);
                await WriteTagHelperAsync(__tagHelperExecutionContext);
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                Instrumentation.BeginContext(254, 2, true);
                WriteLiteral("\r\n");
                Instrumentation.EndContext();
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __PTagHelper = CreateTagHelper<PTagHelper>();
            __tagHelperExecutionContext.Add(__PTagHelper);
            __tagHelperExecutionContext.AddHtmlAttribute("class", Html.Raw("btn"));
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(32, 228, false);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
        }
        #pragma warning restore 1998
    }
}
