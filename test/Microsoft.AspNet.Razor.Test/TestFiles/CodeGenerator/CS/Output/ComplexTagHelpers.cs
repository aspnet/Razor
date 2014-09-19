namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class ComplexTagHelpers
    {
        [Activate]
        private ITagHelperManager __tagHelperManager { get; set; }
        #line hidden
        public ComplexTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperAttributeValue = string.Empty;
            PTagHelper __p_PTagHelper_None;
            InputTagHelper __input_InputTagHelper_None;
            InputTagHelper2 __input_InputTagHelper2_None;
#line 1 "ComplexTagHelpers.cshtml"
 if (true)
{
    var checkbox = "checkbox";


#line default
#line hidden

            WriteLiteral("    <div class=\"randomNonTagHelperAttribute\">\r\n        ");
            __p_PTagHelper_None = __tagHelperManager.StartTagHelper<PTagHelper>();
            try {
                NewWritingScope();
                WriteLiteral("Current Time: ");
                Write(
#line 6 "ComplexTagHelpers.cshtml"
DateTime.Now

#line default
#line hidden
                );

            }
            finally {
                __tagHelperAttributeValue = EndWritingScope();
            }
            __tagHelperManager.AddHTMLAttribute("time", __tagHelperAttributeValue);
            __tagHelperManager.StartActiveTagHelpers("p");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("\r\n            <h1>Set Time:</h1>\r\n");
#line 8 "ComplexTagHelpers.cshtml"
            

#line default
#line hidden

#line 8 "ComplexTagHelpers.cshtml"
             if (false)
            {

#line default
#line hidden

            WriteLiteral("                ");
            __p_PTagHelper_None = __tagHelperManager.StartTagHelper<PTagHelper>();
            __tagHelperManager.StartActiveTagHelpers("p");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("New Time: ");
            __input_InputTagHelper_None = __tagHelperManager.StartTagHelper<InputTagHelper>();
            __input_InputTagHelper_None.Type = "text";
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper_None.Type);
            __input_InputTagHelper2_None = __tagHelperManager.StartTagHelper<InputTagHelper2>();
            __input_InputTagHelper2_None.Type = __input_InputTagHelper_None.Type;
            __tagHelperManager.AddHTMLAttribute("value", "");
            __tagHelperManager.AddHTMLAttribute("placeholder", "Enter in a new time...");
            __tagHelperManager.StartActiveTagHelpers("input");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n");
#line 11 "ComplexTagHelpers.cshtml"
            }
            else
            {

#line default
#line hidden

            WriteLiteral("                ");
            __p_PTagHelper_None = __tagHelperManager.StartTagHelper<PTagHelper>();
            __tagHelperManager.StartActiveTagHelpers("p");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("Current Time: ");
            __input_InputTagHelper_None = __tagHelperManager.StartTagHelper<InputTagHelper>();
            try {
                NewWritingScope();
                Write(
#line 14 "ComplexTagHelpers.cshtml"
checkbox

#line default
#line hidden
                );

            }
            finally {
                __tagHelperAttributeValue = EndWritingScope();
            }
            __input_InputTagHelper_None.Type = __tagHelperAttributeValue;
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper_None.Type);
            __input_InputTagHelper2_None = __tagHelperManager.StartTagHelper<InputTagHelper2>();
            __input_InputTagHelper2_None.Type = __input_InputTagHelper_None.Type;
            __input_InputTagHelper2_None.Checked = true;
            __tagHelperManager.AddTagHelperAttribute("checked", __input_InputTagHelper2_None.Checked);
            __tagHelperManager.StartActiveTagHelpers("input");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n                ");
            __input_InputTagHelper_None = __tagHelperManager.StartTagHelper<InputTagHelper>();
            try {
                NewWritingScope();
                Write(
#line 15 "ComplexTagHelpers.cshtml"
true ? "checkbox" : "anything"

#line default
#line hidden
                );

            }
            finally {
                __tagHelperAttributeValue = EndWritingScope();
            }
            __input_InputTagHelper_None.Type = __tagHelperAttributeValue;
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper_None.Type);
            __input_InputTagHelper2_None = __tagHelperManager.StartTagHelper<InputTagHelper2>();
            __input_InputTagHelper2_None.Type = __input_InputTagHelper_None.Type;
            __tagHelperManager.StartActiveTagHelpers("input");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n                ");
            __input_InputTagHelper_None = __tagHelperManager.StartTagHelper<InputTagHelper>();
            try {
                NewWritingScope();
#line 16 "ComplexTagHelpers.cshtml"
if(true) {

#line default
#line hidden

                WriteLiteral(" checkbox ");
#line 16 "ComplexTagHelpers.cshtml"
} else {

#line default
#line hidden

                WriteLiteral(" anything ");
#line 16 "ComplexTagHelpers.cshtml"
}

#line default
#line hidden

            }
            finally {
                __tagHelperAttributeValue = EndWritingScope();
            }
            __input_InputTagHelper_None.Type = __tagHelperAttributeValue;
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper_None.Type);
            __input_InputTagHelper2_None = __tagHelperManager.StartTagHelper<InputTagHelper2>();
            __input_InputTagHelper2_None.Type = __input_InputTagHelper_None.Type;
            __tagHelperManager.StartActiveTagHelpers("input");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n");
#line 17 "ComplexTagHelpers.cshtml"
            }

#line default
#line hidden

            WriteLiteral("        ");
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n    </div>\r\n");
#line 20 "ComplexTagHelpers.cshtml"
}

#line default
#line hidden

        }
        #pragma warning restore 1998
    }
}
