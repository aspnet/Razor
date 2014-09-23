namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class ContentBehaviorTagHelpers
    {
        [Activate]
        private TagHelperManager __tagHelperManager { get; set; }
        #line hidden
        public ContentBehaviorTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperAttributeValue = string.Empty;
            ModifyTagHelper __modify_ModifyTagHelper;
            NoneTagHelper __none_NoneTagHelper;
            AppendTagHelper __append_AppendTagHelper;
            PrependTagHelper __prepend_PrependTagHelper;
            ReplaceTagHelper __replace_ReplaceTagHelper;
            __modify_ModifyTagHelper = __tagHelperManager.InstantiateTagHelper<ModifyTagHelper>();
            __tagHelperManager.AddHTMLAttribute("class", "myModifyClass");
            __tagHelperManager.AddHTMLAttribute("style", "color:red;");
            __tagHelperManager.StartActiveTagHelpers("modify");
            try {
                NewWritingScope(__tagHelperManager.GetTagBodyBuffer());
                WriteLiteral("\r\n    ");
                __none_NoneTagHelper = __tagHelperManager.InstantiateTagHelper<NoneTagHelper>();
                __tagHelperManager.AddHTMLAttribute("class", "myNoneClass");
                __tagHelperManager.StartActiveTagHelpers("none");
                await __tagHelperManager.ExecuteTagHelpersAsync();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral("\r\n        ");
                __append_AppendTagHelper = __tagHelperManager.InstantiateTagHelper<AppendTagHelper>();
                __tagHelperManager.AddHTMLAttribute("style", "color:red;");
                __tagHelperManager.StartActiveTagHelpers("append");
                await __tagHelperManager.ExecuteTagHelpersAsync();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral("\r\n            ");
                __prepend_PrependTagHelper = __tagHelperManager.InstantiateTagHelper<PrependTagHelper>();
                __tagHelperManager.AddHTMLAttribute("class", "myPrependClass");
                __tagHelperManager.AddHTMLAttribute("customAttribute", "customValue");
                __tagHelperManager.StartActiveTagHelpers("prepend");
                await __tagHelperManager.ExecuteTagHelpersAsync();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral(__tagHelperManager.GenerateTagContent());
                WriteLiteral("\r\n                ");
                __replace_ReplaceTagHelper = __tagHelperManager.InstantiateTagHelper<ReplaceTagHelper>();
                __tagHelperManager.AddHTMLAttribute("for", "hello");
                __tagHelperManager.AddHTMLAttribute("id", "bar");
                __tagHelperManager.StartActiveTagHelpers("replace");
                await __tagHelperManager.ExecuteTagHelpersAsync();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral(__tagHelperManager.GenerateTagContent());
                WriteLiteral(__tagHelperManager.GenerateTagEnd());
                __tagHelperManager.EndTagHelpers();
                WriteLiteral("\r\n            ");
                WriteLiteral(__tagHelperManager.GenerateTagEnd());
                __tagHelperManager.EndTagHelpers();
                WriteLiteral("\r\n        ");
                WriteLiteral(__tagHelperManager.GenerateTagContent());
                WriteLiteral(__tagHelperManager.GenerateTagEnd());
                __tagHelperManager.EndTagHelpers();
                WriteLiteral("\r\n    ");
                WriteLiteral(__tagHelperManager.GenerateTagEnd());
                __tagHelperManager.EndTagHelpers();
                WriteLiteral("\r\n");
            }
            finally {
                __tagHelperAttributeValue = EndWritingScope();
            }
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagContent());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
        }
        #pragma warning restore 1998
    }
}
