
namespace Omnia.Compiller.Ast
{
    abstract class Expr
    {
        public abstract ExprType ExprType { get; }
    }

    enum ExprType
    {
        ParamExpr,
        Arg,
        FunctionalCall,
        Lambda,
        Literal,
        Open,
        Assign,
        Id
    }
}
