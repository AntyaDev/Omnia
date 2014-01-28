using System.Collections.Generic;

namespace Omnia.Compiller.Ast
{
    class OpenExpr
    {
        public IEnumerable<string> OpenList { get; private set; }

        public OpenExpr(IEnumerable<string> openList)
        {
            OpenList = openList;
        }
    }
}
