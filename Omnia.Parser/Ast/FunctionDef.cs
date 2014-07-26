
namespace Omnia.Parser.Ast
{
    public class FunctionDef : Ast
    {
        public override AstType AstType { get { return AstType.FunctionDef; } }
        public string Name { get; private set; }
        public LamdaDef Lamda { get; private set; }

        public FunctionDef(IdAst name, LamdaDef lamda)
        {
            Name = name.Name;
            Lamda = lamda;
        }
    }
}
