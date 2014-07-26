using System.Linq.Expressions;

namespace Omnia.Parser.Ast
{
    public class BinExpr : Ast
    {
        public override AstType AstType { get { return AstType.BinExpr; } }
        public Ast Left { get; private set; }
        public Ast Right { get; private set; }
        public ExpressionType Operation { get; private set; }

        public BinExpr(Ast left, Ast right, ExpressionType operation)
        {
            Left = left;
            Right = right;
            Operation = operation;
        }
    }
}
