using System;
using System.Collections.Generic;
using Microsoft.Scripting;
using Microsoft.Scripting.Runtime;
using Microsoft.Scripting.Utils;
using Omnia.CodeGen;

namespace Omnia.Hosting
{
    public class OmniaContext : LanguageContext
    {
        public const string OmniaDisplayName = "Omnia 0.0.0.0";
        public const string OmniaNames = "Omnia;om";
        public const string OmniaFileExtensions = ".om";

        readonly OmniaEngine _engine;
        readonly IDictionary<string, object> _options;

        public OmniaContext(ScriptDomainManager scriptManager, IDictionary<string, object> options) : base(scriptManager)
        {
            _engine = new OmniaEngine(new OmniaRuntime(scriptManager.GetLoadedAssemblyList()));
            _options = options;
        }

        public override ScriptCode CompileSourceCode(SourceUnit sourceUnit, CompilerOptions options, ErrorSink errorSink)
        {
            try
            {
                switch (sourceUnit.Kind)
                {
                    case SourceCodeKind.SingleStatement:
                    case SourceCodeKind.Expression:
                    case SourceCodeKind.AutoDetect:
                    case SourceCodeKind.File:
                    case SourceCodeKind.InteractiveCode: return new OmniaScriptCode(_engine, sourceUnit);
                    default: throw Assert.Unreachable;
                }
            }
            catch (Exception e)
            {
                errorSink.Add(sourceUnit, e.Message, SourceSpan.None, 0, Severity.FatalError);
                return null;
            }
        }
    }
}
