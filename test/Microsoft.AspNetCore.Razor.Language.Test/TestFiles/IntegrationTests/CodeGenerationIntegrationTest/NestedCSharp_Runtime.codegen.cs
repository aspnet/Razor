#pragma checksum "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NestedCSharp.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "2b9e8dcf7c08153c15ac84973938a7c0254f2369"
namespace Microsoft.AspNetCore.Razor.Language.IntegrationTests.TestFiles
{
    #line hidden
    using System;
    using System.Threading.Tasks;
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class TestFiles_IntegrationTests_CodeGenerationIntegrationTest_NestedCSharp_Runtime
    {
        #pragma warning disable 1998
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
#line 2 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NestedCSharp.cshtml"
     foreach (var result in (dynamic)Url)
    {

#line default
#line hidden
            WriteLiteral("        <div>\r\n            ");
#line 5 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NestedCSharp.cshtml"
       Write(result.SomeValue);

#line default
#line hidden
            WriteLiteral(".\r\n        </div>\r\n");
#line 7 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/NestedCSharp.cshtml"
    }

#line default
#line hidden
        }
        #pragma warning restore 1998
    }
}
