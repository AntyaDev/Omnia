
namespace Omnia.Compiller.Ast
{
    abstract class Expr
    {
        public abstract ExprType ExprType { get; }
    }

    enum ExprType
    {
        Argument,
        FunctionalCall,
        Lambda,
        Literal,
        Open,
        Assign
    }
}
