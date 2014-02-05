using System.Collections.Generic;

namespace Omnia.Compiller.Ast
{
    class FunctionCallExpr : Expr
    {
        public override ExprType ExprType { get { return ExprType.FunctionalCall; } }
        public IEnumerable<Expr> Params { get; private set; }

        public FunctionCallExpr(IEnumerable<Expr> @params)
        {
            Params = @params;
        }
    }
}
