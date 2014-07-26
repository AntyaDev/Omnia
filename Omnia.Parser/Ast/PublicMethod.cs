
namespace Omnia.Parser.Ast
{
    public class PublicMethod : Ast
    {
        public IdAst Name { get; private set; }
        public LamdaDef Lamda { get; private set; }
        public override AstType AstType { get { return AstType.PublicMethod; } }

        public PublicMethod(IdAst name, LamdaDef lamda)
        {
            Name = name;
            Lamda = lamda;
        }
    }
}
