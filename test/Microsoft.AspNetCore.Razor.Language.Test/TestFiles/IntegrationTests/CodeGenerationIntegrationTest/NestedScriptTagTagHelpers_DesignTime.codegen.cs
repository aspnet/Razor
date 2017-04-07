namespace Microsoft.AspNetCore.Razor.Language.IntegrationTests.TestFiles
{
    #line hidden
    using System;
    using System.Threading.Tasks;
    public class TestFiles_IntegrationTests_CodeGenerationIntegrationTest_NestedScriptTagTagHelpers_DesignTime
    {
        #pragma warning disable 219
        private void __RazorDirectiveTokenHelpers__() {
        ((System.Action)(() => {
global::System.Object __typeHelper = "*, TestAssembly";
        }
        ))();
        }
        #pragma warning restore 219
        private static System.Object __o = null;
        private global::TestNamespace.PTagHelper __TestNamespace_PTagHelper = null;
        private global::TestNamespace.InputTagHelper __TestNamespace_InputTagHelper = null;
        private global::TestNamespace.InputTagHelper2 __TestNamespace_InputTagHelper2 = null;
        #pragma warning disable 1998
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
#line 6 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NestedScriptTagTagHelpers.cshtml"
             for(var i = 0; i < 5; i++) {
                

#line default
#line hidden
            __TestNamespace_InputTagHelper = CreateTagHelper<global::TestNamespace.InputTagHelper>();
            __TestNamespace_InputTagHelper2 = CreateTagHelper<global::TestNamespace.InputTagHelper2>();
#line 8 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NestedScriptTagTagHelpers.cshtml"
                                            __o = ViewBag.DefaultInterval;

#line default
#line hidden
            __TestNamespace_InputTagHelper.Type = "text";
            __TestNamespace_InputTagHelper2.Type = __TestNamespace_InputTagHelper.Type;
#line 8 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NestedScriptTagTagHelpers.cshtml"
                                                          __TestNamespace_InputTagHelper2.Checked = true;

#line default
#line hidden
#line 9 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NestedScriptTagTagHelpers.cshtml"
                         
            }

#line default
#line hidden
            __TestNamespace_PTagHelper = CreateTagHelper<global::TestNamespace.PTagHelper>();
        }
        #pragma warning restore 1998
    }
}
