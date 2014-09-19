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
            modifyTagHelper __modify_modifyTagHelper_Modify;
            noneTagHelper __none_noneTagHelper_None;
            appendTagHelper __append_appendTagHelper_Append;
            prependTagHelper __prepend_prependTagHelper_Prepend;
            replaceTagHelper __replace_replaceTagHelper_Replace;
            __modify_modifyTagHelper_Modify = __tagHelperManager.StartTagHelper<modifyTagHelper>();
            __tagHelperManager.AddHTMLAttribute("class", "myModifyClass");
            __tagHelperManager.AddHTMLAttribute("style", "color:red;");
            __tagHelperManager.StartActiveTagHelpers("modify");
            try {
                NewWritingScope(__tagHelperManager.GetTagBodyBuffer());
                WriteLiteral("\r\n    ");
                __none_noneTagHelper_None = __tagHelperManager.StartTagHelper<noneTagHelper>();
                __tagHelperManager.AddHTMLAttribute("class", "myNoneClass");
                __tagHelperManager.StartActiveTagHelpers("none");
                __tagHelperManager.ExecuteTagHelpers();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral("\r\n        ");
                __append_appendTagHelper_Append = __tagHelperManager.StartTagHelper<appendTagHelper>();
                __tagHelperManager.AddHTMLAttribute("style", "color:red;");
                __tagHelperManager.StartActiveTagHelpers("append");
                __tagHelperManager.ExecuteTagHelpers();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral("\r\n            ");
                __prepend_prependTagHelper_Prepend = __tagHelperManager.StartTagHelper<prependTagHelper>();
                __tagHelperManager.AddHTMLAttribute("class", "myPrependClass");
                __tagHelperManager.AddHTMLAttribute("customAttribute", "customValue");
                __tagHelperManager.StartActiveTagHelpers("prepend");
                __tagHelperManager.ExecuteTagHelpers();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral(__tagHelperManager.GenerateTagContent());
                WriteLiteral("\r\n                ");
                __replace_replaceTagHelper_Replace = __tagHelperManager.StartTagHelper<replaceTagHelper>();
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
