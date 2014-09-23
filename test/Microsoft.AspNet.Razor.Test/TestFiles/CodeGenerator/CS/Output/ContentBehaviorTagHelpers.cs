namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class ContentBehaviorTagHelpers
    {
        [Activate]
        private ITagHelperRunner __tagHelperRunner { get; set; }
        [Activate]
        private ITagHelperScopeManager __tagHelperScopeManager { get; set; }
        #line hidden
        public ContentBehaviorTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperBufferedStringValue = string.Empty;
            TagHelperExecutionContext __executionContext = null;
            ModifyTagHelper __ModifyTagHelper;
            NoneTagHelper __NoneTagHelper;
            AppendTagHelper __AppendTagHelper;
            PrependTagHelper __PrependTagHelper;
            ReplaceTagHelper __ReplaceTagHelper;
            __executionContext = __tagHelperScopeManager.Begin("modify");
            __ModifyTagHelper = CreateTagHelper<ModifyTagHelper>();
            __executionContext.Add(__ModifyTagHelper);
            __executionContext.AddHtmlAttribute("class", "myModifyClass");
            __executionContext.AddHtmlAttribute("style", "color:red;");
            try {
                StartWritingScope();
                WriteLiteral("\r\n    ");
                __executionContext = __tagHelperScopeManager.Begin("none");
                __NoneTagHelper = CreateTagHelper<NoneTagHelper>();
                __executionContext.Add(__NoneTagHelper);
                __executionContext.AddHtmlAttribute("class", "myNoneClass");
                __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
                WriteLiteral(__executionContext.Output.GenerateTagStart());
                WriteLiteral("\r\n        ");
                __executionContext = __tagHelperScopeManager.Begin("append");
                __AppendTagHelper = CreateTagHelper<AppendTagHelper>();
                __executionContext.Add(__AppendTagHelper);
                __executionContext.AddHtmlAttribute("style", "color:red;");
                __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
                WriteLiteral(__executionContext.Output.GenerateTagStart());
                WriteLiteral("\r\n            ");
                __executionContext = __tagHelperScopeManager.Begin("prepend");
                __PrependTagHelper = CreateTagHelper<PrependTagHelper>();
                __executionContext.Add(__PrependTagHelper);
                __executionContext.AddHtmlAttribute("class", "myPrependClass");
                __executionContext.AddHtmlAttribute("customAttribute", "customValue");
                __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
                WriteLiteral(__executionContext.Output.GenerateTagStart());
                WriteLiteral(__executionContext.Output.GenerateTagContent());
                WriteLiteral("\r\n                ");
                __executionContext = __tagHelperScopeManager.Begin("replace");
                __ReplaceTagHelper = CreateTagHelper<ReplaceTagHelper>();
                __executionContext.Add(__ReplaceTagHelper);
                __executionContext.AddHtmlAttribute("for", "hello");
                __executionContext.AddHtmlAttribute("id", "bar");
                __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
                WriteLiteral(__executionContext.Output.GenerateTagStart());
                WriteLiteral(__executionContext.Output.GenerateTagContent());
                WriteLiteral(__executionContext.Output.GenerateTagEnd());
                __executionContext = __tagHelperScopeManager.End();
                WriteLiteral("\r\n            ");
                WriteLiteral(__executionContext.Output.GenerateTagEnd());
                __executionContext = __tagHelperScopeManager.End();
                WriteLiteral("\r\n        ");
                WriteLiteral(__executionContext.Output.GenerateTagContent());
                WriteLiteral(__executionContext.Output.GenerateTagEnd());
                __executionContext = __tagHelperScopeManager.End();
                WriteLiteral("\r\n    ");
                WriteLiteral(__executionContext.Output.GenerateTagEnd());
                __executionContext = __tagHelperScopeManager.End();
                WriteLiteral("\r\n");
            }
            finally {
                __tagHelperBufferedStringValue = EndWritingScope();
            }
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext, __tagHelperBufferedStringValue);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral(__executionContext.Output.GenerateTagContent());
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
        }
        #pragma warning restore 1998
    }
}
