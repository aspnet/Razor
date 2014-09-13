namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class BasicTagHelpers
    {
        #line hidden
        public BasicTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            ITagHelperManager __tagHelperManager = CreateTagHelper();
            var __tagHelperBufferValue = string.Empty;
            pTagHelper __p_pTagHelper_None = null;
            inputTagHelper __input_inputTagHelper_None = null;
            inputTagHelper2 __input_inputTagHelper2_None = null;
            WriteLiteral("<div class=\"randomNonTagHelperAttribute\">\r\n    ");
            __p_pTagHelper_None = CreateTagHelper<pTagHelper>();
            __tagHelperManager.AddActiveTagHelper(__p_pTagHelper_None);
            __tagHelperManager.AddHTMLAttribute("class", "Hello World");
            __tagHelperManager.StartActiveTagHelpers("p");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("\r\n        ");
            __p_pTagHelper_None = CreateTagHelper<pTagHelper>();
            __tagHelperManager.AddActiveTagHelper(__p_pTagHelper_None);
            __tagHelperManager.StartActiveTagHelpers("p");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n        ");
            __input_inputTagHelper_None = CreateTagHelper<inputTagHelper>();
            __input_inputTagHelper_None.Type = "text";
            __tagHelperManager.AddTagHelperAttribute("type", __input_inputTagHelper_None.Type);
            __tagHelperManager.AddActiveTagHelper(__input_inputTagHelper_None);
            __input_inputTagHelper2_None = CreateTagHelper<inputTagHelper2>();
            __input_inputTagHelper2_None.Type = __input_inputTagHelper_None.Type;
            __tagHelperManager.AddActiveTagHelper(__input_inputTagHelper2_None);
            __tagHelperManager.StartActiveTagHelpers("input");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
            WriteLiteral("\r\n        ");
            __input_inputTagHelper_None = CreateTagHelper<inputTagHelper>();
            __input_inputTagHelper_None.Type = "checkbox";
            __tagHelperManager.AddTagHelperAttribute("type", __input_inputTagHelper_None.Type);
            __tagHelperManager.AddActiveTagHelper(__input_inputTagHelper_None);
            __input_inputTagHelper2_None = CreateTagHelper<inputTagHelper2>();
            __input_inputTagHelper2_None.Type = __input_inputTagHelper_None.Type;
            __input_inputTagHelper2_None.Checked = true;
            __tagHelperManager.AddTagHelperAttribute("checked", __input_inputTagHelper2_None.Checked);
            __tagHelperManager.AddActiveTagHelper(__input_inputTagHelper2_None);
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
