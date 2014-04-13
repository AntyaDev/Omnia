using Irony.Parsing;

namespace Omnia.Compiller
{
    [Language("Omnia grammar", "0.0", "Omnia programming language")]
    public class Grammar : Irony.Parsing.Grammar
    {
        public Grammar() : base(caseSensitive: true)
        {
            // ReSharper disable InconsistentNaming

            // 1. Terminals
            var number = TerminalFactory.CreateCSharpNumber("Number");
            var identifier = TerminalFactory.CreateCSharpIdentifier("Identifier");
            var comment = new CommentTerminal("Comment", "#", "\n", "\r");
            //comment must to be added to NonGrammarTerminals list; it is not used directly in grammar rules,
            // so we add it to this list to let Scanner know that it is also a valid terminal. 
            NonGrammarTerminals.Add(comment);
            var comma = ToTerm(",");
            var dot = ToTerm(".");
            var colon = ToTerm(":");
            var funcGlyph = ToTerm("->");
            var fun = ToTerm("fun");
            var open = ToTerm("open");

            // 2. Non-terminals

            var OpenExpr = new NonTerminal("OpenExpr");
            var OpenArg = new NonTerminal("OpenArg");
            var OpenArgList = new NonTerminal("OpenArgList");

            var Expr = new NonTerminal("Expr");
            var Term = new NonTerminal("Term");
            var DottedExpr = new NonTerminal("DottedExpr");
            var BinExpr = new NonTerminal("BinExpr");
            var UnExpr = new NonTerminal("UnExpr");
            var UnOp = new NonTerminal("UnOp", "operator");
            var BinOp = new NonTerminal("BinOp", "operator");
            var AssignmentStmt = new NonTerminal("AssignmentStmt");
            var Stmt = new NonTerminal("Stmt");
            var ExtStmt = new NonTerminal("ExtStmt");
            //Just as a test for NotSupportedNode
            var ReturnStmt = new NonTerminal("return");
            var Block = new NonTerminal("Block");
            var StmtList = new NonTerminal("StmtList");
            var Program = new NonTerminal("Program");

            var ParamList = new NonTerminal("ParamList");
            var ArgList = new NonTerminal("ArgList");
            var Arg = new NonTerminal("Arg");
            var FunctionDef = new NonTerminal("FunctionDef");
            var LamdaDef = new NonTerminal("LamdaDef");
            var LamdaArg = new NonTerminal("LamdaArg");
            var FunctionCall = new NonTerminal("FunctionCall");
            
            // 3. BNF rules
            OpenExpr.Rule = open + "(" + OpenArgList + ")" + Eos;
            OpenArg.Rule = MakeListRule(OpenArg, dot, identifier);
            OpenArgList.Rule = MakeListRule(OpenArgList, comma, OpenArg);

            Expr.Rule = UnExpr | BinExpr | DottedExpr;
            Term.Rule = number | identifier | FunctionCall;
            DottedExpr.Rule = MakeListRule(DottedExpr, dot, Term);
            UnExpr.Rule = UnOp + Term;
            UnOp.Rule = ToTerm("+") | "-";
            BinExpr.Rule = Expr + BinOp + Expr;
            BinOp.Rule = ToTerm("+") | "-" | "*" | "/" | "**";
            
            AssignmentStmt.Rule = identifier + "=" + Expr;
            Stmt.Rule = AssignmentStmt | Expr | ReturnStmt | Empty;
            ReturnStmt.Rule = "return" + Expr;
            ExtStmt.Rule = FunctionDef | Stmt + Eos;
            StmtList.Rule = MakePlusRule(StmtList, ExtStmt);
            Block.Rule = Indent + StmtList + Dedent;
            Program.Rule = OpenExpr | OpenExpr + StmtList;

            ParamList.Rule = MakeStarRule(ParamList, comma, identifier);
            
            Arg.Rule = Expr | LamdaArg;
            LamdaArg.Rule = fun + LamdaDef;
            ArgList.Rule = MakeStarRule(ArgList, comma, Arg);
            
            LamdaDef.Rule = "(" + ParamList + ")" + funcGlyph + Eos + Block
                            | "(" + ParamList + ")" + funcGlyph + Expr;

            FunctionDef.Rule = identifier + "=" + LamdaDef;

            FunctionCall.Rule = identifier + "(" + ArgList + ")";

            Root = Program;       // Set grammar root

            // 4. Token filters - created in a separate method CreateTokenFilters
            //    we need to add continuation symbol to NonGrammarTerminals because it is not used anywhere in grammar
            NonGrammarTerminals.Add(ToTerm(@"\"));

            // 5. Operators precedence
            RegisterOperators(1, "+", "-");
            RegisterOperators(2, "*", "/");
            RegisterOperators(3, Associativity.Right, "**");

            // 6. Miscellaneous: punctuation, braces, transient nodes
            MarkPunctuation("(", ")", ":");
            RegisterBracePair("(", ")");
            MarkTransient(Term, Expr, Stmt, ExtStmt, UnOp, BinOp, ExtStmt, Block);

            // 7. Error recovery rule
            ExtStmt.ErrorRule = SyntaxError + Eos;
            FunctionDef.ErrorRule = SyntaxError + Dedent;

            // 8. Syntax error reporting
            AddToNoReportGroup("(");
            AddToNoReportGroup(Eos);
            AddOperatorReportGroup("operator");

            // 10. Language flags
            LanguageFlags = LanguageFlags.NewLineBeforeEOF;
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
