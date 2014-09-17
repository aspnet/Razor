namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class ComplexTagHelpers
    {
        [Activate]
        private TagHelperManager __tagHelperManager { get; set; }
        #line hidden
        public ComplexTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperBufferedStringValue = string.Empty;
            PTagHelper __p_PTagHelper;
            InputTagHelper __input_InputTagHelper;
            InputTagHelper2 __input_InputTagHelper2;
            WriteLiteral("\r\n");
#line 3 "ComplexTagHelpers.cshtml"
 if (true)
{
    var checkbox = "checkbox";


#line default
#line hidden

            WriteLiteral("    <div class=\"randomNonTagHelperAttribute\">\r\n        ");
            __p_PTagHelper = __tagHelperManager.InstantiateTagHelper<PTagHelper>();
            try {
                NewWritingScope();
                WriteLiteral("Current Time: ");
                Write(
#line 8 "ComplexTagHelpers.cshtml"
DateTime.Now

#line default
#line hidden
                );

            }
            finally {
                __tagHelperBufferedStringValue = EndWritingScope();
            }
            __tagHelperManager.AddHtmlAttribute("time", __tagHelperBufferedStringValue);
            __tagHelperManager.StartTagHelpersScope("p");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("\r\n            <h1>Set Time:</h1>\r\n");
#line 10 "ComplexTagHelpers.cshtml"
            

#line default
#line hidden

#line 10 "ComplexTagHelpers.cshtml"
             if (false)
            {

#line default
#line hidden

            WriteLiteral("                ");
            __p_PTagHelper = __tagHelperManager.InstantiateTagHelper<PTagHelper>();
            __tagHelperManager.StartTagHelpersScope("p");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("New Time: ");
            __input_InputTagHelper = __tagHelperManager.InstantiateTagHelper<InputTagHelper>();
            __input_InputTagHelper.Type = "text";
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper.Type);
            __input_InputTagHelper2 = __tagHelperManager.InstantiateTagHelper<InputTagHelper2>();
            __input_InputTagHelper2.Type = __input_InputTagHelper.Type;
            __tagHelperManager.AddHtmlAttribute("value", "");
            __tagHelperManager.AddHtmlAttribute("placeholder", "Enter in a new time...");
            __tagHelperManager.StartTagHelpersScope("input");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral("\r\n");
#line 13 "ComplexTagHelpers.cshtml"
            }
            else
            {

#line default
#line hidden

            WriteLiteral("                ");
            __p_PTagHelper = __tagHelperManager.InstantiateTagHelper<PTagHelper>();
            __tagHelperManager.StartTagHelpersScope("p");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("Current Time: ");
            __input_InputTagHelper = __tagHelperManager.InstantiateTagHelper<InputTagHelper>();
            try {
                NewWritingScope();
                Write(
#line 16 "ComplexTagHelpers.cshtml"
checkbox

#line default
#line hidden
                );

            }
            finally {
                __tagHelperBufferedStringValue = EndWritingScope();
            }
            __input_InputTagHelper.Type = __tagHelperBufferedStringValue;
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper.Type);
            __input_InputTagHelper2 = __tagHelperManager.InstantiateTagHelper<InputTagHelper2>();
            __input_InputTagHelper2.Type = __input_InputTagHelper.Type;
            __input_InputTagHelper2.Checked = true;
            __tagHelperManager.AddTagHelperAttribute("checked", __input_InputTagHelper2.Checked);
            __tagHelperManager.StartTagHelpersScope("input");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral("\r\n                ");
            __input_InputTagHelper = __tagHelperManager.InstantiateTagHelper<InputTagHelper>();
            try {
                NewWritingScope();
                Write(
#line 17 "ComplexTagHelpers.cshtml"
true ? "checkbox" : "anything"

#line default
#line hidden
                );

            }
            finally {
                __tagHelperBufferedStringValue = EndWritingScope();
            }
            __input_InputTagHelper.Type = __tagHelperBufferedStringValue;
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper.Type);
            __input_InputTagHelper2 = __tagHelperManager.InstantiateTagHelper<InputTagHelper2>();
            __input_InputTagHelper2.Type = __input_InputTagHelper.Type;
            __tagHelperManager.StartTagHelpersScope("input");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral("\r\n                ");
            __input_InputTagHelper = __tagHelperManager.InstantiateTagHelper<InputTagHelper>();
            try {
                NewWritingScope();
#line 18 "ComplexTagHelpers.cshtml"
if(true) {

#line default
#line hidden

                WriteLiteral(" checkbox ");
#line 18 "ComplexTagHelpers.cshtml"
} else {

#line default
#line hidden

                WriteLiteral(" anything ");
#line 18 "ComplexTagHelpers.cshtml"
}

#line default
#line hidden

            }
            finally {
                __tagHelperBufferedStringValue = EndWritingScope();
            }
            __input_InputTagHelper.Type = __tagHelperBufferedStringValue;
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper.Type);
            __input_InputTagHelper2 = __tagHelperManager.InstantiateTagHelper<InputTagHelper2>();
            __input_InputTagHelper2.Type = __input_InputTagHelper.Type;
            __tagHelperManager.StartTagHelpersScope("input");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral("\r\n");
#line 19 "ComplexTagHelpers.cshtml"
            }

#line default
#line hidden

            WriteLiteral("        ");
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral("\r\n    </div>\r\n");
#line 22 "ComplexTagHelpers.cshtml"
}

#line default
#line hidden

        }
        #pragma warning restore 1998
    }
}
