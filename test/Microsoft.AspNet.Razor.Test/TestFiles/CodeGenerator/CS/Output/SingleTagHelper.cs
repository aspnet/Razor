namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class SingleTagHelper
    {
        [Activate]
        private TagHelperManager __tagHelperManager { get; set; }
        #line hidden
        public SingleTagHelper()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperBufferedStringValue = string.Empty;
            PTagHelper __p_PTagHelper;
            WriteLiteral("\r\n");
            __p_PTagHelper = __tagHelperManager.InstantiateTagHelper<PTagHelper>();
            __p_PTagHelper.Foo = 1337;
            __tagHelperManager.AddTagHelperAttribute("foo", __p_PTagHelper.Foo);
            __tagHelperManager.AddHtmlAttribute("class", "Hello World");
            __tagHelperManager.StartTagHelpersScope("p");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("Body of Tag");
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpersScope();
        }
        #pragma warning restore 1998
    }
}
