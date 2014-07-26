
namespace Omnia.Parser.Ast
{
    public class ObjExpr : Ast
    {
        public override AstType AstType { get { return AstType.ObjExpr; } }

        public Ast[] Objects { get; private set; }

        public bool IsDottedExpr { get; private set; }

        public ObjExpr(Ast[] objects)
        {
            Objects = objects;
            IsDottedExpr = Objects.Length > 1;
        }

        public ObjExpr(Ast obj) : this(new[] { obj })
        { }
    }
}
