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
            var @string = TerminalFactory.CreatePythonString("String");
            var comment = new CommentTerminal("Comment", "#", "\n", "\r");
            //comment must to be added to NonGrammarTerminals list; it is not used directly in grammar rules,
            // so we add it to this list to let Scanner know that it is also a valid terminal. 
            NonGrammarTerminals.Add(comment);
            var comma = ToTerm(",");
            var dot = ToTerm(".");
            var colon = ToTerm(":");
            var funcGlyph = ToTerm("->");
            var piping = ToTerm("|>");
            var fun = ToTerm("fun");
            var CLASS = ToTerm("class");

            // 2. Non-terminals
            var OpenExpr = new NonTerminal("OpenExpr");
            var OpenArg = new NonTerminal("OpenArg");
            var OpenArgExtStmt = new NonTerminal("OpenArgExtStmt");
            var OpenArgStmtList = new NonTerminal("OpenArgStmtList");
            var OpenArgBlock = new NonTerminal("OpenArgBlock");

            var Expr = new NonTerminal("Expr");
            var Term = new NonTerminal("Term");
            var Obj = new NonTerminal("Obj");
            var ObjExpr = new NonTerminal("ObjExpr");
            var BinExpr = new NonTerminal("BinExpr");
            var UnExpr = new NonTerminal("UnExpr");
            var UnOp = new NonTerminal("UnOp", "operator");
            var BinOp = new NonTerminal("BinOp", "operator");
            var Assignment = new NonTerminal("Assignment");
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
            var PipeFunctionCall = new NonTerminal("PipeFunctionCall");
            var PipeCall = new NonTerminal("PipeCall");
            var Class = new NonTerminal("Class");
            var PublicMethod = new NonTerminal("PublicMethod");
            var OptionParamStart = ToTerm("(") | Empty;
            var OptionParamEnd = ToTerm(")") | Empty;
            var InitClass = new NonTerminal("InitClass");
            
            // 3. BNF rules
            OpenExpr.Rule = ToTerm("open") + OpenArgExtStmt | ToTerm("open") + OpenArg + Eos + OpenArgBlock;
            OpenArg.Rule = MakePlusRule(OpenArg, dot, identifier);
            OpenArgExtStmt.Rule = OpenArg + Eos;
            OpenArgStmtList.Rule = MakePlusRule(OpenArgStmtList, OpenArgExtStmt);
            OpenArgBlock.Rule = Indent + OpenArgStmtList + Dedent;

            Expr.Rule = UnExpr | BinExpr | ObjExpr;
            Term.Rule = @string | number | identifier;
            Obj.Rule = Term | FunctionCall | PipeFunctionCall | PipeCall | InitClass;
            ObjExpr.Rule = MakeListRule(ObjExpr, dot, Obj);
            UnExpr.Rule = UnOp + Term;
            UnOp.Rule = ToTerm("+") | "-";
            BinExpr.Rule = Expr + BinOp + Expr;
            BinOp.Rule = ToTerm("+") | "-" | "*" | "/" | "**";
            
            Assignment.Rule = identifier + "=" + Expr;
            Stmt.Rule = Assignment | Expr | ReturnStmt | Empty;
            ReturnStmt.Rule = "return" + Expr;
            ExtStmt.Rule = Class | PublicMethod | FunctionDef | Stmt + Eos;
            StmtList.Rule = MakePlusRule(StmtList, ExtStmt);
            Block.Rule = Indent + StmtList + Dedent;
            Program.Rule = StmtList | OpenExpr + StmtList;

            ParamList.Rule = MakeStarRule(ParamList, comma, identifier);
            
            Arg.Rule = Expr | LamdaArg;
            LamdaArg.Rule = fun + LamdaDef;
            ArgList.Rule = MakeStarRule(ArgList, comma, Arg);
            
            LamdaDef.Rule = "(" + ParamList + ")" + funcGlyph + Eos + Block
                            | funcGlyph + Eos + Block
                            | "(" + ParamList + ")" + funcGlyph + Expr
                            | funcGlyph + Expr;

            FunctionDef.Rule = identifier + "=" + LamdaDef;

            FunctionCall.Rule = identifier + "(" + ArgList + ")";

            PipeFunctionCall.Rule = Term + piping + FunctionCall;

            PipeCall.Rule = Term + piping + identifier;

            Class.Rule = CLASS + identifier + Eos + Block
                         | CLASS + identifier + "(" + ParamList + ")" + Eos + Block;

            PublicMethod.Rule = identifier + ":" + LamdaDef;

            InitClass.Rule = "new" + identifier + "(" + ArgList + ")";

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
            MarkTransient(OpenArgExtStmt, Term, Obj, Expr, Stmt, ExtStmt, UnOp, BinOp, ExtStmt, Block);

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
