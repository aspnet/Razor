#pragma checksum "PrefixedAttributeTagHelpers.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "ad6b545da741ad711339a34893d12d2da6f945c6"
namespace TestOutput
{
    using Microsoft.AspNet.Razor.Runtime.TagHelpers;
    using System;
    using System.Threading.Tasks;

    public class PrefixedAttributeTagHelpers
    {
        #line hidden
        #pragma warning disable 0414
        private TagHelperContent __tagHelperStringValueBuffer = null;
        #pragma warning restore 0414
        private TagHelperExecutionContext __tagHelperExecutionContext = null;
        private TagHelperRunner __tagHelperRunner = null;
        private TagHelperScopeManager __tagHelperScopeManager = new TagHelperScopeManager();
        private InputTagHelper2 __InputTagHelper2 = null;
        private InputTagHelper1 __InputTagHelper1 = null;
        #line hidden
        public PrefixedAttributeTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __tagHelperRunner = __tagHelperRunner ?? new TagHelperRunner();
            Instrumentation.BeginContext(33, 2, true);
            WriteLiteral("\r\n");
            Instrumentation.EndContext();
#line 3 "PrefixedAttributeTagHelpers.cshtml"
   
    var intDictionary = new Dictionary<string, int>
    {
        { "three", 3 },
    };
    var stringDictionary = new SortedDictionary<string, string>
    {
        { "name", "value" },
    };

#line default
#line hidden

            Instrumentation.BeginContext(244, 51, true);
            WriteLiteral("\r\n\r\n<div class=\"randomNonTagHelperAttribute\">\r\n    ");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", true, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __tagHelperExecutionContext.Add(__InputTagHelper2);
            if (__InputTagHelper2.IntDictionaryProperty == null)
            {
                __InputTagHelper2.IntDictionaryProperty = new System.Collections.Generic.Dictionary<string, int>();
            }
#line 15 "PrefixedAttributeTagHelpers.cshtml"
 __InputTagHelper2.IntDictionaryProperty = intDictionary;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-dictionary", __InputTagHelper2.IntDictionaryProperty);
#line 15 "PrefixedAttributeTagHelpers.cshtml"
                                __InputTagHelper2.StringDictionaryProperty = stringDictionary;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("string-dictionary", __InputTagHelper2.StringDictionaryProperty);
            __InputTagHelper1 = CreateTagHelper<InputTagHelper1>();
            __tagHelperExecutionContext.Add(__InputTagHelper1);
            if (__InputTagHelper1.IntDictionaryProperty == null)
            {
                __InputTagHelper1.IntDictionaryProperty = new System.Collections.Generic.Dictionary<string, int>();
            }
            __InputTagHelper1.IntDictionaryProperty = __InputTagHelper2.IntDictionaryProperty;
            __InputTagHelper1.StringDictionaryProperty = __InputTagHelper2.StringDictionaryProperty;
            __tagHelperExecutionContext.AddHtmlAttribute("type", Html.Raw("checkbox"));
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(387, 6, true);
            WriteLiteral("\r\n    ");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", true, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __tagHelperExecutionContext.Add(__InputTagHelper2);
            if (__InputTagHelper2.IntDictionaryProperty == null)
            {
                __InputTagHelper2.IntDictionaryProperty = new System.Collections.Generic.Dictionary<string, int>();
            }
#line 16 "PrefixedAttributeTagHelpers.cshtml"
 __InputTagHelper2.IntDictionaryProperty = intDictionary;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-dictionary", __InputTagHelper2.IntDictionaryProperty);
#line 16 "PrefixedAttributeTagHelpers.cshtml"
                         __InputTagHelper2.IntDictionaryProperty[""] = 37;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-", __InputTagHelper2.IntDictionaryProperty[""]);
#line 16 "PrefixedAttributeTagHelpers.cshtml"
                                          __InputTagHelper2.IntDictionaryProperty["grabber"] = 42;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-grabber", __InputTagHelper2.IntDictionaryProperty["grabber"]);
            __InputTagHelper1 = CreateTagHelper<InputTagHelper1>();
            __tagHelperExecutionContext.Add(__InputTagHelper1);
            if (__InputTagHelper1.IntDictionaryProperty == null)
            {
                __InputTagHelper1.IntDictionaryProperty = new System.Collections.Generic.Dictionary<string, int>();
            }
            __InputTagHelper1.IntProperty = __InputTagHelper2.IntDictionaryProperty["grabber"];
            __InputTagHelper1.IntDictionaryProperty = __InputTagHelper2.IntDictionaryProperty;
            __InputTagHelper1.IntDictionaryProperty[""] = __InputTagHelper2.IntDictionaryProperty[""];
            __tagHelperExecutionContext.AddHtmlAttribute("type", Html.Raw("password"));
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(490, 6, true);
            WriteLiteral("\r\n    ");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", true, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __tagHelperExecutionContext.Add(__InputTagHelper2);
            if (__InputTagHelper2.IntDictionaryProperty == null)
            {
                __InputTagHelper2.IntDictionaryProperty = new System.Collections.Generic.Dictionary<string, int>();
            }
#line 18 "PrefixedAttributeTagHelpers.cshtml"
__InputTagHelper2.IntDictionaryProperty["grabber"] = 42;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-grabber", __InputTagHelper2.IntDictionaryProperty["grabber"]);
#line 18 "PrefixedAttributeTagHelpers.cshtml"
  __InputTagHelper2.IntDictionaryProperty[""] = 37;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-", __InputTagHelper2.IntDictionaryProperty[""]);
            __InputTagHelper2.StringDictionaryProperty["grabber"] = "string";
            __tagHelperExecutionContext.AddTagHelperAttribute("string-prefix-grabber", __InputTagHelper2.StringDictionaryProperty["grabber"]);
            __InputTagHelper2.StringDictionaryProperty["value"] = "another string";
            __tagHelperExecutionContext.AddTagHelperAttribute("string-prefix-value", __InputTagHelper2.StringDictionaryProperty["value"]);
            __InputTagHelper1 = CreateTagHelper<InputTagHelper1>();
            __tagHelperExecutionContext.Add(__InputTagHelper1);
            if (__InputTagHelper1.IntDictionaryProperty == null)
            {
                __InputTagHelper1.IntDictionaryProperty = new System.Collections.Generic.Dictionary<string, int>();
            }
            __InputTagHelper1.IntProperty = __InputTagHelper2.IntDictionaryProperty["grabber"];
            __InputTagHelper1.StringProperty = __InputTagHelper2.StringDictionaryProperty["grabber"];
            __InputTagHelper1.IntDictionaryProperty[""] = __InputTagHelper2.IntDictionaryProperty[""];
            __InputTagHelper1.StringDictionaryProperty["value"] = __InputTagHelper2.StringDictionaryProperty["value"];
            __tagHelperExecutionContext.AddHtmlAttribute("type", Html.Raw("radio"));
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(650, 6, true);
            WriteLiteral("\r\n    ");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", true, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __tagHelperExecutionContext.Add(__InputTagHelper2);
            if (__InputTagHelper2.IntDictionaryProperty == null)
            {
                __InputTagHelper2.IntDictionaryProperty = new System.Collections.Generic.Dictionary<string, int>();
            }
#line 20 "PrefixedAttributeTagHelpers.cshtml"
__InputTagHelper2.IntDictionaryProperty["value"] = 37;

#line default
#line hidden
            __tagHelperExecutionContext.AddTagHelperAttribute("int-prefix-value", __InputTagHelper2.IntDictionaryProperty["value"]);
            __InputTagHelper2.StringDictionaryProperty["value"] = "string";
            __tagHelperExecutionContext.AddTagHelperAttribute("string-prefix-value", __InputTagHelper2.StringDictionaryProperty["value"]);
            __InputTagHelper1 = CreateTagHelper<InputTagHelper1>();
            __tagHelperExecutionContext.Add(__InputTagHelper1);
            if (__InputTagHelper1.IntDictionaryProperty == null)
            {
                __InputTagHelper1.IntDictionaryProperty = new System.Collections.Generic.Dictionary<string, int>();
            }
            __InputTagHelper1.IntDictionaryProperty["value"] = __InputTagHelper2.IntDictionaryProperty["value"];
            __InputTagHelper1.StringDictionaryProperty["value"] = __InputTagHelper2.StringDictionaryProperty["value"];
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(716, 8, true);
            WriteLiteral("\r\n</div>");
            Instrumentation.EndContext();
        }
        #pragma warning restore 1998
    }
}
