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
            __tagHelperDirectiveSyntaxHelper = "THS";
            __tagHelperDirectiveSyntaxHelper = "something, nice";
            #pragma warning restore 219
        }
        #line hidden
        private global::TestNamespace.PTagHelper __TestNamespace_PTagHelper = null;
        private global::TestNamespace.InputTagHelper __TestNamespace_InputTagHelper = null;
        private global::TestNamespace.InputTagHelper2 __TestNamespace_InputTagHelper2 = null;
        #line hidden
        public BasicTagHelpers.Prefixed()
        {
        }

        #pragma warning disable 1998
        public override async System.Threading.Tasks.Task ExecuteAsync()
        {
            __TestNamespace_InputTagHelper = CreateTagHelper<global::TestNamespace.InputTagHelper>();
            __TestNamespace_InputTagHelper2 = CreateTagHelper<global::TestNamespace.InputTagHelper2>();
            __TestNamespace_InputTagHelper.Type = "checkbox";
            __TestNamespace_InputTagHelper2.Type = __TestNamespace_InputTagHelper.Type;
#line 8 "BasicTagHelpers.Prefixed.cshtml"
 __TestNamespace_InputTagHelper2.Checked = true;

#line default
#line hidden
            __TestNamespace_PTagHelper = CreateTagHelper<global::TestNamespace.PTagHelper>();
        }
        #pragma warning restore 1998
    }
}
