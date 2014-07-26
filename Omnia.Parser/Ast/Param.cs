
namespace Omnia.Parser.Ast
{
    public class Param : Ast
    {
        public override AstType AstType { get { return AstType.Param; } }
        public string Name { get; private set; }

        public Param(IdAst name)
        {
            Name = name.Name;
        }
    }
}
