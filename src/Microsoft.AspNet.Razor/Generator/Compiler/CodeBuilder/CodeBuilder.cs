
namespace Microsoft.AspNet.Razor.Generator.Compiler
{
    public abstract class CodeBuilder
    {
        private readonly CodeGeneratorContext _codeGeneratorContext;

        public CodeBuilder(CodeGeneratorContext codeGeneratorContext)
        {
            _codeGeneratorContext = codeGeneratorContext;
        }

        protected CodeGeneratorContext Context
        {
            get { return _codeGeneratorContext; }
        }

        public abstract CodeBuilderResult Build();
    }
}
