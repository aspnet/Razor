namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class BasicTagHelpers
    {
        [Activate]
        private ITagHelperRunner __tagHelperRunner { get; set; }
        [Activate]
        private ITagHelperScopeManager __tagHelperScopeManager { get; set; }
        #line hidden
        public BasicTagHelpers()
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
            WriteLiteral("<div class=\"randomNonTagHelperAttribute\">\r\n    ");
            __executionContext = __tagHelperScopeManager.Begin("p");
            __PTagHelper = CreateTagHelper<PTagHelper>();
            __executionContext.Add(__PTagHelper);
            __executionContext.AddHtmlAttribute("class", "Hello World");
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral("\r\n        ");
            __executionContext = __tagHelperScopeManager.Begin("p");
            __PTagHelper = CreateTagHelper<PTagHelper>();
            __executionContext.Add(__PTagHelper);
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n        ");
            __executionContext = __tagHelperScopeManager.Begin("input");
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __executionContext.Add(__InputTagHelper);
            __InputTagHelper.Type = "text";
            __executionContext.AddTagHelperAttribute("type", __InputTagHelper.Type);
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __executionContext.Add(__InputTagHelper2);
            __InputTagHelper2.Type = __InputTagHelper.Type;
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n        ");
            __executionContext = __tagHelperScopeManager.Begin("input");
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __executionContext.Add(__InputTagHelper);
            __InputTagHelper.Type = "checkbox";
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
            WriteLiteral("\r\n    ");
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
            WriteLiteral("\r\n</div>");
        }
        #pragma warning restore 1998
    }
}
