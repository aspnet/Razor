namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class Templates
    {
#line 1 "Templates.cshtml"

    public HelperResult Repeat(int times, Func<int, object> template) {
        return new HelperResult((writer) => {
            for(int i = 0; i < times; i++) {
                ((HelperResult)template(i)).WriteTo(writer);
            }
        });
    }

#line default
#line hidden
        #line hidden
        public Templates()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
            Instrumentation.BeginContext(280, 2, true);
            WriteLiteral("\r\n");
            Instrumentation.EndContext();
#line 11 "Templates.cshtml"
  
    Func<dynamic, object> foo = 

#line default
#line hidden

            item => new Template((__razor_template_writer) => {
                Instrumentation.BeginContext(325, 11, true);
                WriteLiteralTo(__razor_template_writer, "This works ");
                Instrumentation.EndContext();
                Instrumentation.BeginContext(337, 4, false);
                WriteTo(__razor_template_writer, 
#line 12 "Templates.cshtml"
                                                   item

#line default
#line hidden
                );

                Instrumentation.EndContext();
                Instrumentation.BeginContext(341, 1, true);
                WriteLiteralTo(__razor_template_writer, "!");
                Instrumentation.EndContext();
            }
            )
#line 12 "Templates.cshtml"
                                                               ;
    

#line default
#line hidden

            Instrumentation.BeginContext(357, 7, false);
            Write(
#line 13 "Templates.cshtml"
     foo("")

#line default
#line hidden
            );

            Instrumentation.EndContext();
#line 13 "Templates.cshtml"
            

#line default
#line hidden

            Instrumentation.BeginContext(367, 10, true);
            WriteLiteral("\r\n\r\n<ul>\r\n");
            Instrumentation.EndContext();
            Instrumentation.BeginContext(379, 11, false);
            Write(
#line 17 "Templates.cshtml"
  Repeat(10, 

#line default
#line hidden
            item => new Template((__razor_template_writer) => {
                Instrumentation.BeginContext(391, 10, true);
                WriteLiteralTo(__razor_template_writer, "<li>Item #");
                Instrumentation.EndContext();
                Instrumentation.BeginContext(402, 4, false);
                WriteTo(__razor_template_writer, 
#line 17 "Templates.cshtml"
                         item

#line default
#line hidden
                );

                Instrumentation.EndContext();
                Instrumentation.BeginContext(406, 5, true);
                WriteLiteralTo(__razor_template_writer, "</li>");
                Instrumentation.EndContext();
            }
            )
#line 17 "Templates.cshtml"
                                  )

#line default
#line hidden
            );

            Instrumentation.EndContext();
            Instrumentation.BeginContext(413, 16, true);
            WriteLiteral("\r\n</ul>\r\n\r\n<p>\r\n");
            Instrumentation.EndContext();
            Instrumentation.BeginContext(430, 16, false);
            Write(
#line 21 "Templates.cshtml"
 Repeat(10,
    

#line default
#line hidden
            item => new Template((__razor_template_writer) => {
                Instrumentation.BeginContext(448, 14, true);
                WriteLiteralTo(__razor_template_writer, " This is line#");
                Instrumentation.EndContext();
                Instrumentation.BeginContext(463, 4, false);
                WriteTo(__razor_template_writer, 
#line 22 "Templates.cshtml"
                     item

#line default
#line hidden
                );

                Instrumentation.EndContext();
                Instrumentation.BeginContext(467, 17, true);
                WriteLiteralTo(__razor_template_writer, " of markup<br/>\r\n");
                Instrumentation.EndContext();
            }
            )
#line 23 "Templates.cshtml"
)

#line default
#line hidden
            );

            Instrumentation.EndContext();
            Instrumentation.BeginContext(485, 20, true);
            WriteLiteral("\r\n</p>\r\n\r\n<ul>\r\n    ");
            Instrumentation.EndContext();
            Instrumentation.BeginContext(506, 11, false);
            Write(
#line 27 "Templates.cshtml"
     Repeat(10, 

#line default
#line hidden
            item => new Template((__razor_template_writer) => {
                Instrumentation.BeginContext(518, 20, true);
                WriteLiteralTo(__razor_template_writer, "<li>\r\n        Item #");
                Instrumentation.EndContext();
                Instrumentation.BeginContext(539, 4, false);
                WriteTo(__razor_template_writer, 
#line 28 "Templates.cshtml"
               item

#line default
#line hidden
                );

                Instrumentation.EndContext();
                Instrumentation.BeginContext(543, 2, true);
                WriteLiteralTo(__razor_template_writer, "\r\n");
                Instrumentation.EndContext();
#line 29 "Templates.cshtml"
        

#line default
#line hidden

#line 29 "Templates.cshtml"
          var parent = item;

#line default
#line hidden

                Instrumentation.BeginContext(574, 93, true);
                WriteLiteralTo(__razor_template_writer, "\r\n        <ul>\r\n            <li>Child Items... ?</li>\r\n            \r\n        </ul" +
">\r\n    </li>");
                Instrumentation.EndContext();
            }
            )
#line 34 "Templates.cshtml"
         )

#line default
#line hidden
            );

            Instrumentation.EndContext();
            Instrumentation.BeginContext(715, 8, true);
            WriteLiteral("\r\n</ul> ");
            Instrumentation.EndContext();
        }
        #pragma warning restore 1998
    }
}
