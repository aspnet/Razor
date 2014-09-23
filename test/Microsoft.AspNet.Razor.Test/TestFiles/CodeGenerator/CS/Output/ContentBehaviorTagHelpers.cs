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
            var __tagHelperBufferedStringValue = string.Empty;
            ModifyTagHelper __modify_ModifyTagHelper;
            NoneTagHelper __none_NoneTagHelper;
            AppendTagHelper __append_AppendTagHelper;
            PrependTagHelper __prepend_PrependTagHelper;
            ReplaceTagHelper __replace_ReplaceTagHelper;
            __modify_ModifyTagHelper = __tagHelperManager.InstantiateTagHelper<ModifyTagHelper>();
            __tagHelperManager.AddHtmlAttribute("class", "myModifyClass");
            __tagHelperManager.AddHtmlAttribute("style", "color:red;");
            __tagHelperManager.StartTagHelpersScope("modify");
            try {
                NewWritingScope(__tagHelperManager.GetContentBuffer());
                WriteLiteral("\r\n    ");
                __none_NoneTagHelper = __tagHelperManager.InstantiateTagHelper<NoneTagHelper>();
                __tagHelperManager.AddHtmlAttribute("class", "myNoneClass");
                __tagHelperManager.StartTagHelpersScope("none");
                await __tagHelperManager.ExecuteTagHelpersAsync();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral("\r\n        ");
                __append_AppendTagHelper = __tagHelperManager.InstantiateTagHelper<AppendTagHelper>();
                __tagHelperManager.AddHtmlAttribute("style", "color:red;");
                __tagHelperManager.StartTagHelpersScope("append");
                await __tagHelperManager.ExecuteTagHelpersAsync();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral("\r\n            ");
                __prepend_PrependTagHelper = __tagHelperManager.InstantiateTagHelper<PrependTagHelper>();
                __tagHelperManager.AddHtmlAttribute("class", "myPrependClass");
                __tagHelperManager.AddHtmlAttribute("customAttribute", "customValue");
                __tagHelperManager.StartTagHelpersScope("prepend");
                await __tagHelperManager.ExecuteTagHelpersAsync();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral(__tagHelperManager.GenerateTagContent());
                WriteLiteral("\r\n                ");
                __replace_ReplaceTagHelper = __tagHelperManager.InstantiateTagHelper<ReplaceTagHelper>();
                __tagHelperManager.AddHtmlAttribute("for", "hello");
                __tagHelperManager.AddHtmlAttribute("id", "bar");
                __tagHelperManager.StartTagHelpersScope("replace");
                await __tagHelperManager.ExecuteTagHelpersAsync();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral(__tagHelperManager.GenerateTagContent());
                WriteLiteral(__tagHelperManager.GenerateTagEnd());
                __tagHelperManager.EndTagHelpersScope();
                WriteLiteral("\r\n            ");
                WriteLiteral(__tagHelperManager.GenerateTagEnd());
                __tagHelperManager.EndTagHelpersScope();
                WriteLiteral("\r\n        ");
                WriteLiteral(__tagHelperManager.GenerateTagContent());
                WriteLiteral(__tagHelperManager.GenerateTagEnd());
                __tagHelperManager.EndTagHelpersScope();
                WriteLiteral("\r\n    ");
                WriteLiteral(__tagHelperManager.GenerateTagEnd());
                __tagHelperManager.EndTagHelpersScope();
                WriteLiteral("\r\n");
            }
            finally {
                __tagHelperBufferedStringValue = EndWritingScope();
            }
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagContent());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
        }
        #pragma warning restore 1998
    }
}
