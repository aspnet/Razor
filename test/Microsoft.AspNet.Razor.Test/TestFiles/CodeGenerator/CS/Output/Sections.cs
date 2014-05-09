namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class Sections
    {
        #line hidden
        public Sections()
        {
        }

        #pragma warning disable 1998
        public override async Task ExecuteAsync()
        {
#line 1 "Sections.cshtml"
  
    Layout = "_SectionTestLayout.cshtml"

#line default
#line hidden

            WriteLiteral("\r\n\r\n<div>This is in the Body>\r\n\r\n");
            DefineSection("Section2", new Template((__razor_template_writer) => {
                WriteLiteralTo(__razor_template_writer, "\r\n    <div>This is in Section 2</div>\r\n");
            }
            ));
            WriteLiteral("\r\n");
            DefineSection("Section1", new Template((__razor_template_writer) => {
                WriteLiteralTo(__razor_template_writer, "\r\n    <div>This is in Section 1</div>\r\n");
            }
            ));
        }
        #pragma warning restore 1998
    }
}
