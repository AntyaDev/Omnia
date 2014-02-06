
namespace Omnia.Compiller.Ast
{
    class LiteralExpr : Expr
    {
        public override ExprType ExprType { get { return ExprType.Literal; } }
        public object Value { get; private set; }

        public LiteralExpr(object value)
        {
            Value = value;
        }
    }
}
