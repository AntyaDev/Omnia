using System.Dynamic;
using Irony.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Omnia.Runtime.Binding;

namespace Omnia.Compiller
{
    class ETGenerator
    {
        public IEnumerable<Expression> GenerateDLRAst(ParseTree ast, AnalysisScope scope)
        {
            yield return AnalyzeNode(ast.Root.ChildNodes.First(), scope);
            foreach (var expr in ast.Root.ChildNodes.Last().ChildNodes.Select(node => AnalyzeNode(node, scope)))
            {
                yield return expr;
            }
        }

        public Expression AnalyzeNode(ParseTreeNode node, AnalysisScope scope)
        {
            switch (node.Term.Name)
            {
                case "OpenExpr": return AnalyzeOpenExpr(node, scope);

                case "FunctionCall": return AnalyzeInvocation(node, scope);

                case "String": return ConvertStringToExpr(node);

                case "Number": return ConvertNumberToExpr(node);

                case "Assign": return AnalizeAssign(node, scope);

                case "SimpleAssignable": return AnalizeSimpleAssignable(node, scope);

                case "Identifier": return AnalizeIdentifier(node, scope);
            }
            throw new NotSupportedException();
        }

        string GetIdentifier(ParseTreeNode node)
        {
            return GetChildNodes(node).First(child => child.Term.Name == "Identifier").Token.ValueString;
        }

        Expression AnalizeAssign(ParseTreeNode node, AnalysisScope scope)
        {
            string idName = GetIdentifier(node.ChildNodes.First());
            var rightExpr = AnalyzeNode(node.ChildNodes.Last(), scope);
            var tmpVal = Expression.Parameter(typeof(object), "assignTmpForRes");

            return Expression.Block(new[] { tmpVal },
                Expression.Assign(tmpVal, Expression.Convert(rightExpr, typeof(object))),
                Expression.Dynamic(scope.Runtime.GetSetMemberBinder(idName), typeof(object),
                                   scope.ModuleExpr, tmpVal),
                                   tmpVal);
        }

        Expression AnalizeSimpleAssignable(ParseTreeNode node, AnalysisScope scope)
        {
            return AnalyzeNode(node.ChildNodes.First(), scope);
        }

        Expression AnalizeIdentifier(ParseTreeNode node, AnalysisScope scope)
        {
            return Expression.Dynamic(
                       scope.Runtime.GetGetMemberBinder(node.Token.ValueString),
                       typeof(object),
                       scope.ModuleExpr);
        }

        Expression AnalyzeInvocation(ParseTreeNode node, AnalysisScope scope)
        {
            var leftNode = node.ChildNodes.First();

            if (leftNode.ChildNodes.Any(n => n.Term.Name == "MemberAcess"))
            {
                var members = AnalyzeMemberAccess(leftNode.ChildNodes.First(), scope).ToArray();
                var args = AnalyzeArgList(node.ChildNodes.Last(), scope).ToArray();
                
                var array = new List<Expression>();
                array.AddRange(members);
                array.AddRange(args);

                return Expression.Dynamic(
                    scope.Runtime.GetInvokeMemberBinder(
                    new InvokeMemberBinderKey("WriteLine",
                        new CallInfo(args.Length))),
                        typeof(object),
                        array);
            }
            throw new NotImplementedException();
        }

        IEnumerable<Expression> AnalyzeArgList(ParseTreeNode node, AnalysisScope scope)
        {
            return node.ChildNodes.Select(chield => AnalyzeNode(chield, scope));
        }

        IEnumerable<Expression> AnalyzeMemberAccess(ParseTreeNode node, AnalysisScope scope)
        {
            yield return GetChildNodes(node)
                .Where(chield => chield.Token != null)
                .Aggregate<ParseTreeNode, Expression>(null, (getMemberExpr, childNode) => 
                    Expression.Dynamic(scope.Runtime.GetGetMemberBinder(childNode.Token.ValueString),
                                       typeof (object),
                                       getMemberExpr ?? scope.ModuleExpr));
        }

        IEnumerable<ParseTreeNode> GetChildNodes(ParseTreeNode node)
        {
            var stack = new Stack<ParseTreeNode>();
            stack.Push(node);

            while (stack.Any())
            {
                var current = stack.Pop();
                current.ChildNodes.Reverse();

                foreach (var child in current.ChildNodes)
                {
                    yield return child;
                    stack.Push(child);
                }
            }
        }

        Expression ConvertStringToExpr(ParseTreeNode node)
        {
            return Expression.Constant(node.Token.ValueString, typeof(string));
        }

        Expression ConvertNumberToExpr(ParseTreeNode node)
        {
            return Expression.Constant(Convert.ToInt32(node.Token.ValueString), typeof(int));
        }

        Expression AnalyzeOpenExpr(ParseTreeNode node, AnalysisScope scope)
        {
            var openList = node.ChildNodes.First(n => n.Term.Name == "OpenList")
                               .ChildNodes.Where(n => n.Term.Name == "OpenArg")
                               .Select(openArg => openArg.ChildNodes);

            var modules = openList.Select(openModule => 
                openModule.Aggregate(
                string.Empty,
                (current, module) => current + (current.Length > 0 
                    ? "." + module.Token.Text 
                    : module.Token.Text))).ToArray();

            return Expression.Call(
                Expression.Constant(scope.Runtime),
                typeof(Runtime.Runtime).GetMethod("OpenModules"),
                scope.RuntimeExpr,
                scope.ModuleExpr,
                Expression.Constant(modules));
        }
    }
}
