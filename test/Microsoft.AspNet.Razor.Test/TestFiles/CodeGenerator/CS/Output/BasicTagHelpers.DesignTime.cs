namespace TestOutput
{
    using Microsoft.AspNet.Razor.Runtime.TagHelpers;
    using System;
    using System.Threading.Tasks;

    public class BasicTagHelpers
    {
        private static object @__o;
        private void @__RazorDesignTimeHelpers__()
        {
            #pragma warning disable 219
            string __tagHelperDirectiveSyntaxHelper = null;
            __tagHelperDirectiveSyntaxHelper = 
#line 1 "BasicTagHelpers.cshtml"
              "something"

#line default
#line hidden
            ;
            #pragma warning restore 219
        }
        #line hidden
        private PTagHelper __PTagHelper = null;
        private InputTagHelper __InputTagHelper = null;
        private InputTagHelper2 __InputTagHelper2 = null;
        #line hidden
        public BasicTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __PTagHelper = CreateTagHelper<PTagHelper>();
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __InputTagHelper.Type = "text";
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __InputTagHelper2.Type = __InputTagHelper.Type;
            __InputTagHelper = CreateTagHelper<InputTagHelper>();
            __InputTagHelper.Type = "checkbox";
            __InputTagHelper2 = CreateTagHelper<InputTagHelper2>();
            __InputTagHelper2.Type = __InputTagHelper.Type;
            __InputTagHelper2.Checked = true;
            __PTagHelper = CreateTagHelper<PTagHelper>();
        }
        #pragma warning restore 1998
    }
}
