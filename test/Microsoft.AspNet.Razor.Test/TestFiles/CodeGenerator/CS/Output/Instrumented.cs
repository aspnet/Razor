namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class Instrumented
    {
public static Template 
#line 1 "Instrumented.cshtml"
Strong(string s)
{

#line default
#line hidden
        return new Template((__razor_helper_writer) => {
#line 2 "Instrumented.cshtml"
 

#line default
#line hidden

            BeginContext(__razor_helper_writer, "~/Instrumented.cshtml", 29, 12, true);
            WriteLiteralTo(__razor_helper_writer, "            <strong>");
            EndContext(__razor_helper_writer, "~/Instrumented.cshtml", 29, 12, true);
            BeginContext(__razor_helper_writer, "~/Instrumented.cshtml", 50, 1, false);
            WriteTo(__razor_helper_writer, 
#line 3 "Instrumented.cshtml"
                     s

#line default
#line hidden
            );

            EndContext(__razor_helper_writer, "~/Instrumented.cshtml", 50, 1, false);
            BeginContext(__razor_helper_writer, "~/Instrumented.cshtml", 51, 9, true);
            WriteLiteralTo(__razor_helper_writer, "</strong>\r\n");
            EndContext(__razor_helper_writer, "~/Instrumented.cshtml", 51, 9, true);
#line 4 "Instrumented.cshtml"

#line default
#line hidden

        }
        );
#line 4 "Instrumented.cshtml"
}

#line default
#line hidden

        #line hidden
        public Instrumented()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            BeginContext("~/Instrumented.cshtml", 65, 2, true);
            WriteLiteral("\r\n");
            EndContext("~/Instrumented.cshtml", 65, 2, true);
#line 6 "Instrumented.cshtml"
  
    int i = 1;
    var foo = 

#line default
#line hidden

            item => new Template((__razor_template_writer) => {
                BeginContext(__razor_template_writer, "~/Instrumented.cshtml", 102, 3, true);
                WriteLiteralTo(__razor_template_writer, "<p>Bar</p>");
                EndContext(__razor_template_writer, "~/Instrumented.cshtml", 102, 3, true);
            }
            )
#line 8 "Instrumented.cshtml"
                         ;

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 115, 4, true);
            WriteLiteral("    Hello, World\r\n    <p>Hello, World</p>\r\n");
            EndContext("~/Instrumented.cshtml", 115, 4, true);
#line 11 "Instrumented.cshtml"

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 161, 4, true);
            WriteLiteral("\r\n\r\n");
            EndContext("~/Instrumented.cshtml", 161, 4, true);
#line 13 "Instrumented.cshtml"
 while (i <= 10)
{

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 186, 4, true);
            WriteLiteral("    <p>Hello from C#, #");
            EndContext("~/Instrumented.cshtml", 186, 4, true);
            BeginContext("~/Instrumented.cshtml", 211, 1, false);
            Write(
#line 15 "Instrumented.cshtml"
                         i

#line default
#line hidden
            );

            EndContext("~/Instrumented.cshtml", 211, 1, false);
            BeginContext("~/Instrumented.cshtml", 213, 4, true);
            WriteLiteral("</p>\r\n");
            EndContext("~/Instrumented.cshtml", 213, 4, true);
#line 16 "Instrumented.cshtml"
    i += 1;
}

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 235, 2, true);
            WriteLiteral("\r\n");
            EndContext("~/Instrumented.cshtml", 235, 2, true);
#line 19 "Instrumented.cshtml"
 if (i == 11)
{

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 255, 4, true);
            WriteLiteral("    <p>We wrote 10 lines!</p>\r\n");
            EndContext("~/Instrumented.cshtml", 255, 4, true);
#line 22 "Instrumented.cshtml"
}

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 289, 2, true);
            WriteLiteral("\r\n");
            EndContext("~/Instrumented.cshtml", 289, 2, true);
#line 24 "Instrumented.cshtml"
 switch (i)
{
    case 11:

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 321, 8, true);
            WriteLiteral("        <p>No really, we wrote 10 lines!</p>\r\n");
            EndContext("~/Instrumented.cshtml", 321, 8, true);
#line 28 "Instrumented.cshtml"
        break;
    default:

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 397, 8, true);
            WriteLiteral("        <p>Actually, we didn\'t...</p>\r\n");
            EndContext("~/Instrumented.cshtml", 397, 8, true);
#line 31 "Instrumented.cshtml"
        break;
}

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 455, 2, true);
            WriteLiteral("\r\n");
            EndContext("~/Instrumented.cshtml", 455, 2, true);
#line 34 "Instrumented.cshtml"
 for (int j = 1; j <= 10; j += 2)
{

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 495, 4, true);
            WriteLiteral("    <p>Hello again from C#, #");
            EndContext("~/Instrumented.cshtml", 495, 4, true);
            BeginContext("~/Instrumented.cshtml", 526, 1, false);
            Write(
#line 36 "Instrumented.cshtml"
                               j

#line default
#line hidden
            );

            EndContext("~/Instrumented.cshtml", 526, 1, false);
            BeginContext("~/Instrumented.cshtml", 528, 4, true);
            WriteLiteral("</p>\r\n");
            EndContext("~/Instrumented.cshtml", 528, 4, true);
#line 37 "Instrumented.cshtml"
}

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 537, 2, true);
            WriteLiteral("\r\n");
            EndContext("~/Instrumented.cshtml", 537, 2, true);
#line 39 "Instrumented.cshtml"
 try
{

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 548, 4, true);
            WriteLiteral("    <p>That time, we wrote 5 lines!</p>\r\n");
            EndContext("~/Instrumented.cshtml", 548, 4, true);
#line 42 "Instrumented.cshtml"
}
catch (Exception ex)
{

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 617, 4, true);
            WriteLiteral("    <p>Oh no! An error occurred: ");
            EndContext("~/Instrumented.cshtml", 617, 4, true);
            BeginContext("~/Instrumented.cshtml", 652, 10, false);
            Write(
#line 45 "Instrumented.cshtml"
                                   ex.Message

#line default
#line hidden
            );

            EndContext("~/Instrumented.cshtml", 652, 10, false);
            BeginContext("~/Instrumented.cshtml", 663, 4, true);
            WriteLiteral("</p>\r\n");
            EndContext("~/Instrumented.cshtml", 663, 4, true);
#line 46 "Instrumented.cshtml"
}

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 672, 2, true);
            WriteLiteral("\r\n");
            EndContext("~/Instrumented.cshtml", 672, 2, true);
#line 48 "Instrumented.cshtml"
 lock (new object())
{

#line default
#line hidden

            BeginContext("~/Instrumented.cshtml", 699, 4, true);
            WriteLiteral("    <p>This block is locked, for your security!</p>\r\n");
            EndContext("~/Instrumented.cshtml", 699, 4, true);
#line 51 "Instrumented.cshtml"
}

#line default
#line hidden

        }
        #pragma warning restore 1998
    }
}
