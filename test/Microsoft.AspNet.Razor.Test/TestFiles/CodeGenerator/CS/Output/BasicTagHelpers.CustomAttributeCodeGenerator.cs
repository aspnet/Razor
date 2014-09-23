namespace TestOutput
{
    using System.Threading.Tasks;

    public class BasicTagHelpers
    {
        private TagHelperManager __tagHelperManager { get; set; }
        #line hidden
        public BasicTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperBufferedStringValue = string.Empty;
            PTagHelper __p_PTagHelper;
            InputTagHelper __input_InputTagHelper;
            InputTagHelper2 __input_InputTagHelper2;
            WriteLiteral("<div class=\"randomNonTagHelperAttribute\">\r\n    ");
            __p_PTagHelper = __tagHelperManager.InstantiateTagHelper<PTagHelper>();
            __tagHelperManager.AddHtmlAttribute("class", "Hello World");
            __tagHelperManager.StartTagHelpersScope("p");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("\r\n        ");
            __p_PTagHelper = __tagHelperManager.InstantiateTagHelper<PTagHelper>();
            __tagHelperManager.StartTagHelpersScope("p");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral("\r\n        ");
            __input_InputTagHelper = __tagHelperManager.InstantiateTagHelper<InputTagHelper>();
            __input_InputTagHelper.Type = **From custom attribute code renderer**: "text";
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper.Type);
            __input_InputTagHelper2 = __tagHelperManager.InstantiateTagHelper<InputTagHelper2>();
            __input_InputTagHelper2.Type = __input_InputTagHelper.Type;
            __tagHelperManager.StartTagHelpersScope("input");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral("\r\n        ");
            __input_InputTagHelper = __tagHelperManager.InstantiateTagHelper<InputTagHelper>();
            __input_InputTagHelper.Type = **From custom attribute code renderer**: "checkbox";
            __tagHelperManager.AddTagHelperAttribute("type", __input_InputTagHelper.Type);
            __input_InputTagHelper2 = __tagHelperManager.InstantiateTagHelper<InputTagHelper2>();
            __input_InputTagHelper2.Type = __input_InputTagHelper.Type;
            __input_InputTagHelper2.Checked = **From custom attribute code renderer**: true;
            __tagHelperManager.AddTagHelperAttribute("checked", __input_InputTagHelper2.Checked);
            __tagHelperManager.StartTagHelpersScope("input");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral("\r\n    ");
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
            WriteLiteral("\r\n</div>");
        }
        #pragma warning restore 1998
    }
}
