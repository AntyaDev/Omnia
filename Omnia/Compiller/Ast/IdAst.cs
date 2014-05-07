
namespace Omnia.Compiller.Ast
{
    class IdAst : Ast
    {
        public override AstType AstType { get { return AstType.Id; } }
        public string Name { get; private set; }

        public IdAst(string name)
        {
            Name = name;
        }
    }
}
