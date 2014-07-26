using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using FunctionalWeapon.Monads;
using Irony.Parsing;
using Omnia.Parser.Ast;

namespace Omnia.Parser
{
    public class OmniaParser
    {
        readonly Irony.Parsing.Parser _parser = new Irony.Parsing.Parser(new OmniaGrammar());

        public IEnumerable<Ast.Ast> Parse(string strExpr)
        {
            var ast = _parser.Parse(strExpr);

            if (ast.HasErrors()) throw new InvalidProgramException();
            
            yield return ParseNode(ast.Root.ChildNodes.First());
            
            if (ast.Root.ChildNodes.Count == 1) yield break;

            foreach (var expr in ast.Root.ChildNodes.Last().ChildNodes.Select(ParseNode))
            {
                yield return expr;
            }
        }

        Ast.Ast ParseNode(ParseTreeNode node)
        {
            switch (node.Term.Name)
            {
                case "OpenExpr": return ParseOpenExpr(node);
                case "ObjExpr": return ParseObjExpr(node);
                case "Assignment": return ParseAssignmentStmt(node);
                case "Identifier": return ParseIdentifier(node);
                
                case "Number": 
                case "String": return ParseLiteral(node);

                case "FunctionDef": return ParseFunctionDef(node);
                case "FunctionCall": return ParseFunctionCall(node);
                case "PipeCall": return ParsePipeCall(node);
                case "LambdaDef": return ParseLambdaDef(node);
                case "BinExpr": return ParseBinExpr(node);
                case "StmtList": return ParseStmtList(node);
                case "Class": return ParseClass(node);
                case "PublicMethod": return ParsePublicMethod(node);
                case "InitClass": return ParseInitClass(node);
            }
            throw new NotSupportedException();
        }

        OpenExpr ParseOpenExpr(ParseTreeNode node)
        {
            var args = new List<ParseTreeNode> {node.ChildNodes.First(n => n.Term.Name == "OpenArg")};

            var openList = Maybe.Some(node.ChildNodes.FirstOrDefault(n => n.Term.Name == "OpenArgBlock"))
                                .Bind(block => block.ChildNodes.First());

            if (openList.IsSome) args.AddRange(openList.Value.ChildNodes);

            var modules = args.Select(openArg => openArg.ChildNodes)
                              .Select(openModule => 
                                  openModule.Aggregate(string.Empty, (current, module) =>
                                      current + (current.Length > 0 ? "." + module.Token.Text : module.Token.Text))).ToList();

            return new OpenExpr(modules);
        }

        ObjExpr ParseObjExpr(ParseTreeNode node)
        {
            if (node.Term.Name == "Identifier") return new ObjExpr(ParseIdentifier(node));
            
            if (node.Term.Name == "String") return new ObjExpr(ParseLiteral(node));

            var objecs = node.ChildNodes.Select(ParseNode).ToArray();
            return new ObjExpr(objecs);
        }

        Assignment ParseAssignmentStmt(ParseTreeNode node)
        {
            var left = ParseIdentifier(node.ChildNodes.First());
            var right = ParseNode(node.ChildNodes.Last());
            return new Assignment(left, right);
        }

        IdAst ParseIdentifier(ParseTreeNode node)
        {
            return new IdAst(node.Token.ValueString);
        }

        FunctionDef ParseFunctionDef(ParseTreeNode node)
        {
            var name = ParseIdentifier(node.ChildNodes.First());
            var lamda = ParseLambdaDef(node.ChildNodes.Last());
            return new FunctionDef(name, lamda);
        }

        FunctionCall ParseFunctionCall(ParseTreeNode node)
        {
            return new FunctionCall(ParseIdentifier(node.ChildNodes.First()), ParseArgList(node.ChildNodes.Last()));
        }

        PipeCall ParsePipeCall(ParseTreeNode node)
        {
            return new PipeCall(ParseObjExpr(node.ChildNodes.Last()), ParseArg(node.ChildNodes.First()));
        }

        LamdaDef ParseLambdaDef(ParseTreeNode node)
        {
            var @params = ParseParamList(node.ChildNodes.First());
            var body = ParseBody(node.ChildNodes.Last());
            return new LamdaDef(@params.ToArray(), body);
        }

        Arg ParseArg(ParseTreeNode node)
        {
            return new Arg(ParseObjExpr(node));
        }

        Arg[] ParseArgList(ParseTreeNode node)
        {
            return node.ChildNodes.Select(ParseArg).ToArray();
        }

        Param[] ParseParamList(ParseTreeNode node)
        {
            return node.ChildNodes.Select(param => new Param(ParseIdentifier(param))).ToArray();
        }

        Ast.Ast ParseBody(ParseTreeNode node)
        {
            return ParseNode(node);
        }

        BinExpr ParseBinExpr(ParseTreeNode node)
        {
            var operation = ParseOperation(node.ChildNodes.ElementAt(1));
            var left = ParseNode(node.ChildNodes.First());
            var right = ParseNode(node.ChildNodes.Last());
            return new BinExpr(left, right, operation);
        }

        ExpressionType ParseOperation(ParseTreeNode node)
        {
            switch (node.Token.ValueString)
            {
                case "+": return ExpressionType.Add;
                case "-": return ExpressionType.Subtract;
                case "*": return ExpressionType.Multiply;
                case "/": return ExpressionType.Divide;
                default: throw new NotSupportedException();
            }
        }

        Literal ParseLiteral(ParseTreeNode node)
        {
            return new Literal(node.Token.Value);
        }

        StmtList ParseStmtList(ParseTreeNode node)
        {
            return new StmtList(node.ChildNodes.Select(ParseNode).ToArray());
        }

        Class ParseClass(ParseTreeNode node)
        {
            var module = new IdAst("default");
            var name = ParseIdentifier(node.ChildNodes[1]);
            var paramList = ParseParamList(node.ChildNodes[2]);
            var body = ParseStmtList(node.ChildNodes[3]);
            return new Class(module, name, paramList, body);
        }

        PublicMethod ParsePublicMethod(ParseTreeNode node)
        {
            var name = ParseIdentifier(node.ChildNodes.First());
            var lamda = ParseLambdaDef(node.ChildNodes.Last());
            return new PublicMethod(name, lamda);
        }

        InitClass ParseInitClass(ParseTreeNode node)
        {
            var className = ParseIdentifier(node.ChildNodes[1]);
            var args = ParseArgList(node.ChildNodes[2]);
            return new InitClass(className, args);
        }
    }
}
