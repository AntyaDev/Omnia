
namespace Omnia.Compiller.Ast
{
    class ParamExpr : Expr
    {
        public override ExprType ExprType { get { return ExprType.ParamExpr; } }
        public Expr Value { get; private set; }

        public ParamExpr(Expr value)
        {
            Value = value;
        }
    }
}
