#pragma checksum "TagHelpersInSection.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "b6492c68360b85d4993de94eeac547b7fb012a26"
namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class TagHelpersInSection
    {
        #line hidden
        #pragma warning disable 0414
        private global::Microsoft.AspNet.Razor.TagHelperContent __tagHelperStringValueBuffer = null;
        #pragma warning restore 0414
        private global::Microsoft.AspNet.Razor.Runtime.TagHelperExecutionContext __tagHelperExecutionContext = null;
        private global::Microsoft.AspNet.Razor.Runtime.TagHelperRunner __tagHelperRunner = null;
        private global::Microsoft.AspNet.Razor.Runtime.TagHelperScopeManager __tagHelperScopeManager = new global::Microsoft.AspNet.Razor.Runtime.TagHelperScopeManager();
        private global::MyTagHelper __MyTagHelper = null;
        private global::NestedTagHelper __NestedTagHelper = null;
        #line hidden
        public TagHelpersInSection()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __tagHelperRunner = __tagHelperRunner ?? new global::Microsoft.AspNet.Razor.Runtime.TagHelperRunner();
            Instrumentation.BeginContext(33, 2, true);
            WriteLiteral("\r\n");
            Instrumentation.EndContext();
#line 3 "TagHelpersInSection.cshtml"
  
    var code = "some code";

#line default
#line hidden

            Instrumentation.BeginContext(71, 2, true);
            WriteLiteral("\r\n");
            Instrumentation.EndContext();
            DefineSection("MySection", async(__razor_template_writer) => {
                Instrumentation.BeginContext(93, 21, true);
                WriteLiteralTo(__razor_template_writer, "\r\n    <div>\r\n        ");
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.Begin("mytaghelper", global::Microsoft.AspNet.Razor.TagHelpers.TagMode.StartTagAndEndTag, "test", async() => {
                    Instrumentation.BeginContext(217, 52, true);
                    WriteLiteral("\r\n            In None ContentBehavior.\r\n            ");
                    Instrumentation.EndContext();
                    __tagHelperExecutionContext = __tagHelperScopeManager.Begin("nestedtaghelper", global::Microsoft.AspNet.Razor.TagHelpers.TagMode.StartTagAndEndTag, "test", async() => {
                        Instrumentation.BeginContext(286, 26, true);
                        WriteLiteral("Some buffered values with ");
                        Instrumentation.EndContext();
                        Instrumentation.BeginContext(313, 4, false);
#line 11 "TagHelpersInSection.cshtml"
                                                  Write(code);

#line default
#line hidden
                        Instrumentation.EndContext();
                    }
                    , StartTagHelperWritingScope, EndTagHelperWritingScope);
                    __NestedTagHelper = CreateTagHelper<global::NestedTagHelper>();
                    __tagHelperExecutionContext.Add(__NestedTagHelper);
                    __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                    Instrumentation.BeginContext(269, 66, false);
                    await WriteTagHelperAsync(__tagHelperExecutionContext);
                    Instrumentation.EndContext();
                    __tagHelperExecutionContext = __tagHelperScopeManager.End();
                    Instrumentation.BeginContext(335, 10, true);
                    WriteLiteral("\r\n        ");
                    Instrumentation.EndContext();
                }
                , StartTagHelperWritingScope, EndTagHelperWritingScope);
                __MyTagHelper = CreateTagHelper<global::MyTagHelper>();
                __tagHelperExecutionContext.Add(__MyTagHelper);
                StartTagHelperWritingScope();
                WriteLiteral("Current Time: ");
#line 9 "TagHelpersInSection.cshtml"
                                      WriteLiteral(DateTime.Now);

#line default
#line hidden
                __tagHelperStringValueBuffer = EndTagHelperWritingScope();
                __MyTagHelper.BoundProperty = __tagHelperStringValueBuffer.GetContent(HtmlEncoder);
                __tagHelperExecutionContext.AddTagHelperAttribute("boundproperty", __MyTagHelper.BoundProperty);
                BeginAddHtmlAttributeValues(__tagHelperExecutionContext, "unboundproperty", 3);
                AddHtmlAttributeValue("", 188, "Current", 188, 7, true);
                AddHtmlAttributeValue(" ", 195, "Time:", 196, 6, true);
#line 9 "TagHelpersInSection.cshtml"
AddHtmlAttributeValue(" ", 201, DateTime.Now, 202, 14, false);

#line default
#line hidden
                EndAddHtmlAttributeValues(__tagHelperExecutionContext);
                __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
                Instrumentation.BeginContext(114, 245, false);
                await WriteTagHelperToAsync(__razor_template_writer, __tagHelperExecutionContext);
                Instrumentation.EndContext();
                __tagHelperExecutionContext = __tagHelperScopeManager.End();
                Instrumentation.BeginContext(359, 14, true);
                WriteLiteralTo(__razor_template_writer, "\r\n    </div>\r\n");
                Instrumentation.EndContext();
            }
            );
        }
        #pragma warning restore 1998
    }
}
