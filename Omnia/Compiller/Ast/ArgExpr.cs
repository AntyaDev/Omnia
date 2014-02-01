
namespace Omnia.Compiller.Ast
{
    class ArgExpr : Expr
    {
        public override ExprType ExprType { get { return ExprType.Arg; } }
        public string Name { get; private set; }

        public ArgExpr(string name)
        {
            Name = name;
        }
    }
}
