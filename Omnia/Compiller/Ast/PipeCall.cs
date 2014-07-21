
namespace Omnia.Compiller.Ast
{
    class PipeCall : Ast
    {
        public override AstType AstType { get { return AstType.PipeCall; } }
        public ObjExpr Function { get; private set; }
        public Arg Argument { get; private set; }

        public PipeCall(ObjExpr function, Arg arg)
        {
            Function = function;
            Argument = arg;
        }
    }
}
