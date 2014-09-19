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
            PTagHelper __p_PTagHelper_None;
            InputTagHelper __input_InputTagHelper_None;
            InputTagHelper2 __input_InputTagHelper2_None;
            WriteLiteral("<div class=\"randomNonTagHelperAttribute\">\r\n    ");
            __p_PTagHelper_None = __tagHelperManager.StartTagHelper<PTagHelper>();
            __tagHelperManager.AddHTMLAttribute("class", "Hello World");
            __tagHelperManager.StartActiveTagHelpers("p");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("\r\n        ");
            __p_PTagHelper_None = __tagHelperManager.StartTagHelper<PTagHelper>();
            __tagHelperManager.StartActiveTagHelpers("p");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n        ");
            __input_InputTagHelper_None = __tagHelperManager.StartTagHelper<InputTagHelper>();
            __input_InputTagHelper_None.Type = "text";
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper_None.Type);
            __input_InputTagHelper2_None = __tagHelperManager.StartTagHelper<InputTagHelper2>();
            __input_InputTagHelper2_None.Type = __input_InputTagHelper_None.Type;
            __tagHelperManager.StartActiveTagHelpers("input");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n        ");
            __input_InputTagHelper_None = __tagHelperManager.StartTagHelper<InputTagHelper>();
            __input_InputTagHelper_None.Type = "checkbox";
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
            WriteLiteral("\r\n    ");
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n</div>");
        }
        #pragma warning restore 1998
    }
}
