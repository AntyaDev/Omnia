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
            
            var expr = new NonTerminal("Expr");
            var term = new NonTerminal("Term");
            var memberAccess = new NonTerminal("MemberAccess");
            var functionCall = new NonTerminal("FunctionCall");
            var argList = new NonTerminal("ArgList");

            argList.Rule = MakeStarRule(argList, comma, expr);
            memberAccess.Rule = expr + PreferShiftHere() + dot + identifier;
            functionCall.Rule = expr + PreferShiftHere() + lPar + argList + rPar | expr + PreferShiftHere() + argList;
            term.Rule = number | identifier | @string | @char | memberAccess | functionCall;
            expr.Rule = term;

            Root = expr;
        }
    }
}
