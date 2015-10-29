namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class MinimizedTagHelpers
    {
        private static object @__o;
        private void @__RazorDesignTimeHelpers__()
        {
            #pragma warning disable 219
            string __tagHelperDirectiveSyntaxHelper = null;
            __tagHelperDirectiveSyntaxHelper = 
#line 1 "MinimizedTagHelpers.cshtml"
              "something, nice"

#line default
#line hidden
            ;
            #pragma warning restore 219
        }
        #line hidden
        private global::CatchAllTagHelper __CatchAllTagHelper = null;
        private global::InputTagHelper __InputTagHelper = null;
        #line hidden
        public MinimizedTagHelpers()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            __CatchAllTagHelper = CreateTagHelper<global::CatchAllTagHelper>();
            __InputTagHelper = CreateTagHelper<global::InputTagHelper>();
            __CatchAllTagHelper = CreateTagHelper<global::CatchAllTagHelper>();
            __InputTagHelper.BoundRequiredString = "hello";
            __InputTagHelper = CreateTagHelper<global::InputTagHelper>();
            __CatchAllTagHelper = CreateTagHelper<global::CatchAllTagHelper>();
            __CatchAllTagHelper.BoundRequiredString = "world";
            __InputTagHelper.BoundRequiredString = "hello2";
            __InputTagHelper = CreateTagHelper<global::InputTagHelper>();
            __CatchAllTagHelper = CreateTagHelper<global::CatchAllTagHelper>();
            __InputTagHelper.BoundRequiredString = "world";
            __CatchAllTagHelper = CreateTagHelper<global::CatchAllTagHelper>();
        }
        #pragma warning restore 1998
    }
}
