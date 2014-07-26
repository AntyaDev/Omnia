
namespace Omnia.Parser.Ast
{
    public class Assignment : Ast
    {
        public override AstType AstType { get { return AstType.Assignment; } }
        public IdAst Left { get; private set; }
        public Ast Right { get; private set; }

        public Assignment(IdAst left, Ast right)
        {
            Left = left;
            Right = right;
        }
    }
}
