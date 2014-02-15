using System.Collections.Generic;
using Omnia.Compiller.Ast;
using Omnia.Compiller.Infrastructure;
using Omnia.Infrastructure;

namespace Omnia.Compiller.Visitors
{
    abstract class AstVisitor
    {
        protected CompillerContext Context;

        protected AstVisitor(CompillerContext context)
        {
            Context = context;
        }

        public CompillerContext Run(CompillerContext context)
        {
            VisitAst(context.Ast);
            return context;
        }

        void VisitAst(IEnumerable<Expr> ast)
        {
            foreach (var expr in ast)
            {
                Visit(expr);
            }
        }

        protected virtual void VisitOpenExpr(OpenExpr expr)
        { }

        protected virtual void VisitAssignExpr(AssignExpr expr)
        {
            Visit(expr.Left);
            Visit(expr.Right);
        }

        protected virtual void VisitIdExpr(IdExpr idExpr)
        { }

        protected virtual void VisitLambdaExpr(LambdaExpr expr)
        {
            expr.Args.ForEach(Visit);
            expr.Body.ForEach(Visit);
        }

        protected virtual void VisitArgExpr(ArgExpr argExpr)
        { }

        void Visit(Expr expr)
        {
            switch (expr.ExprType)
            {
                case ExprType.Open: VisitOpenExpr(expr as OpenExpr); break;
                case ExprType.Assign: VisitAssignExpr(expr as AssignExpr); break;
                case ExprType.Id: VisitIdExpr(expr as IdExpr); break;
                case ExprType.Lambda: VisitLambdaExpr(expr as LambdaExpr); break;
                case ExprType.Arg: VisitArgExpr(expr as ArgExpr); break;
            }
        }
    }
}
