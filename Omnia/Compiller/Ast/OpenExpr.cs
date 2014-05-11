using System.Collections.Generic;

namespace Omnia.Compiller.Ast
{
    class OpenExpr : Ast
    {
        public override AstType AstType { get { return AstType.OpenExpr; } }
        public IEnumerable<string> Modules { get; private set; }

        public OpenExpr(IEnumerable<string> modules)
        {
            Modules = modules;
        }
    }
}
