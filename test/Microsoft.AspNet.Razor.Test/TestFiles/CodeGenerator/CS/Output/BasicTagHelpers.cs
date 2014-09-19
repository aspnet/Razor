namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class BasicTagHelpers
    {
        [Activate]
        private ITagHelperManager __tagHelperManager { get; set; }
        #line hidden
        public BasicTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperAttributeValue = string.Empty;
            pTagHelper __p_pTagHelper_None;
            inputTagHelper __input_inputTagHelper_None;
            inputTagHelper2 __input_inputTagHelper2_None;
            WriteLiteral("<div class=\"randomNonTagHelperAttribute\">\r\n    ");
            __p_pTagHelper_None = __tagHelperManager.StartTagHelper<pTagHelper>();
            __tagHelperManager.AddHTMLAttribute("class", "Hello World");
            __tagHelperManager.StartActiveTagHelpers("p");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("\r\n        ");
            __p_pTagHelper_None = __tagHelperManager.StartTagHelper<pTagHelper>();
            __tagHelperManager.StartActiveTagHelpers("p");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n        ");
            __input_inputTagHelper_None = __tagHelperManager.StartTagHelper<inputTagHelper>();
            __input_inputTagHelper_None.Type = "text";
            __tagHelperManager.AddTagHelperAttribute("type", __input_inputTagHelper_None.Type);
            __input_inputTagHelper2_None = __tagHelperManager.StartTagHelper<inputTagHelper2>();
            __input_inputTagHelper2_None.Type = __input_inputTagHelper_None.Type;
            __tagHelperManager.StartActiveTagHelpers("input");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n        ");
            __input_inputTagHelper_None = __tagHelperManager.StartTagHelper<inputTagHelper>();
            __input_inputTagHelper_None.Type = "checkbox";
            __tagHelperManager.AddTagHelperAttribute("type", __input_inputTagHelper_None.Type);
            __input_inputTagHelper2_None = __tagHelperManager.StartTagHelper<inputTagHelper2>();
            __input_inputTagHelper2_None.Type = __input_inputTagHelper_None.Type;
            __input_inputTagHelper2_None.Checked = true;
            __tagHelperManager.AddTagHelperAttribute("checked", __input_inputTagHelper2_None.Checked);
            __tagHelperManager.StartActiveTagHelpers("input");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n    ");
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n</div>");
        }
        #pragma warning restore 1998
    }
}
