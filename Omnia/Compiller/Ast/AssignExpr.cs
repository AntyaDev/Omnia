
namespace Omnia.Compiller.Ast
{
    class AssignExpr : Expr
    {
        public override ExprType ExprType { get { return ExprType.Assign; } }
        public Expr Left { get; private set; }
        public Expr Right { get; private set; }

        public AssignExpr(Expr left, Expr right)
        {
            Left = left;
            Right = right;
        }
    }
}
