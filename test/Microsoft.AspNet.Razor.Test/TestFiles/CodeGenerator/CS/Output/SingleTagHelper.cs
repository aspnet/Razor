namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class SingleTagHelper
    {
        [Activate]
        private ITagHelperRunner __tagHelperRunner { get; set; }
        [Activate]
        private ITagHelperScopeManager __tagHelperScopeManager { get; set; }
        #line hidden
        public SingleTagHelper()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperBufferedStringValue = string.Empty;
            TagHelperExecutionContext __executionContext = null;
            PTagHelper __PTagHelper;
            __executionContext = __tagHelperScopeManager.Begin("p");
            __PTagHelper = CreateTagHelper<PTagHelper>();
            __executionContext.Add(__PTagHelper);
            __PTagHelper.Foo = 1337;
            __executionContext.AddTagHelperAttribute("foo", __PTagHelper.Foo);
            __executionContext.AddHtmlAttribute("class", "Hello World");
            __executionContext.Output = await __tagHelperRunner.RunAsync(__executionContext);
            WriteLiteral(__executionContext.Output.GenerateTagStart());
            WriteLiteral("Body of Tag");
            WriteLiteral(__executionContext.Output.GenerateTagEnd());
            __executionContext = __tagHelperScopeManager.End();
        }
        #pragma warning restore 1998
    }
}
