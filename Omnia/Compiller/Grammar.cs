using Irony.Parsing;

namespace Omnia.Compiller
{
    [Language("Omnia grammar", "0.0", "Omnia programming language")]
    public class Grammar : Irony.Parsing.Grammar
    {
        public Grammar() : base(caseSensitive: true)
        {
            var identifier = TerminalFactory.CreateCSharpIdentifier("Identifier");
            var number = TerminalFactory.CreateCSharpNumber("Number");
            var @string = TerminalFactory.CreateCSharpString("String");
            var @char = TerminalFactory.CreateCSharpChar("Char");
            var singleLineComment = new CommentTerminal("SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            var delimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            NonGrammarTerminals.Add(singleLineComment);
            NonGrammarTerminals.Add(delimitedComment);

            var colon = ToTerm(":", "Colon");
            var dot = ToTerm(".", "Dot");
            var comma = ToTerm(",", "Comma");
            var lPar = ToTerm("(");
            var rPar = ToTerm(")");
            var open = ToTerm("open", "Open");

            var program = new NonTerminal("Program");
            var expr = new NonTerminal("Expr");
            var term = new NonTerminal("Term");

            var openExpr = new NonTerminal("OpenExpr");
            var openArg = new NonTerminal("OpenArg");
            var openList = new NonTerminal("OpenList");

            var memberAccess = new NonTerminal("MemberAccess");
            var functionCall = new NonTerminal("FunctionCall");
            var argList = new NonTerminal("ArgList");

            program.Rule = expr | openExpr + expr;
            expr.Rule = term;
            term.Rule = number | identifier | @string | @char | memberAccess | functionCall;

            openExpr.Rule = open + PreferShiftHere() + lPar + openList + rPar + NewLine;
            openArg.Rule = MakeStarRule(openArg, dot, identifier);
            openList.Rule = MakeStarRule(openList, comma, openArg);

            memberAccess.Rule = expr + PreferShiftHere() + dot + identifier;
            functionCall.Rule = expr + PreferShiftHere() + lPar + argList + rPar | expr + PreferShiftHere() + argList;
            argList.Rule = MakeStarRule(argList, comma, expr);

            Root = program;
        }
    }
}
