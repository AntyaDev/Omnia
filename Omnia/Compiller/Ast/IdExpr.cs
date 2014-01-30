
namespace Omnia.Compiller.Ast
{
    class IdExpr : Expr
    {
        public override ExprType ExprType { get { return ExprType.Id; } }
        public string Name { get; private set; }

        public IdExpr(string name)
        {
            Name = name;
        }
    }
}
