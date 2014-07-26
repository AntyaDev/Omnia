using System;
using System.Dynamic;
using System.Linq.Expressions;
using Microsoft.Scripting;
using Omnia.CodeGen;
using Omnia.CodeGen.Runtime;
using Omnia.Parser;

namespace Omnia.Hosting
{
    class OmniaEngine
    {
        readonly OmniaRuntime _runtime;
        readonly AnalysisScope _scope;

        public OmniaEngine(OmniaRuntime runtime)
        {
            _runtime = runtime;
            _scope = new AnalysisScope(
                null,
                "omnia_snippet",
                Expression.Parameter(typeof(OmniaRuntime), "OmniaRuntime"),
                Expression.Parameter(typeof(ExpandoObject), "OmniaSnippetModule"),
                runtime);
        }

        public object ExecuteSourceUnit(SourceUnit sourceUnit)
        {
            var parser = new OmniaParser();
            var ast = parser.Parse(sourceUnit.GetCode());

            var codeGen = new DLRCodeGen();
            var dlrAst = Expression.Block(codeGen.GenerateDLRAst(ast, _scope));

            var dlrExpr = Expression.Lambda<Func<OmniaRuntime, ExpandoObject, object>>(
                dlrAst,
                _scope.RuntimeExpr,
                _scope.ModuleExpr).Compile();

            return dlrExpr(_runtime, new ExpandoObject());
        }
    }
}
