namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class BasicTagHelpers.Prefixed
    {
        private static object @__o;
        private void @__RazorDesignTimeHelpers__()
        {
            #pragma warning disable 219
            string __tagHelperDirectiveSyntaxHelper = null;
            __tagHelperDirectiveSyntaxHelper = 
#line 1 "BasicTagHelpers.Prefixed.cshtml"
                 "THS"

#line default
#line hidden
            ;
            __tagHelperDirectiveSyntaxHelper = 
#line 2 "BasicTagHelpers.Prefixed.cshtml"
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
        public BasicTagHelpers.Prefixed()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __InputTagHelper = CreateTagHelper<global::InputTagHelper>();
            __InputTagHelper2 = CreateTagHelper<global::InputTagHelper2>();
            __InputTagHelper.Type = "checkbox";
            __InputTagHelper2.Type = __InputTagHelper.Type;
#line 8 "BasicTagHelpers.Prefixed.cshtml"
               __InputTagHelper2.Checked = true;

#line default
#line hidden
            __PTagHelper = CreateTagHelper<global::PTagHelper>();
        }
        #pragma warning restore 1998
    }
}
