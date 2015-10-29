#pragma checksum "MinimizedTagHelpers.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "07839be4304797e30b19b50b95e2247c93cdff06"
namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class MinimizedTagHelpers
    {
        #line hidden
        #pragma warning disable 0414
        private global::Microsoft.AspNet.Razor.TagHelperContent __tagHelperStringValueBuffer = null;
        #pragma warning restore 0414
        private global::Microsoft.AspNet.Razor.Runtime.TagHelperExecutionContext __tagHelperExecutionContext = null;
        private global::Microsoft.AspNet.Razor.Runtime.TagHelperRunner __tagHelperRunner = null;
        private global::Microsoft.AspNet.Razor.Runtime.TagHelperScopeManager __tagHelperScopeManager = new global::Microsoft.AspNet.Razor.Runtime.TagHelperScopeManager();
        private global::CatchAllTagHelper __CatchAllTagHelper = null;
        private global::InputTagHelper __InputTagHelper = null;
        #line hidden
        public MinimizedTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __tagHelperRunner = __tagHelperRunner ?? new global::Microsoft.AspNet.Razor.Runtime.TagHelperRunner();
            Instrumentation.BeginContext(33, 2, true);
            WriteLiteral("\r\n");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("p", global::Microsoft.AspNet.Razor.TagHelpers.TagMode.StartTagAndEndTag, "test", async() => {
                Instrumentation.BeginContext(64, 34, true);
                WriteLiteral("\r\n    <input nottaghelper />\r\n    ");
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNet.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
                }
                , StartTagHelperWritingScope, EndTagHelperWritingScope);
                __CatchAllTagHelper = CreateTagHelper<global::CatchAllTagHelper>();
                __tagHelperExecutionContext.Add(__CatchAllTagHelper);
                __tagHelperExecutionContext.AddHtmlAttribute("class", Html.Raw("btn"));
                __tagHelperExecutionContext.AddMinimizedHtmlAttribute("catchall-unbound-required");
                __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                Instrumentation.BeginContext(98, 59, false);
                await WriteTagHelperAsync(__tagHelperExecutionContext);
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                Instrumentation.BeginContext(157, 6, true);
                WriteLiteral("\r\n    ");
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNet.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
                }
                , StartTagHelperWritingScope, EndTagHelperWritingScope);
                __InputTagHelper = CreateTagHelper<global::InputTagHelper>();
                __tagHelperExecutionContext.Add(__InputTagHelper);
                __CatchAllTagHelper = CreateTagHelper<global::CatchAllTagHelper>();
                __tagHelperExecutionContext.Add(__CatchAllTagHelper);
                __tagHelperExecutionContext.AddHtmlAttribute("class", Html.Raw("btn"));
                __tagHelperExecutionContext.AddMinimizedHtmlAttribute("catchall-unbound-required");
                __tagHelperExecutionContext.AddMinimizedHtmlAttribute("input-unbound-required");
                __InputTagHelper.BoundRequiredString = "hello";
                __tagHelperExecutionContext.AddTagHelperAttribute("input-bound-required-string", __InputTagHelper.BoundRequiredString);
                __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                Instrumentation.BeginContext(163, 119, false);
                await WriteTagHelperAsync(__tagHelperExecutionContext);
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                Instrumentation.BeginContext(282, 6, true);
                WriteLiteral("\r\n    ");
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNet.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
                }
                , StartTagHelperWritingScope, EndTagHelperWritingScope);
                __InputTagHelper = CreateTagHelper<global::InputTagHelper>();
                __tagHelperExecutionContext.Add(__InputTagHelper);
                __CatchAllTagHelper = CreateTagHelper<global::CatchAllTagHelper>();
                __tagHelperExecutionContext.Add(__CatchAllTagHelper);
                __tagHelperExecutionContext.AddHtmlAttribute("class", Html.Raw("btn"));
                __tagHelperExecutionContext.AddMinimizedHtmlAttribute("catchall-unbound-required");
                __tagHelperExecutionContext.AddMinimizedHtmlAttribute("input-unbound-required");
                __CatchAllTagHelper.BoundRequiredString = "world";
                __tagHelperExecutionContext.AddTagHelperAttribute("catchall-bound-string", __CatchAllTagHelper.BoundRequiredString);
                __InputTagHelper.BoundRequiredString = "hello2";
                __tagHelperExecutionContext.AddTagHelperAttribute("input-bound-required-string", __InputTagHelper.BoundRequiredString);
                __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                Instrumentation.BeginContext(288, 176, false);
                await WriteTagHelperAsync(__tagHelperExecutionContext);
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                Instrumentation.BeginContext(464, 6, true);
                WriteLiteral("\r\n    ");
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", global::Microsoft.AspNet.Razor.TagHelpers.TagMode.SelfClosing, "test", async() => {
                }
                , StartTagHelperWritingScope, EndTagHelperWritingScope);
                __InputTagHelper = CreateTagHelper<global::InputTagHelper>();
                __tagHelperExecutionContext.Add(__InputTagHelper);
                __CatchAllTagHelper = CreateTagHelper<global::CatchAllTagHelper>();
                __tagHelperExecutionContext.Add(__CatchAllTagHelper);
                __tagHelperExecutionContext.AddHtmlAttribute("class", Html.Raw("btn"));
                __tagHelperExecutionContext.AddHtmlAttribute("catchall-unbound-required", Html.Raw("hello"));
                __tagHelperExecutionContext.AddHtmlAttribute("input-unbound-required", Html.Raw("hello2"));
                __tagHelperExecutionContext.AddMinimizedHtmlAttribute("catchall-unbound-required");
                __InputTagHelper.BoundRequiredString = "world";
                __tagHelperExecutionContext.AddTagHelperAttribute("input-bound-required-string", __InputTagHelper.BoundRequiredString);
                __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                Instrumentation.BeginContext(470, 206, false);
                await WriteTagHelperAsync(__tagHelperExecutionContext);
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                Instrumentation.BeginContext(676, 2, true);
                WriteLiteral("\r\n");
                Instrumentation.EndContext();
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __CatchAllTagHelper = CreateTagHelper<global::CatchAllTagHelper>();
            __tagHelperExecutionContext.Add(__CatchAllTagHelper);
            __tagHelperExecutionContext.AddMinimizedHtmlAttribute("catchall-unbound-required");
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(35, 647, false);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
        }
        #pragma warning restore 1998
    }
}
