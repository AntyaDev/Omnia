
namespace Omnia.Parser.Ast
{
    public class FunctionCall : Ast
    {
        public override AstType AstType { get { return AstType.FunctionCall; } }
        public Arg[] Arguments { get; private set; }
        public IdAst Function { get; private set; }

        public FunctionCall(IdAst function, Arg[] args)
        {
            Function = function;
            Arguments = args;
        }
    }
}
