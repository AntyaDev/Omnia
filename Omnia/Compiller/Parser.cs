
namespace Omnia.Compiller
{
    class Parser : Irony.Parsing.Parser
    {
        public Parser() : base(new Grammar())
        { }

        public Irony.Parsing.ParseTree ParseExpr(string expr)
        {
            return Parse(expr);
        }
    }
}
