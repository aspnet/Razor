namespace TestOutput
{
    using System;
    using System.Threading.Tasks;

    public class Await
    {
        private static object @__o;
#line 1 "Await.cshtml"

    public async Task<string> Foo()
    {
        return "Bar";
    }

#line default
#line hidden
        private void @__RazorDesignTimeHelpers__()
        {
            #pragma warning disable 219
            #pragma warning restore 219
        }
        #line hidden
        public Await()
        {
        }

        public override async Task ExecuteAsync()
        {
#line 1 "------------------------------------------"
								 __o = await Foo();

#line default
#line hidden
#line 1 "------------------------------------------"
								__o = await Foo();

#line default
#line hidden
#line 12 "Await.cshtml"
									    await Foo(); 

#line default
#line hidden

#line 13 "Await.cshtml"
											   

#line default
#line hidden

#line 1 "------------------------------------------"
											 __o = await Foo();

#line default
#line hidden
#line 13 "Await.cshtml"
																   

#line default
#line hidden

#line 1 "------------------------------------------"
										   __o = await;

#line default
#line hidden
#line 1 "------------------------------------------"
									__o = await Foo(1, 2);

#line default
#line hidden
#line 1 "------------------------------------------"
								   __o = await Foo("bob", true);

#line default
#line hidden
#line 21 "Await.cshtml"
										   await Foo(something, hello: "world"); 

#line default
#line hidden

#line 22 "Await.cshtml"
												  

#line default
#line hidden

#line 1 "------------------------------------------"
												__o = await Foo(boolValue: false);

#line default
#line hidden
#line 22 "Await.cshtml"
																					  

#line default
#line hidden

#line 1 "------------------------------------------"
											  __o = await ("wrrronggg");

#line default
#line hidden
        }
    }
}
