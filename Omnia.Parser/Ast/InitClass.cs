
namespace Omnia.Parser.Ast
{
    public class InitClass : Ast
    {
        public override AstType AstType { get { return AstType.InitClass; } }
        public IdAst ClassName { get; private set; }
        public Arg[] Arguments { get; private set; }

        public InitClass(IdAst className, Arg[] arguments)
        {
            ClassName = className;
            Arguments = arguments;
        }
    }
}
