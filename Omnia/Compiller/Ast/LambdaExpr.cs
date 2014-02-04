using System.Collections.Generic;

namespace Omnia.Compiller.Ast
{
    class LambdaExpr : Expr
    {
        public override ExprType ExprType { get { return ExprType.Lambda; } }
        public IEnumerable<ArgExpr> Args { get; private set; }
        public IEnumerable<Expr> Body { get; set; }

        public LambdaExpr(IEnumerable<ArgExpr> args, IEnumerable<Expr> body)
        {
            Args = args;
            Body = body;
        }
    }
}
