using System.Collections.Generic;

namespace Omnia.Compiller.Ast
{
    class FunctionCall : Ast
    {
        public override AstType AstType { get { return AstType.FunctionCall; } }
        public IEnumerable<Ast> Params { get; private set; }

        public FunctionCall(IEnumerable<Ast> @params)
        {
            Params = @params;
        }
    }
}
