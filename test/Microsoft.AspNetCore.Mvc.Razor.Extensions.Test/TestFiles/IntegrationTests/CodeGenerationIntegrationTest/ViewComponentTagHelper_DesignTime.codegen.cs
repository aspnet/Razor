[assembly:global::Microsoft.AspNetCore.Mvc.Razor.Compilation.RazorViewAttribute(null, typeof(AspNetCore.TestFiles_IntegrationTests_CodeGenerationIntegrationTest_ViewComponentTagHelper_cshtml))]
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
    public class TestFiles_IntegrationTests_CodeGenerationIntegrationTest_ViewComponentTagHelper_cshtml : global::Microsoft.AspNetCore.Mvc.Razor.RazorPage<dynamic>
    {
        #pragma warning disable 219
        private void __RazorDirectiveTokenHelpers__() {
        ((System.Action)(() => {
global::System.Object __typeHelper = "*, AppCode";
        }
        ))();
        }
        #pragma warning restore 219
        private static System.Object __o = null;
        private global::AspNetCore.TestFiles_IntegrationTests_CodeGenerationIntegrationTest_ViewComponentTagHelper_cshtml.__Generated__TestViewComponentTagHelper __AspNetCore_TestFiles_IntegrationTests_CodeGenerationIntegrationTest_ViewComponentTagHelper_cshtml___Generated__TestViewComponentTagHelper;
        private global::AllTagHelper __AllTagHelper;
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
#line 2 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/ViewComponentTagHelper.cshtml"
  
    var foo = "Hello";

#line default
#line hidden
            __AspNetCore_TestFiles_IntegrationTests_CodeGenerationIntegrationTest_ViewComponentTagHelper_cshtml___Generated__TestViewComponentTagHelper = CreateTagHelper<global::AspNetCore.TestFiles_IntegrationTests_CodeGenerationIntegrationTest_ViewComponentTagHelper_cshtml.__Generated__TestViewComponentTagHelper>();
            __AllTagHelper = CreateTagHelper<global::AllTagHelper>();
#line 6 "TestFiles/IntegrationTests/CodeGenerationIntegrationTest/ViewComponentTagHelper.cshtml"
                __o = foo;

#line default
#line hidden
            __AspNetCore_TestFiles_IntegrationTests_CodeGenerationIntegrationTest_ViewComponentTagHelper_cshtml___Generated__TestViewComponentTagHelper.firstName = string.Empty;
            __AllTagHelper.Bar = " World";
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
        [Microsoft.AspNetCore.Razor.TagHelpers.HtmlTargetElementAttribute("vc:test")]
public class __Generated__TestViewComponentTagHelper : Microsoft.AspNetCore.Razor.TagHelpers.TagHelper
{
    private readonly global::Microsoft.AspNetCore.Mvc.IViewComponentHelper _helper = null;
    public __Generated__TestViewComponentTagHelper(global::Microsoft.AspNetCore.Mvc.IViewComponentHelper helper)
    {
        _helper = helper;
    }
    [Microsoft.AspNetCore.Razor.TagHelpers.HtmlAttributeNotBoundAttribute, global::Microsoft.AspNetCore.Mvc.ViewFeatures.ViewContextAttribute]
    public global::Microsoft.AspNetCore.Mvc.Rendering.ViewContext ViewContext { get; set; }
    public System.String firstName { get; set; }
    public override async global::System.Threading.Tasks.Task ProcessAsync(Microsoft.AspNetCore.Razor.TagHelpers.TagHelperContext context, Microsoft.AspNetCore.Razor.TagHelpers.TagHelperOutput output)
    {
        (_helper as global::Microsoft.AspNetCore.Mvc.ViewFeatures.IViewContextAware)?.Contextualize(ViewContext);
        var content = await _helper.InvokeAsync("Test", new { firstName });
        output.TagName = null;
        output.Content.SetHtmlContent(content);
    }
}

    }
}
