
namespace Omnia.Parser.Ast
{
    public abstract class Ast
    {
        public abstract AstType AstType { get; }
    }

    public enum AstType
    {
        PublicMethod,
        InitClass,
        Class,
        ObjExpr,
        Param,
        Arg,
        FunctionDef,
        FunctionCall,
        PipeCall,
        Lambda,
        Literal,
        OpenExpr,
        Assignment,
        Id,
        BinExpr,
        StmtList
    }
}
