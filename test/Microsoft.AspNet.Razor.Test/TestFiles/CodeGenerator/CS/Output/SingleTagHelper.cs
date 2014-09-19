namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class SingleTagHelper
    {
        [Activate]
        private ITagHelperManager __tagHelperManager { get; set; }
        #line hidden
        public SingleTagHelper()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            var __tagHelperAttributeValue = string.Empty;
            pTagHelper __p_pTagHelper_None;
            __p_pTagHelper_None = __tagHelperManager.StartTagHelper<pTagHelper>();
            __p_pTagHelper_None.Foo = 1337;
            __tagHelperManager.AddTagHelperAttribute("foo", __p_pTagHelper_None.Foo);
            __tagHelperManager.AddHTMLAttribute("class", "Hello World");
            __tagHelperManager.StartActiveTagHelpers("p");
            __tagHelperManager.ExecuteTagHelpers();
            WriteLiteral(__tagHelperManager.GenerateTagStart());
            WriteLiteral("Body of Tag");
            WriteLiteral(__tagHelperManager.GenerateTagEnd());
            __tagHelperManager.EndTagHelpers();
        }
        #pragma warning restore 1998
    }
}
