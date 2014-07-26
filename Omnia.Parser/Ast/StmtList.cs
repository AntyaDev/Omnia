
namespace Omnia.Parser.Ast
{
    public class StmtList : Ast
    {
        public override AstType AstType { get { return AstType.StmtList; } }
        public Ast[] List { get; private set; }

        public StmtList(Ast[] list)
        {
            List = list;
        }
    }
}
