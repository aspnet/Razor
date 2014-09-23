namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class ComplexTagHelpers
    {
        [Activate]
        private ITagHelperRunner __tagHelperRunner { get; set; }
        [Activate]
        private ITagHelperScopeManager __tagHelperScopeManager { get; set; }
        #line hidden
        public ComplexTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperBufferedStringValue = string.Empty;
            TagHelperExecutionContext __executionContext = null;
            PTagHelper __PTagHelper;
            InputTagHelper __InputTagHelper;
            InputTagHelper2 __InputTagHelper2;
#line 1 "ComplexTagHelpers.cshtml"
 if (true)
{
    var checkbox = "checkbox";


#line default
#line hidden

            WriteLiteral("    <div class=\"randomNonTagHelperAttribute\">\r\n        ");
            __executionContext = __tagHelperScopeManager.Begin("p");
            __PTagHelper = CreateTagHelper<PTagHelper>();
            __executionContext.Add(__PTagHelper);
            try {
                StartWritingScope();
                WriteLiteral("Current Time: ");
                Write(
#line 6 "ComplexTagHelpers.cshtml"
DateTime.Now

#line default
#line hidden
                );

            }
            finally {
                __tagHelperBufferedStringValue = EndWritingScope();
            }
            __executionContext.AddHtmlAttribute("time", __tagHelperBufferedStringValue.ToString());
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
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
            __executionContext = __tagHelperScopeManager.Begin("p");
            __PTagHelper = CreateTagHelper<PTagHelper>();
            __executionContext.Add(__PTagHelper);
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral("New Time: ");
            __executionContext = __tagHelperScopeManager.Begin("input");
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __executionContext.Add(__InputTagHelper);
            __InputTagHelper.Type = "text";
            __executionContext.AddTagHelperAttribute("type", __InputTagHelper.Type);
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __executionContext.Add(__InputTagHelper2);
            __InputTagHelper2.Type = __InputTagHelper.Type;
            __executionContext.AddHtmlAttribute("value", "");
            __executionContext.AddHtmlAttribute("placeholder", "Enter in a new time...");
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n");
#line 11 "ComplexTagHelpers.cshtml"
            }
            else
            {

#line default
#line hidden

            WriteLiteral("                ");
            __executionContext = __tagHelperScopeManager.Begin("p");
            __PTagHelper = CreateTagHelper<PTagHelper>();
            __executionContext.Add(__PTagHelper);
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral("Current Time: ");
            __executionContext = __tagHelperScopeManager.Begin("input");
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __executionContext.Add(__InputTagHelper);
            try {
                StartWritingScope();
                Write(
#line 14 "ComplexTagHelpers.cshtml"
checkbox

#line default
#line hidden
                );

            }
            finally {
                __tagHelperBufferedStringValue = EndWritingScope();
            }
            __InputTagHelper.Type = __tagHelperBufferedStringValue.ToString();
            __executionContext.AddTagHelperAttribute("type", __InputTagHelper.Type);
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __executionContext.Add(__InputTagHelper2);
            __InputTagHelper2.Type = __InputTagHelper.Type;
            __InputTagHelper2.Checked = true;
            __executionContext.AddTagHelperAttribute("checked", __InputTagHelper2.Checked);
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n                ");
            __executionContext = __tagHelperScopeManager.Begin("input");
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __executionContext.Add(__InputTagHelper);
            try {
                StartWritingScope();
                Write(
#line 15 "ComplexTagHelpers.cshtml"
true ? "checkbox" : "anything"

#line default
#line hidden
                );

            }
            finally {
                __tagHelperBufferedStringValue = EndWritingScope();
            }
            __InputTagHelper.Type = __tagHelperBufferedStringValue.ToString();
            __executionContext.AddTagHelperAttribute("type", __InputTagHelper.Type);
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __executionContext.Add(__InputTagHelper2);
            __InputTagHelper2.Type = __InputTagHelper.Type;
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n                ");
            __executionContext = __tagHelperScopeManager.Begin("input");
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __executionContext.Add(__InputTagHelper);
            try {
                StartWritingScope();
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
                __tagHelperBufferedStringValue = EndWritingScope();
            }
            __InputTagHelper.Type = __tagHelperBufferedStringValue.ToString();
            __executionContext.AddTagHelperAttribute("type", __InputTagHelper.Type);
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __executionContext.Add(__InputTagHelper2);
            __InputTagHelper2.Type = __InputTagHelper.Type;
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n");
#line 17 "ComplexTagHelpers.cshtml"
            }

#line default
#line hidden

            WriteLiteral("        ");
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n    </div>\r\n");
#line 20 "ComplexTagHelpers.cshtml"
}

#line default
#line hidden

        }
        #pragma warning restore 1998
    }
}
