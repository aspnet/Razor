#pragma checksum "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/MarkupInCodeBlock.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "cf059b36d7e93e260c1d5b852f7a59e6c99ae33d"
namespace Microsoft.AspNetCore.Razor.Language.IntegrationTests.TestFiles
{
    #line hidden
    using System;
    using System.Threading.Tasks;
    public class TestFiles_IntegrationTests_CodeGenerationIntegrationTest_MarkupInCodeBlock_Runtime
    {
        #pragma warning disable 1998
        public async System.Threading.Tasks.Task ExecuteAsync()
        {
#line 1 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/MarkupInCodeBlock.cshtml"
  
    for(int i = 1; i <= 10; i++) {

#line default
#line hidden
            WriteLiteral("        <p>Hello from C#, #");
#line 3 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/MarkupInCodeBlock.cshtml"
                       Write(i.ToString());

#line default
#line hidden
            WriteLiteral("</p>\r\n");
#line 4 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/MarkupInCodeBlock.cshtml"
    }

#line default
#line hidden
        }
        #pragma warning restore 1998
    }
}
