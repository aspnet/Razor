#pragma checksum "RazorComments.cshtml" "{ff1816ec-aa5e-4d10-87f7-6f4963833460}" "051345e2cc0313fea445db2f6cf48fe28b0b4edf"
namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class RazorComments
    {
        #line hidden
        public RazorComments()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            Instrumentation.BeginContext(0, 34, true);
            WriteLiteral("\r\n<p>This should  be shown</p>\r\n\r\n");
            Instrumentation.EndContext();
#line 4 "RazorComments.cshtml"
  
    

#line default
#line hidden

#line 5 "RazorComments.cshtml"
                                       
    Exception foo = 

#line default
#line hidden

#line 6 "RazorComments.cshtml"
                                                  null;
    if(foo != null) {
        throw foo;
    }

#line default
#line hidden

            Instrumentation.BeginContext(232, 4, true);
            WriteLiteral("\r\n\r\n");
            Instrumentation.EndContext();
#line 12 "RazorComments.cshtml"
   var bar = "@* bar *@"; 

#line default
#line hidden

            Instrumentation.BeginContext(263, 46, true);
            WriteLiteral("\r\n<p>But this should show the comment syntax: ");
            Instrumentation.EndContext();
            Instrumentation.BeginContext(310, 3, false);
#line 13 "RazorComments.cshtml"
                                       Write(bar);

#line default
#line hidden
            Instrumentation.EndContext();
            Instrumentation.BeginContext(313, 8, true);
            WriteLiteral("</p>\r\n\r\n");
            Instrumentation.EndContext();
            Instrumentation.BeginContext(323, 1, false);
#line 15 "RazorComments.cshtml"
Write(ab);

#line default
#line hidden
            Instrumentation.EndContext();
            Instrumentation.BeginContext(330, 2, true);
            WriteLiteral("\r\n");
            Instrumentation.EndContext();
        }
        #pragma warning restore 1998
    }
}
