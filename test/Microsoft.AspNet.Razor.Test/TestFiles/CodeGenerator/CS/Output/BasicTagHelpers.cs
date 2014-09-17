namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class BasicTagHelpers
    {
        [Activate]
        private TagHelperManager __tagHelperManager { get; set; }
        #line hidden
        public BasicTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperAttributeValue = string.Empty;
            PTagHelper __p_PTagHelper;
            InputTagHelper __input_InputTagHelper;
            InputTagHelper2 __input_InputTagHelper2;
            WriteLiteral("\r\n<div class=\"randomNonTagHelperAttribute\">\r\n    ");
            __p_PTagHelper = __tagHelperManager.InstantiateTagHelper<PTagHelper>();
            __tagHelperManager.AddHTMLAttribute("class", "Hello World");
            __tagHelperManager.StartActiveTagHelpers("p");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("\r\n        ");
            __p_PTagHelper = __tagHelperManager.InstantiateTagHelper<PTagHelper>();
            __tagHelperManager.StartActiveTagHelpers("p");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n        ");
            __input_InputTagHelper = __tagHelperManager.InstantiateTagHelper<InputTagHelper>();
            __input_InputTagHelper.Type = "text";
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper.Type);
            __input_InputTagHelper2 = __tagHelperManager.InstantiateTagHelper<InputTagHelper2>();
            __input_InputTagHelper2.Type = __input_InputTagHelper.Type;
            __tagHelperManager.StartActiveTagHelpers("input");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n        ");
            __input_InputTagHelper = __tagHelperManager.InstantiateTagHelper<InputTagHelper>();
            __input_InputTagHelper.Type = "checkbox";
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper.Type);
            __input_InputTagHelper2 = __tagHelperManager.InstantiateTagHelper<InputTagHelper2>();
            __input_InputTagHelper2.Type = __input_InputTagHelper.Type;
            __input_InputTagHelper2.Checked = true;
            __tagHelperManager.AddTagHelperAttribute("checked", __input_InputTagHelper2.Checked);
            __tagHelperManager.StartActiveTagHelpers("input");
            await __tagHelperManager.ExecuteTagHelpersAsync();
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
