namespace Microsoft.AspNetCore.TestGenerated
{
    #line hidden
    using System;
    using System.Threading.Tasks;
    internal class TestView : Microsoft.Extensions.RazorViews.BaseView
    {
        #pragma warning disable 1998
        public async override global::System.Threading.Tasks.Task ExecuteAsync()
        {
            WriteLiteral(@"The time is ");
#line 1 "TestView.cshtml"
       Write(DateTime.UtcNow);

#line default
#line hidden
            WriteLiteral(@"
window.alert(""Hello world"");
Footer goes here.");
        }
        #pragma warning restore 1998
    }
}
