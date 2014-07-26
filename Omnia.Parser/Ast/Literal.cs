
namespace Omnia.Parser.Ast
{
    public class Literal : Ast
    {
        public override AstType AstType { get { return AstType.Literal; } }
        public object Value { get; private set; }

        public Literal(object value)
        {
            Value = value;
        }
    }
}
