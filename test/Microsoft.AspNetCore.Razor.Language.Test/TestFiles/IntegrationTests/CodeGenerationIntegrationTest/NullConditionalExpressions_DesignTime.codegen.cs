namespace Microsoft.AspNetCore.Razor.Language.IntegrationTests.TestFiles
{
    #line hidden
    using System;
    using System.Threading.Tasks;
    public class TestFiles_IntegrationTests_CodeGenerationIntegrationTest_NullConditionalExpressions_DesignTime
    {
        #pragma warning disable 219
        private void __RazorDirectiveTokenHelpers__() {
        }
        #pragma warning restore 219
        private static System.Object __o = null;
        #pragma warning disable 1998
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
              
    
#line 2 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NullConditionalExpressions.cshtml"
__o = ViewBag?.Data;

#line default
#line hidden
                              
    
#line 3 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NullConditionalExpressions.cshtml"
__o = ViewBag.IntIndexer?[0];

#line default
#line hidden
                                       
    
#line 4 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NullConditionalExpressions.cshtml"
__o = ViewBag.StrIndexer?["key"];

#line default
#line hidden
                                           
    
#line 5 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NullConditionalExpressions.cshtml"
__o = ViewBag?.Method(Value?[23]?.More)?["key"];

#line default
#line hidden
                                                          

#line 8 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NullConditionalExpressions.cshtml"
__o = ViewBag?.Data;

#line default
#line hidden
#line 9 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NullConditionalExpressions.cshtml"
__o = ViewBag.IntIndexer?[0];

#line default
#line hidden
#line 10 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NullConditionalExpressions.cshtml"
__o = ViewBag.StrIndexer?["key"];

#line default
#line hidden
#line 11 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NullConditionalExpressions.cshtml"
__o = ViewBag?.Method(Value?[23]?.More)?["key"];

#line default
#line hidden
        }
        #pragma warning restore 1998
    }
}
