namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class ContentBehaviorTagHelpers
    {
        #line hidden
        public ContentBehaviorTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            ITagHelperManager __tagHelperManager = CreateTagHelper();
            var __tagHelperBufferValue = string.Empty;
            modifyTagHelper __modify_modifyTagHelper_Modify = null;
            noneTagHelper __none_noneTagHelper_None = null;
            appendTagHelper __append_appendTagHelper_Append = null;
            prependTagHelper __prepend_prependTagHelper_Prepend = null;
            replaceTagHelper __replace_replaceTagHelper_Replace = null;
            __modify_modifyTagHelper_Modify = CreateTagHelper<modifyTagHelper>();
            __tagHelperManager.AddActiveTagHelper(__modify_modifyTagHelper_Modify);
            __tagHelperManager.AddHTMLAttribute("class", "myModifyClass");
            __tagHelperManager.AddHTMLAttribute("style", "color:red;");
            __tagHelperManager.StartActiveTagHelpers("modify");
            try {
                NewWritingScope(__tagHelperManager.GetTagBodyBuffer());
                WriteLiteral("\r\n    ");
                __none_noneTagHelper_None = CreateTagHelper<noneTagHelper>();
                __tagHelperManager.AddActiveTagHelper(__none_noneTagHelper_None);
                __tagHelperManager.AddHTMLAttribute("class", "myNoneClass");
                __tagHelperManager.StartActiveTagHelpers("none");
                __tagHelperManager.ExecuteTagHelpers();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral("\r\n        ");
                __append_appendTagHelper_Append = CreateTagHelper<appendTagHelper>();
                __tagHelperManager.AddActiveTagHelper(__append_appendTagHelper_Append);
                __tagHelperManager.AddHTMLAttribute("style", "color:red;");
                __tagHelperManager.StartActiveTagHelpers("append");
                __tagHelperManager.ExecuteTagHelpers();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral("\r\n            ");
                __prepend_prependTagHelper_Prepend = CreateTagHelper<prependTagHelper>();
                __tagHelperManager.AddActiveTagHelper(__prepend_prependTagHelper_Prepend);
                __tagHelperManager.AddHTMLAttribute("class", "myPrependClass");
                __tagHelperManager.AddHTMLAttribute("customAttribute", "customValue");
                __tagHelperManager.StartActiveTagHelpers("prepend");
                __tagHelperManager.ExecuteTagHelpers();
                WriteLiteral(__tagHelperManager.GenerateTagStart());
                WriteLiteral(__tagHelperManager.GenerateTagContent());
                WriteLiteral("\r\n                ");
                __replace_replaceTagHelper_Replace = CreateTagHelper<replaceTagHelper>();
                __tagHelperManager.AddActiveTagHelper(__replace_replaceTagHelper_Replace);
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
                __tagHelperBufferValue = EndWritingScope();
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
