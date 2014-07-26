
namespace Omnia.Parser.Ast
{
    public class IdAst : Ast
    {
        public override AstType AstType { get { return AstType.Id; } }
        public string Name { get; private set; }

        public IdAst(string name)
        {
            Name = name;
        }
    }
}
