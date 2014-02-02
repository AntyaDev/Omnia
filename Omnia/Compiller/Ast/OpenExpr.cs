using System.Collections.Generic;

namespace Omnia.Compiller.Ast
{
    class OpenExpr : Expr
    {
        public override ExprType ExprType { get { return ExprType.Open; } }
        public IEnumerable<string> Modules { get; private set; }

        public OpenExpr(IEnumerable<string> modules)
        {
            Modules = modules;
        }
    }
}
