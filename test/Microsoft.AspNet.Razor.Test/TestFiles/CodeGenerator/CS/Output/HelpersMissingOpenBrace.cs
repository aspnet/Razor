namespace TestOutput
{
    using System;

    public class HelpersMissingOpenBrace
    {
public static Template 
#line 1 "HelpersMissingOpenBrace.cshtml"
Bold(string s) {

#line default
#line hidden
        return new Template((__razor_helper_writer) => {
#line 1 "HelpersMissingOpenBrace.cshtml"
                        
    s = s.ToUpper();

#line default
#line hidden

            WriteLiteralTo(__razor_helper_writer, "    <strong>");
            WriteTo(__razor_helper_writer, 
#line 3 "HelpersMissingOpenBrace.cshtml"
             s

#line default
#line hidden
            );

            WriteLiteralTo(__razor_helper_writer, "</strong>\r\n");
#line 4 "HelpersMissingOpenBrace.cshtml"

#line default
#line hidden

        }
        );
#line 4 "HelpersMissingOpenBrace.cshtml"
}

#line default
#line hidden

public static Template 
#line 6 "HelpersMissingOpenBrace.cshtml"
Italic(string s) 

#line default
#line hidden

        #line hidden
        public HelpersMissingOpenBrace()
        {
        }

        public override void Execute()
        {
            WriteLiteral("\r\n");
            Write(
#line 7 "HelpersMissingOpenBrace.cshtml"
 Italic(s)

#line default
#line hidden
            );

        }
    }
}
