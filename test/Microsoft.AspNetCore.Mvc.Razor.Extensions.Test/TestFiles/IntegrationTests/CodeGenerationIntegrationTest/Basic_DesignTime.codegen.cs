[assembly:Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(null, typeof(AspNetCore.TestFiles_IntegrationTests_CodeGenerationIntegrationTest_Basic_cshtml))]
namespace AspNetCore
{
    #line hidden
    using TModel = global::System.Object;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Rendering;
    using Microsoft.AspNetCore.Mvc.ViewFeatures;
    public class TestFiles_IntegrationTests_CodeGenerationIntegrationTest_Basic_cshtml : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 219
        private void __RazorDirectiveTokenHelpers__() {
        }
        #pragma warning restore 219
        private static System.Object __o = null;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 1 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Basic.cshtml"
       __o = this.ToString();

#line default
#line hidden
#line 3 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Basic.cshtml"
__o = string.Format("{0}", "Hello");

#line default
#line hidden
#line 5 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Basic.cshtml"
   
    var cls = "foo";

#line default
#line hidden
#line 8 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Basic.cshtml"
           if(cls != null) { 

#line default
#line hidden
#line 8 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Basic.cshtml"
                        __o = cls;

#line default
#line hidden
#line 8 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Basic.cshtml"
                                  }

#line default
#line hidden
        }
        #pragma warning restore 1998
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.ViewFeatures.IModelExpressionProvider ModelExpressionProvider { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IUrlHelper Url { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.IViewComponentHelper Component { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IJsonHelper Json { get; private set; }
        [global::Microsoft.AspNetCore.Mvc.Razor.Internal.RazorInjectAttribute]
        public global::Microsoft.AspNetCore.Mvc.Rendering.IHtmlHelper<dynamic> Html { get; private set; }
    }
}
