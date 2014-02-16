using System;
using Omnia.Compiller.Ast;
using Omnia.Compiller.Infrastructure;
using Omnia.Compiller.Scopes;
using Omnia.Compiller.TypeSystem;

namespace Omnia.Compiller.Visitors
{
    class SymbolTable : AstVisitor
    {
        readonly Scope _scope;

        public SymbolTable(CompillerContext context, Scope scope) : base(context)
        {
            _scope = scope;
        }

        protected override void VisitIdExpr(IdExpr idExpr)
        {
            if (_scope.TryGet(idExpr.Name).IsNone) _scope.Put(new LocalVar(idExpr.Name));
        }
        
        protected override void VisitLambdaExpr(LambdaExpr expr)
        {
            _scope.Put(new LocalVar("Compiller_Lambda_" + Guid.NewGuid()));
            _scope.
            base.VisitLambdaExpr(expr);
        }
    }
}
