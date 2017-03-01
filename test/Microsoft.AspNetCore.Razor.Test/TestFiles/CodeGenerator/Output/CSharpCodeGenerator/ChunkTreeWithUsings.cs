namespace TestNamespace
{
#line 1 ""
using FakeNamespace1

#line default
#line hidden
    ;
#line 1 ""
using FakeNamespace2.SubNamespace

#line default
#line hidden
    ;
    using System.Threading.Tasks;

    public class TestClass
    {
        #line hidden
        public TestClass()
        {
        }

        #pragma warning disable 1998
        public override async System.Threading.Tasks.Task ExecuteAsync()
        {
        }
        #pragma warning restore 1998
    }
}
