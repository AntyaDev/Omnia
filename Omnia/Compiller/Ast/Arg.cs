﻿
namespace Omnia.Compiller.Ast
{
    class Arg : Ast
    {
        public override AstType AstType { get { return AstType.Arg; } }

        public Ast Argument { get; private set; }

        public Arg(Ast argument)
        {
            Argument = argument;
        }
    }
}
