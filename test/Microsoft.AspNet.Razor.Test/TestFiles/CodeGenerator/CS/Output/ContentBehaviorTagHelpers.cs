namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class ContentBehaviorTagHelpers
    {
        [Activate]
        private ITagHelperManager __tagHelperManager { get; set; }
        #line hidden
        public ContentBehaviorTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperAttributeValue = string.Empty;
            ModifyTagHelper __modify_ModifyTagHelper_Modify;
            NoneTagHelper __none_NoneTagHelper_None;
            AppendTagHelper __append_AppendTagHelper_Append;
            PrependTagHelper __prepend_PrependTagHelper_Prepend;
            ReplaceTagHelper __replace_ReplaceTagHelper_Replace;
            __modify_ModifyTagHelper_Modify = __tagHelperManager.StartTagHelper<ModifyTagHelper>();
            __tagHelperManager.AddHTMLAttribute("class", "myModifyClass");
            __tagHelperManager.AddHTMLAttribute("style", "color:red;");
            __tagHelperManager.StartActiveTagHelpers("modify");
            try {
                NewWritingScope(__tagHelperManager.GetTagBodyBuffer());
                WriteLiteral("\r\n    ");
                __none_NoneTagHelper_None = __tagHelperManager.StartTagHelper<NoneTagHelper>();
                __tagHelperManager.AddHTMLAttribute("class", "myNoneClass");
                __tagHelperManager.StartActiveTagHelpers("none");
                __tagHelperManager.ExecuteTagHelpers();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral("\r\n        ");
                __append_AppendTagHelper_Append = __tagHelperManager.StartTagHelper<AppendTagHelper>();
                __tagHelperManager.AddHTMLAttribute("style", "color:red;");
                __tagHelperManager.StartActiveTagHelpers("append");
                __tagHelperManager.ExecuteTagHelpers();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral("\r\n            ");
                __prepend_PrependTagHelper_Prepend = __tagHelperManager.StartTagHelper<PrependTagHelper>();
                __tagHelperManager.AddHTMLAttribute("class", "myPrependClass");
                __tagHelperManager.AddHTMLAttribute("customAttribute", "customValue");
                __tagHelperManager.StartActiveTagHelpers("prepend");
                __tagHelperManager.ExecuteTagHelpers();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral(__tagHelperManager.GenerateTagContent());
                WriteLiteral("\r\n                ");
                __replace_ReplaceTagHelper_Replace = __tagHelperManager.StartTagHelper<ReplaceTagHelper>();
                __tagHelperManager.AddHTMLAttribute("for", "hello");
                __tagHelperManager.AddHTMLAttribute("id", "bar");
                __tagHelperManager.StartActiveTagHelpers("replace");
                __tagHelperManager.ExecuteTagHelpers();
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
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral(__tagHelperManager.GenerateTagContent());
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
        }
        #pragma warning restore 1998
    }
}
