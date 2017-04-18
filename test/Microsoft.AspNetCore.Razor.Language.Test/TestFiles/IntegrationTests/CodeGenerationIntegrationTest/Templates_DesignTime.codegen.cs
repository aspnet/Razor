namespace Microsoft.AspNetCore.Razor.Language.IntegrationTests.TestFiles
{
    #line hidden
    using System;
    using System.Threading.Tasks;
    public class TestFiles_IntegrationTests_CodeGenerationIntegrationTest_Templates_DesignTime
    {
        #pragma warning disable 219
        private void __RazorDirectiveTokenHelpers__() {
        }
        #pragma warning restore 219
        private static System.Object __o = null;
        #pragma warning disable 1998
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
#line 11 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
  
    Func<dynamic, object> foo = 

#line default
#line hidden
            item => new Microsoft.AspNetCore.Mvc.Razor.HelperResult(async(__razor_template_writer) => {
#line 12 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
                                             __o = item;

#line default
#line hidden
            }
            )
#line 12 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
                                                               ;
    

#line default
#line hidden
#line 13 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
__o = foo("");

#line default
#line hidden
                        

#line 16 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
   
    Func<dynamic, object> bar = 

#line default
#line hidden
            item => new Microsoft.AspNetCore.Mvc.Razor.HelperResult(async(__razor_template_writer) => {
#line 17 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
                                      __o = item;

#line default
#line hidden
            }
            )
#line 17 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
                                                           ;
    

#line default
#line hidden
#line 18 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
__o = bar("myclass");

#line default
#line hidden
                               

#line 22 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
__o = Repeat(10, item => new Microsoft.AspNetCore.Mvc.Razor.HelperResult(async(__razor_template_writer) => {
#line 22 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
                   __o = item;

#line default
#line hidden
}
));

#line default
#line hidden
#line 26 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
__o = Repeat(10,
    item => new Microsoft.AspNetCore.Mvc.Razor.HelperResult(async(__razor_template_writer) => {
#line 27 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
               __o = item;

#line default
#line hidden
}
));

#line default
#line hidden
#line 32 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
__o = Repeat(10,
    item => new Microsoft.AspNetCore.Mvc.Razor.HelperResult(async(__razor_template_writer) => {
#line 33 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
                __o = item;

#line default
#line hidden
}
));

#line default
#line hidden
#line 38 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
__o = Repeat(10,
    item => new Microsoft.AspNetCore.Mvc.Razor.HelperResult(async(__razor_template_writer) => {
#line 39 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
                 __o = item;

#line default
#line hidden
}
));

#line default
#line hidden
#line 45 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
__o = Repeat(10, item => new Microsoft.AspNetCore.Mvc.Razor.HelperResult(async(__razor_template_writer) => {
#line 46 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
         __o = item;

#line default
#line hidden
#line 47 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
          var parent = item;

#line default
#line hidden
}
));

#line default
#line hidden
        }
        #pragma warning restore 1998
#line 1 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/Templates.cshtml"
            
    public HelperResult Repeat(int times, Func<int, object> template) {
        return new HelperResult((writer) => {
            for(int i = 0; i < times; i++) {
                ((HelperResult)template(i)).WriteTo(writer);
            }
        });
    }

#line default
#line hidden
    }
}
