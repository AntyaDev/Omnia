using System.Collections.Generic;
using System.Linq.Expressions;

namespace Omnia.Compiller
{
    class AnalysisScope
    {
        readonly string _name;
        readonly Dictionary<string, ParameterExpression> _names;

        public AnalysisScope Parent { get; private set; }
        public ParameterExpression RuntimeExpr { get; private set; }
        public Runtime.Runtime Runtime { get; private set; }
        public ParameterExpression ModuleExpr { get; private set; }

        public AnalysisScope(AnalysisScope parent, string name, ParameterExpression runtimeExpr,
                             ParameterExpression moduleExpr, Runtime.Runtime runtime)
        {
            Parent = parent;
            _name = name;
            RuntimeExpr = runtimeExpr;
            ModuleExpr = moduleExpr;
            Runtime = runtime;
        }
    }
}
