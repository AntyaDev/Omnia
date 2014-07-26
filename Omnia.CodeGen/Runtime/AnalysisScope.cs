using System.Collections.Generic;
using System.Linq.Expressions;

namespace Omnia.CodeGen.Runtime
{
    public class AnalysisScope
    {        
        public AnalysisScope Parent { get; private set; }
        public ParameterExpression RuntimeExpr { get; private set; }
        public OmniaRuntime Runtime { get; private set; }
        public ParameterExpression ModuleExpr { get; private set; }
        public string Description { get; private set; }
        public Dictionary<string, ParameterExpression> Names { get; private set; }

        public AnalysisScope(AnalysisScope parent, string description)
        {
            Parent = parent;
            Description = description;
            Names = new Dictionary<string, ParameterExpression>();
            Runtime = parent.Runtime;
            ModuleExpr = parent.ModuleExpr;
            RuntimeExpr = parent.RuntimeExpr;
        }

        public AnalysisScope(AnalysisScope parent, string description, ParameterExpression runtimeExpr,
                             ParameterExpression moduleExpr, OmniaRuntime runtime)
        {
            Parent = parent;
            Description = description;
            RuntimeExpr = runtimeExpr;
            ModuleExpr = moduleExpr;
            Runtime = runtime;
            Names = new Dictionary<string, ParameterExpression>();
        }
    }
}
