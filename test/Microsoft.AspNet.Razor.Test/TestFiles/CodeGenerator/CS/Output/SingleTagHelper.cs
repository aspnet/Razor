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
            var __tagHelperAttributeValue = string.Empty;
            PTagHelper __p_PTagHelper;
            __p_PTagHelper = __tagHelperManager.InstantiateTagHelper<PTagHelper>();
            __p_PTagHelper.Foo = 1337;
            __tagHelperManager.AddTagHelperAttribute("foo", __p_PTagHelper.Foo);
            __tagHelperManager.AddHTMLAttribute("class", "Hello World");
            __tagHelperManager.StartActiveTagHelpers("p");
            await __tagHelperManager.ExecuteTagHelpersAsync();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("Body of Tag");
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
        }
        #pragma warning restore 1998
    }
}
