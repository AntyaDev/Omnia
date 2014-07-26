using System;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;

namespace Omnia.Hosting
{
    class OmniaScriptCode : ScriptCode
    {
        readonly OmniaEngine _engine;

        public OmniaScriptCode(OmniaEngine engine, SourceUnit sourceUnit) : base(sourceUnit)
        {
            _engine = engine;
        }

        public override object Run(Scope scope)
        {
            return _engine.ExecuteSourceUnit(SourceUnit);
        }
    }
}
