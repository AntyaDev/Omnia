using System.Linq.Expressions;

namespace Omnia.Compiller.Ast
{
    class BinExpr : Ast
    {
        public Ast Left { get; private set; }
        public Ast Right { get; private set; }
        public ExpressionType Operation { get; private set; }
        public override AstType AstType { get { return AstType.BinExpr; } }

        public BinExpr(Ast left, Ast right, ExpressionType operation)
        {
            Left = left;
            Right = right;
            Operation = operation;
        }
    }
}
