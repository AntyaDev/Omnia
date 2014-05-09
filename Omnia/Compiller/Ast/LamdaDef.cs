using System.Collections.Generic;

namespace Omnia.Compiller.Ast
{
    class LamdaDef : Ast
    {
        public override AstType AstType { get { return AstType.Lambda; } }
        public Param[] Parameters { get; private set; }
        public Ast Body { get; set; }

        public LamdaDef(Param[] parameters, Ast body)
        {
            Parameters = parameters;
            Body = body;
        }
    }
}
