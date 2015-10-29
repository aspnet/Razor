namespace TestOutput
{
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
              "something, nice"

#line default
#line hidden
            ;
            #pragma warning restore 219
        }
        #line hidden
        private global::PTagHelper __PTagHelper = null;
        private global::InputTagHelper __InputTagHelper = null;
        private global::InputTagHelper2 __InputTagHelper2 = null;
        #line hidden
        public BasicTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __PTagHelper = CreateTagHelper<global::PTagHelper>();
            __InputTagHelper = CreateTagHelper<global::InputTagHelper>();
            __InputTagHelper2 = CreateTagHelper<global::InputTagHelper2>();
#line 6 "BasicTagHelpers.cshtml"
                                __o = ViewBag.DefaultInterval;

#line default
#line hidden
            __InputTagHelper.Type = "text";
            __InputTagHelper2.Type = __InputTagHelper.Type;
            __InputTagHelper = CreateTagHelper<global::InputTagHelper>();
            __InputTagHelper2 = CreateTagHelper<global::InputTagHelper2>();
            __InputTagHelper.Type = "checkbox";
            __InputTagHelper2.Type = __InputTagHelper.Type;
#line 7 "BasicTagHelpers.cshtml"
            __InputTagHelper2.Checked = true;

#line default
#line hidden
            __PTagHelper = CreateTagHelper<global::PTagHelper>();
        }
        #pragma warning restore 1998
    }
}
