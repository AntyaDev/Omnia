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
            var @bool = ToTerm("true") | "false";
            var alphaNumeric = new NonTerminal("AlphaNumeric");
            var literal = new NonTerminal("Literal");
            var singleLineComment = new CommentTerminal("SingleLineComment", "//", "\r", "\n", "\u2085", "\u2028", "\u2029");
            var delimitedComment = new CommentTerminal("DelimitedComment", "/*", "*/");
            NonGrammarTerminals.Add(singleLineComment);
            NonGrammarTerminals.Add(delimitedComment);

            var lPar = ToTerm("(");
            var rPar = ToTerm(")");
            var dot = ToTerm(".", "Dot");
            var comma = ToTerm(",", "Comma");
            var funcGlyph = ToTerm("->");
            var binaryOperator = new NonTerminal("BinaryOperator");
            var binaryExpr = new NonTerminal("BinaryExpr");

            var program = new NonTerminal("Program");
            var expr = new NonTerminal("Expr");
            var line = new NonTerminal("Line");
            var body = new NonTerminal("Body");
            var block = new NonTerminal("Block");

            var value = new NonTerminal("Val");
            var assign = new NonTerminal("Assign");
            var simpleAssignable = new NonTerminal("SimpleAssignable");
            var assignable = new NonTerminal("Assignable");
            var memberAcess = new NonTerminal("MemberAcess");
            var argList = new NonTerminal("ArgList");
            var arguments = new NonTerminal("Arguments");
            var functionCall = new NonTerminal("FunctionCall");
            var param = new NonTerminal("Param");
            var paramList = new NonTerminal("ParamList");
            var functionDef = new NonTerminal("FunctionDef");

            var open = ToTerm("open", "Open");
            var openExpr = new NonTerminal("OpenExpr");
            var openArg = new NonTerminal("OpenArg");
            var openList = new NonTerminal("OpenList");
            
            alphaNumeric.Rule = number | @string | @char;

            literal.Rule = alphaNumeric | @bool;

            expr.Rule = value | assign | functionCall | functionDef | binaryExpr;

            line.Rule = expr + Eos;

            body.Rule = MakePlusRule(body, line);

            block.Rule = Indent + body + Dedent;

            binaryOperator.Rule = ToTerm("+") | "-" | "*" | "/";

            binaryExpr.Rule = value + binaryOperator + value;

            openExpr.Rule = open + PreferShiftHere() + "(" + openList + ")" + Eos;
            
            openArg.Rule = MakePlusRule(openArg, dot, identifier);

            openList.Rule = MakeStarRule(openList, comma, openArg);

            program.Rule = openExpr + body | body;

            simpleAssignable.Rule = identifier | value + memberAcess | functionCall + memberAcess;

            assignable.Rule = simpleAssignable;

            assign.Rule = (assignable + "=" + expr) | (assignable + "=" + Indent + expr + Dedent);

            value.Rule = assignable | literal;

            memberAcess.Rule = ToTerm(".") + identifier;

            argList.Rule = MakePlusRule(argList, comma, expr);

            arguments.Rule = lPar + rPar
                             | lPar + argList + rPar;

            functionCall.Rule = value + arguments
                              | functionCall + arguments;

            param.Rule = identifier | (identifier + "=" + expr);

            paramList.Rule = MakePlusRule(paramList, comma, param);

            functionDef.Rule = (lPar + paramList + rPar + funcGlyph + Eos + block + expr)
                               | (lPar + paramList + rPar + funcGlyph + expr);

            MarkPunctuation("=", "(", ")", ".", ",");
            MarkTransient(line, expr, literal, alphaNumeric, value, assignable, arguments);

            Root = program;
        }

        //Make parser indentation-aware
        public override void CreateTokenFilters(LanguageData language, TokenFilterList filters)
        {
            const OutlineOptions options = OutlineOptions.ProduceIndents 
                                           | OutlineOptions.CheckBraces 
                                           | OutlineOptions.CheckOperator;

            var outlineFilter = new CodeOutlineFilter(language.GrammarData, options, null);
            filters.Add(outlineFilter);
        }
    }
}
