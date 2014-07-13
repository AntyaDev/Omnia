﻿namespace Omnia.Compiller.Ast
{
    class ClassDef : Ast
    {
        public override AstType AstType { get { return AstType.Class; } }
        public string Module { get; private set; }
        public string Name { get; private set; }
        public Param[] Parameters {get; private set; }
        public StmtList Body { get; private set; }

        public ClassDef(IdAst module, IdAst name, Param[] parameters, StmtList body)
        {
            Module = module.Name;
            Name = name.Name;
            Parameters = parameters;
            Body = body;
        }
    }
}
