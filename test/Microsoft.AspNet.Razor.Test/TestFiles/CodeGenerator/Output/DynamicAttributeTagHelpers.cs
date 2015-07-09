#pragma checksum "DynamicAttributeTagHelpers.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "40990f364c71b55cb3888667144e8558c1645fa4"
namespace TestOutput
{
    using Microsoft.AspNet.Razor.Runtime.TagHelpers;
    using System;
    using System.Threading.Tasks;

    public class DynamicAttributeTagHelpers
    {
        #line hidden
        #pragma warning disable 0414
        private TagHelperContent __tagHelperStringValueBuffer = null;
        #pragma warning restore 0414
        private TagHelperExecutionContext __tagHelperExecutionContext = null;
        private TagHelperRunner __tagHelperRunner = null;
        private TagHelperScopeManager __tagHelperScopeManager = new TagHelperScopeManager();
        private StringCollectionTextWriter __originalTagHelperAttributeValue = null;
        private object __rawTagHelperAttributeValue = null;
        private bool __shouldRenderTagHelperAttribute = false;
        private InputTagHelper __InputTagHelper = null;
        #line hidden
        public DynamicAttributeTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __tagHelperRunner = __tagHelperRunner ?? new TagHelperRunner();
            Instrumentation.BeginContext(33, 4, true);
            WriteLiteral("\r\n\r\n");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", true, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __tagHelperExecutionContext.Add(__InputTagHelper);
            StartTagHelperWritingScope();
            __originalTagHelperAttributeValue = new StringCollectionTextWriter(Output.Encoding);
            WriteLiteral("prefix");
            WriteLiteralTo(__originalTagHelperAttributeValue, "prefix");
            __rawTagHelperAttributeValue = DateTime.Now;
            if (ShouldRenderAttributeValue(__rawTagHelperAttributeValue))
            {
                WriteLiteral(" ");
                WriteUnprefixedAttributeValueTo(Output, GetStringAttributeValue("unbound", __rawTagHelperAttributeValue), __rawTagHelperAttributeValue, false);
            }
            WriteLiteralTo(__originalTagHelperAttributeValue, " ");
            WriteTo(__originalTagHelperAttributeValue, __rawTagHelperAttributeValue);
            __tagHelperStringValueBuffer = EndTagHelperWritingScope();
            __tagHelperExecutionContext.HtmlAttributes.Add("unbound", Html.Raw(__tagHelperStringValueBuffer));
            __tagHelperExecutionContext.AddTagHelperAttribute("unbound", Html.Raw(__originalTagHelperAttributeValue));
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(37, 40, false);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(77, 4, true);
            WriteLiteral("\r\n\r\n");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", true, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __tagHelperExecutionContext.Add(__InputTagHelper);
            StartTagHelperWritingScope();
            __originalTagHelperAttributeValue = new StringCollectionTextWriter(Output.Encoding);
            __rawTagHelperAttributeValue = new Template((__razor_attribute_value_writer) => {
#line 6 "DynamicAttributeTagHelpers.cshtml"
if (true) { 

#line default
#line hidden

#line 6 "DynamicAttributeTagHelpers.cshtml"
WriteTo(__razor_attribute_value_writer, string.Empty);

#line default
#line hidden
#line 6 "DynamicAttributeTagHelpers.cshtml"
 } else { 

#line default
#line hidden

#line 6 "DynamicAttributeTagHelpers.cshtml"
WriteTo(__razor_attribute_value_writer, false);

#line default
#line hidden
#line 6 "DynamicAttributeTagHelpers.cshtml"
 }

#line default
#line hidden

            }
            );
            if (ShouldRenderAttributeValue(__rawTagHelperAttributeValue))
            {
                WriteUnprefixedAttributeValueTo(Output, GetStringAttributeValue("unbound", __rawTagHelperAttributeValue), __rawTagHelperAttributeValue, false);
            }
            WriteTo(__originalTagHelperAttributeValue, __rawTagHelperAttributeValue);
            WriteLiteral(" suffix");
            WriteLiteralTo(__originalTagHelperAttributeValue, " suffix");
            __tagHelperStringValueBuffer = EndTagHelperWritingScope();
            __tagHelperExecutionContext.HtmlAttributes.Add("unbound", Html.Raw(__tagHelperStringValueBuffer));
            __tagHelperExecutionContext.AddTagHelperAttribute("unbound", Html.Raw(__originalTagHelperAttributeValue));
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(81, 71, false);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(152, 4, true);
            WriteLiteral("\r\n\r\n");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", true, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __tagHelperExecutionContext.Add(__InputTagHelper);
            StartTagHelperWritingScope();
            WriteLiteral("prefix ");
#line 8 "DynamicAttributeTagHelpers.cshtml"
WriteLiteral(DateTime.Now);

#line default
#line hidden
            WriteLiteral(" suffix");
            __tagHelperStringValueBuffer = EndTagHelperWritingScope();
            __InputTagHelper.Bound = __tagHelperStringValueBuffer.ToString();
            __tagHelperExecutionContext.AddTagHelperAttribute("bound", __InputTagHelper.Bound);
            StartTagHelperWritingScope();
            __originalTagHelperAttributeValue = new StringCollectionTextWriter(Output.Encoding);
            WriteLiteral("prefix");
            WriteLiteralTo(__originalTagHelperAttributeValue, "prefix");
            __rawTagHelperAttributeValue = DateTime.Now;
            if (ShouldRenderAttributeValue(__rawTagHelperAttributeValue))
            {
                WriteLiteral(" ");
                WriteUnprefixedAttributeValueTo(Output, GetStringAttributeValue("unbound", __rawTagHelperAttributeValue), __rawTagHelperAttributeValue, false);
            }
            WriteLiteralTo(__originalTagHelperAttributeValue, " ");
            WriteTo(__originalTagHelperAttributeValue, __rawTagHelperAttributeValue);
            WriteLiteral(" suffix");
            WriteLiteralTo(__originalTagHelperAttributeValue, " suffix");
            __tagHelperStringValueBuffer = EndTagHelperWritingScope();
            __tagHelperExecutionContext.HtmlAttributes.Add("unbound", Html.Raw(__tagHelperStringValueBuffer));
            __tagHelperExecutionContext.AddTagHelperAttribute("unbound", Html.Raw(__originalTagHelperAttributeValue));
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(156, 83, false);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(239, 4, true);
            WriteLiteral("\r\n\r\n");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", true, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __tagHelperExecutionContext.Add(__InputTagHelper);
            StartTagHelperWritingScope();
#line 10 "DynamicAttributeTagHelpers.cshtml"
WriteLiteral(long.MinValue);

#line default
#line hidden
            WriteLiteral(" ");
#line 10 "DynamicAttributeTagHelpers.cshtml"
if (true) { 

#line default
#line hidden

#line 10 "DynamicAttributeTagHelpers.cshtml"
WriteLiteral(string.Empty);

#line default
#line hidden
#line 10 "DynamicAttributeTagHelpers.cshtml"
 } else { 

#line default
#line hidden

#line 10 "DynamicAttributeTagHelpers.cshtml"
WriteLiteral(false);

#line default
#line hidden
#line 10 "DynamicAttributeTagHelpers.cshtml"
 }

#line default
#line hidden

            WriteLiteral(" ");
#line 10 "DynamicAttributeTagHelpers.cshtml"
WriteLiteral(int.MaxValue);

#line default
#line hidden
            __tagHelperStringValueBuffer = EndTagHelperWritingScope();
            __InputTagHelper.Bound = __tagHelperStringValueBuffer.ToString();
            __tagHelperExecutionContext.AddTagHelperAttribute("bound", __InputTagHelper.Bound);
            StartTagHelperWritingScope();
            __shouldRenderTagHelperAttribute = false;
            __originalTagHelperAttributeValue = new StringCollectionTextWriter(Output.Encoding);
            __rawTagHelperAttributeValue = long.MinValue;
            if (ShouldRenderAttributeValue(__rawTagHelperAttributeValue))
            {
                WriteUnprefixedAttributeValueTo(Output, GetStringAttributeValue("unbound", __rawTagHelperAttributeValue), __rawTagHelperAttributeValue, false);
                __shouldRenderTagHelperAttribute = true;
            }
            WriteTo(__originalTagHelperAttributeValue, __rawTagHelperAttributeValue);
            __rawTagHelperAttributeValue = new Template((__razor_attribute_value_writer) => {
#line 11 "DynamicAttributeTagHelpers.cshtml"
if (true) { 

#line default
#line hidden

#line 11 "DynamicAttributeTagHelpers.cshtml"
WriteTo(__razor_attribute_value_writer, string.Empty);

#line default
#line hidden
#line 11 "DynamicAttributeTagHelpers.cshtml"
 } else { 

#line default
#line hidden

#line 11 "DynamicAttributeTagHelpers.cshtml"
WriteTo(__razor_attribute_value_writer, false);

#line default
#line hidden
#line 11 "DynamicAttributeTagHelpers.cshtml"
 }

#line default
#line hidden

            }
            );
            if (ShouldRenderAttributeValue(__rawTagHelperAttributeValue))
            {
                WriteLiteral(" ");
                WriteUnprefixedAttributeValueTo(Output, GetStringAttributeValue("unbound", __rawTagHelperAttributeValue), __rawTagHelperAttributeValue, false);
                __shouldRenderTagHelperAttribute = true;
            }
            WriteLiteralTo(__originalTagHelperAttributeValue, " ");
            WriteTo(__originalTagHelperAttributeValue, __rawTagHelperAttributeValue);
            __rawTagHelperAttributeValue = int.MaxValue;
            if (ShouldRenderAttributeValue(__rawTagHelperAttributeValue))
            {
                WriteLiteral(" ");
                WriteUnprefixedAttributeValueTo(Output, GetStringAttributeValue("unbound", __rawTagHelperAttributeValue), __rawTagHelperAttributeValue, false);
                __shouldRenderTagHelperAttribute = true;
            }
            WriteLiteralTo(__originalTagHelperAttributeValue, " ");
            WriteTo(__originalTagHelperAttributeValue, __rawTagHelperAttributeValue);
            __tagHelperStringValueBuffer = EndTagHelperWritingScope();
            if (__shouldRenderTagHelperAttribute)
            {
                __tagHelperExecutionContext.HtmlAttributes.Add("unbound", Html.Raw(__tagHelperStringValueBuffer));
            }
            __tagHelperExecutionContext.AddTagHelperAttribute("unbound", Html.Raw(__originalTagHelperAttributeValue));
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(243, 183, false);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(426, 4, true);
            WriteLiteral("\r\n\r\n");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", true, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __tagHelperExecutionContext.Add(__InputTagHelper);
            StartTagHelperWritingScope();
            __originalTagHelperAttributeValue = new StringCollectionTextWriter(Output.Encoding);
            __rawTagHelperAttributeValue = long.MinValue;
            if (ShouldRenderAttributeValue(__rawTagHelperAttributeValue))
            {
                WriteUnprefixedAttributeValueTo(Output, GetStringAttributeValue("unbound", __rawTagHelperAttributeValue), __rawTagHelperAttributeValue, false);
            }
            WriteTo(__originalTagHelperAttributeValue, __rawTagHelperAttributeValue);
            __rawTagHelperAttributeValue = DateTime.Now;
            if (ShouldRenderAttributeValue(__rawTagHelperAttributeValue))
            {
                WriteLiteral(" ");
                WriteUnprefixedAttributeValueTo(Output, GetStringAttributeValue("unbound", __rawTagHelperAttributeValue), __rawTagHelperAttributeValue, false);
            }
            WriteLiteralTo(__originalTagHelperAttributeValue, " ");
            WriteTo(__originalTagHelperAttributeValue, __rawTagHelperAttributeValue);
            WriteLiteral(" static    content");
            WriteLiteralTo(__originalTagHelperAttributeValue, " static    content");
            __rawTagHelperAttributeValue = int.MaxValue;
            if (ShouldRenderAttributeValue(__rawTagHelperAttributeValue))
            {
                WriteLiteral(" ");
                WriteUnprefixedAttributeValueTo(Output, GetStringAttributeValue("unbound", __rawTagHelperAttributeValue), __rawTagHelperAttributeValue, false);
            }
            WriteLiteralTo(__originalTagHelperAttributeValue, " ");
            WriteTo(__originalTagHelperAttributeValue, __rawTagHelperAttributeValue);
            __tagHelperStringValueBuffer = EndTagHelperWritingScope();
            __tagHelperExecutionContext.HtmlAttributes.Add("unbound", Html.Raw(__tagHelperStringValueBuffer));
            __tagHelperExecutionContext.AddTagHelperAttribute("unbound", Html.Raw(__originalTagHelperAttributeValue));
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(430, 80, false);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
            Instrumentation.BeginContext(510, 4, true);
            WriteLiteral("\r\n\r\n");
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.Begin("input", true, "test", async() => {
            }
            , StartTagHelperWritingScope, EndTagHelperWritingScope);
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __tagHelperExecutionContext.Add(__InputTagHelper);
            StartTagHelperWritingScope();
            __shouldRenderTagHelperAttribute = false;
            __originalTagHelperAttributeValue = new StringCollectionTextWriter(Output.Encoding);
            __rawTagHelperAttributeValue = new Template((__razor_attribute_value_writer) => {
#line 15 "DynamicAttributeTagHelpers.cshtml"
if (true) { 

#line default
#line hidden

#line 15 "DynamicAttributeTagHelpers.cshtml"
WriteTo(__razor_attribute_value_writer, string.Empty);

#line default
#line hidden
#line 15 "DynamicAttributeTagHelpers.cshtml"
 } else { 

#line default
#line hidden

#line 15 "DynamicAttributeTagHelpers.cshtml"
WriteTo(__razor_attribute_value_writer, false);

#line default
#line hidden
#line 15 "DynamicAttributeTagHelpers.cshtml"
 }

#line default
#line hidden

            }
            );
            if (ShouldRenderAttributeValue(__rawTagHelperAttributeValue))
            {
                WriteUnprefixedAttributeValueTo(Output, GetStringAttributeValue("unbound", __rawTagHelperAttributeValue), __rawTagHelperAttributeValue, false);
                __shouldRenderTagHelperAttribute = true;
            }
            WriteTo(__originalTagHelperAttributeValue, __rawTagHelperAttributeValue);
            __tagHelperStringValueBuffer = EndTagHelperWritingScope();
            if (__shouldRenderTagHelperAttribute)
            {
                __tagHelperExecutionContext.HtmlAttributes.Add("unbound", Html.Raw(__tagHelperStringValueBuffer));
            }
            __tagHelperExecutionContext.AddTagHelperAttribute("unbound", Html.Raw(__originalTagHelperAttributeValue));
            __tagHelperExecutionContext.Output = await __tagHelperRunner.RunAsync(__tagHelperExecutionContext);
            Instrumentation.BeginContext(514, 64, false);
            await WriteTagHelperAsync(__tagHelperExecutionContext);
            Instrumentation.EndContext();
            __tagHelperExecutionContext = __tagHelperScopeManager.End();
        }
        #pragma warning restore 1998
    }
}
