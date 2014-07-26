using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Linq.Expressions;
using FunctionalWeapon.Monads;
using Omnia.CodeGen.Runtime;
using Omnia.CodeGen.Runtime.Binders;
using Omnia.CodeGen.TypeSystem;
using Omnia.Parser.Ast;

namespace Omnia.CodeGen
{
    public class DLRCodeGen
    {
        readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();

        public IEnumerable<Expression> GenerateDLRAst(IEnumerable<Ast> ast, AnalysisScope scope)
        {
            return ast.Select(node => AnalyzeNode(node, scope));
        }

        Expression AnalyzeNode(Ast node, AnalysisScope scope)
        {
            switch (node.AstType)
            {
                case AstType.OpenExpr: return AnalyzeOpenExpr(node as OpenExpr, scope);
                case AstType.Id: return AnalyzeIdentifier(node as IdAst, scope);
                case AstType.FunctionDef: return AnalyzeFunctionDef(node as FunctionDef, scope);
                case AstType.ObjExpr: return AnalyzeObjExpr(node as ObjExpr, scope);
                case AstType.FunctionCall: return AnalyzeFunctionCall(node as FunctionCall, scope);
                case AstType.PipeCall: return AnalyzePipeCall(node as PipeCall, scope);
                case AstType.BinExpr: return AnalyzeBinExpr(node as BinExpr, scope);
                case AstType.Assignment: return AnalyzeAssignment(node as Assignment, scope);
                case AstType.Arg: return AnalyzeNode((node as Arg).Argument, scope);
                case AstType.Literal: return AnalyzeLiteral(node as Literal, scope);
                case AstType.StmtList: return AnalyzeStmtList(node as StmtList, scope);
                case AstType.Class:
                    {
                        var @class = node as Class;
                        var type = GenerateType(@class, scope);
                        _types.Add(@class.Name, type);
                        return Expression.Empty();
                    }
                case AstType.PublicMethod: return AnalyzePublicMethod(node as PublicMethod, scope);
            }
            throw new NotSupportedException();
        }

        Expression AnalyzeOpenExpr(OpenExpr node, AnalysisScope scope)
        {
            return Expression.Call(
                Expression.Constant(scope.Runtime),
                typeof(OmniaRuntime).GetMethod("OpenModules"),
                scope.RuntimeExpr,
                scope.ModuleExpr,
                Expression.Constant(node.Modules));
        }

        Expression AnalyzeIdentifier(IdAst node, AnalysisScope scope)
        {
            var id = FindIdentifier(node.Name, scope);

            return id.IsSome
                ? id.Value
                : Expression.Dynamic(
                       scope.Runtime.GetGetMemberBinder(node.Name),
                       typeof(object),
                       scope.ModuleExpr);
        }

        Maybe<Expression> FindIdentifier(string name, AnalysisScope scope)
        {
            var curscope = scope;
            ParameterExpression res;
            while (curscope != null)
            {
                if (curscope.Names.TryGetValue(name, out res)) return Maybe.Some(res as Expression);

                curscope = curscope.Parent;
            }

            if (scope == null)
            {
                throw new InvalidOperationException("AnalysisScope chain with no module at end.");
            }
            return Maybe.None<Expression>();
        }

        Expression AnalyzeIdentifier(IdAst node, AnalysisScope scope, Expression parentExpr)
        {
            return Expression.Dynamic(
                       scope.Runtime.GetGetMemberBinder(node.Name),
                       typeof(object),
                       parentExpr);
        }

        Expression AnalyzeFunctionDef(FunctionDef node, AnalysisScope scope)
        {
            return Expression.Dynamic(
                scope.Runtime.GetSetMemberBinder(node.Name),
                typeof(object),
                scope.ModuleExpr,
                AnalyzeLambdaDef(node.Lamda, scope, "defun " + node.Name));
        }

        Expression AnalyzeLambdaDef(LamdaDef lamda, AnalysisScope scope, string description)
        {
            var funScope = new AnalysisScope(scope, description);

            var paramsInOrder = new List<ParameterExpression>();
            foreach (var p in lamda.Parameters)
            {
                var pe = Expression.Parameter(typeof(object), p.Name);
                paramsInOrder.Add(pe);
                funScope.Names[p.Name] = pe;
            }

            var bodyExpr = AnalyzeNode(lamda.Body, funScope);
            // Set up the Type arg array for the delegate type.  Must include
            // the return type as the last Type, which is object for Omnia defs.
            var funcTypeArgs = Enumerable.Repeat(typeof(object), lamda.Parameters.Length + 1);

            return Expression.Lambda(
                       Expression.GetFuncType(funcTypeArgs.ToArray()),
                       Expression.Block(bodyExpr),
                       paramsInOrder);
        }

        Expression AnalyzeObjExpr(ObjExpr objExpr, AnalysisScope scope)
        {
            var currExpr = AnalyzeNode(objExpr.Objects.First(), scope);

            foreach (var node in objExpr.Objects.Skip(1))
            {
                switch (node.AstType)
                {
                    case AstType.Id: currExpr = AnalyzeIdentifier(node as IdAst, scope, currExpr); break;
                    case AstType.FunctionCall: currExpr = AnalyzeFunctionCall(node as FunctionCall, scope, currExpr); break;
                    default: throw new NotImplementedException();
                }
            }
            return currExpr;
        }

        Expression AnalyzeFunctionCall(FunctionCall node, AnalysisScope scope)
        {
            var func = AnalyzeNode(node.Function, scope);
            var args = new List<Expression> { func };
            args.AddRange(node.Arguments.Select(arg => AnalyzeNode(arg, scope)));
            return Expression.Dynamic(scope.Runtime.GetInvokeBinder(new CallInfo(node.Arguments.Length)), typeof(object), args);
        }

        Expression AnalyzeFunctionCall(FunctionCall node, AnalysisScope scope, Expression parentExpr)
        {
            var args = new List<Expression> { parentExpr };
            args.AddRange(node.Arguments.Select(arg => AnalyzeNode(arg, scope)));
            return Expression.Dynamic(
                scope.Runtime.GetInvokeMemberBinder(
                new InvokeMemberBinderKey(node.Function.Name, new CallInfo(node.Arguments.Length))), typeof(object), args);
        }

        Expression AnalyzePipeCall(PipeCall node, AnalysisScope scope)
        {
            var func = AnalyzeNode(node.Function, scope);
            var arg = AnalyzeNode(node.Argument, scope);
            var args = new List<Expression> { func, arg };
            return Expression.Dynamic(scope.Runtime.GetInvokeBinder(new CallInfo(1)), typeof(object), args);
        }

        Expression AnalyzeBinExpr(BinExpr node, AnalysisScope scope)
        {
            return Expression.Dynamic(
                scope.Runtime.GetBinaryOperationBinder(node.Operation),
                typeof(object),
                AnalyzeNode(node.Left, scope),
                AnalyzeNode(node.Right, scope));
        }

        Expression AnalyzeAssignment(Assignment node, AnalysisScope scope)
        {
            var variable = Expression.Variable(typeof(object), node.Left.Name);
            //scope.Names[node.Left.Name] = variable;
            var right = AnalyzeNode(node.Right, scope);
            var assignExpr = Expression.Assign(variable, right);
            return Expression.Block(new[] { variable }, assignExpr,
                                    Expression.Dynamic(scope.Runtime.GetSetMemberBinder(node.Left.Name), typeof(object), scope.ModuleExpr, variable),
                                    variable);
        }

        Expression AnalyzeLiteral(Literal node, AnalysisScope scope)
        {
            return Expression.Constant(node.Value, typeof(object));
        }

        Expression AnalyzeStmtList(StmtList node, AnalysisScope scope)
        {
            var stmList = node.List.Select(stm => AnalyzeNode(stm, scope)).ToArray();
            return Expression.Block(stmList);
        }

        Type GenerateType(Class node, AnalysisScope scope)
        {
            return TypeGenerator.CreateType(node.Name);
        }

        Expression AnalyzePublicMethod(PublicMethod node, AnalysisScope scope)
        {
            throw new NotImplementedException();
        }
    }
}
