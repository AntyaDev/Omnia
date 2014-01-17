using Irony.Parsing;
using Omnia.Compiller;
using Omnia.Runtime.Binding;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Parser = Omnia.Compiller.Parser;

namespace Omnia.Runtime
{
    class Runtime
    {
        readonly Parser _parser;
        readonly ETGenerator _etGenerator;
        readonly IEnumerable<Assembly> _assemblies;
        readonly Dictionary<string, CallSite<Func<CallSite, object, object>>> _getSites = new Dictionary<string, CallSite<Func<CallSite, object, object>>>();
        readonly Dictionary<string, CallSite<Action<CallSite, object, object>>> _setSites = new Dictionary<string, CallSite<Action<CallSite, object, object>>>();
        readonly Dictionary<string, OmniaSetMemberBinder> _setMemberBinders = new Dictionary<string, OmniaSetMemberBinder>();
        readonly Dictionary<string, OmniaGetMemberBinder> _getMemberBinders = new Dictionary<string, OmniaGetMemberBinder>();
        readonly Dictionary<InvokeMemberBinderKey, OmniaInvokeMemberBinder> _invokeMemberBinders = new Dictionary<InvokeMemberBinderKey, OmniaInvokeMemberBinder>();

        public ExpandoObject Globals { get; private set; }

        public Runtime(Parser parser, ETGenerator etGenerator, IEnumerable<Assembly> assemblies)
        {
            _parser = parser;
            _etGenerator = etGenerator;
            _assemblies = assemblies;
            Globals = new ExpandoObject();
            AddAssemblyNamesAndTypes();
        }

        public object ExecuteExpression(string expression)
        {
            var ast = _parser.Parse(expression);

            if (ast.Status != ParseTreeStatus.Parsed) throw new InvalidOperationException();

            var scope = new AnalysisScope(
                null,
                "omnia_snippet",
                Expression.Parameter(typeof(Runtime), "OmniaRuntime"),
                Expression.Parameter(typeof(ExpandoObject), "OmniaSnippetModule"),
                this);

            var dlrAst = Expression.Block(_etGenerator.GenerateDLRAst(ast, scope));

            var dlrExpr = Expression.Lambda<Func<Runtime, ExpandoObject, object>>(
                dlrAst,
                scope.RuntimeExpr,
                scope.ModuleExpr).Compile();

            return dlrExpr(this, new ExpandoObject());
        }

        public object OpenModules(Runtime runtime, ExpandoObject module, string[] modulesNames)
        {
            foreach (var moduleName in modulesNames)
            {
                if (HasSite(runtime.Globals, moduleName))
                {
                    var value = GetSite(runtime.Globals, moduleName);
                    SetSite(module, moduleName, value);
                }
            }
            return null;
        }

        public OmniaInvokeMemberBinder GetInvokeMemberBinder(InvokeMemberBinderKey key)
        {
            return _invokeMemberBinders.ContainsKey(key)
                ? _invokeMemberBinders[key]
                : _invokeMemberBinders[key] = new OmniaInvokeMemberBinder(key.Name, key.Info);
        }

        public OmniaSetMemberBinder GetSetMemberBinder(string name)
        {
            return _setMemberBinders.ContainsKey(name)
                ? _setMemberBinders[name]
                : _setMemberBinders[name] = new OmniaSetMemberBinder(name);
        }

        public OmniaGetMemberBinder GetGetMemberBinder(string name)
        {
            return _getMemberBinders.ContainsKey(name)
                ? _getMemberBinders[name]
                : _getMemberBinders[name] = new OmniaGetMemberBinder(name);
        }

        object GetSite(IDynamicMetaObjectProvider metaObject, string name)
        {
            CallSite<Func<CallSite, object, object>> site;
            if (!_getSites.TryGetValue(name, out site))
            {
                site = CallSite<Func<CallSite, object, object>>.Create(new HelperGetMemberBinder(name));
                _getSites[name] = site;
            }
            return site.Target(site, metaObject);
        }

        void SetSite(IDynamicMetaObjectProvider metaObject, string name, object value)
        {
            CallSite<Action<CallSite, object, object>> site;
            if (!_setSites.TryGetValue(name, out site))
            {
                site = CallSite<Action<CallSite, object, object>>.Create(new OmniaSetMemberBinder(name));
                _setSites[name] = site;
            }
            site.Target(site, metaObject, value);
        }

        bool HasSite(IDynamicMetaObjectProvider metaObject, string name)
        {
            return GetSite(metaObject, name) != RuntimeHelper.Sentinel;
        }

        // AddAssemblyNamesAndTypes() builds a tree of ExpandoObjects representing
        // .NET namespaces, with TypeModel objects at the leaves.  Though Omnia is
        // case-insensitive, we store the names as they appear in .NET reflection
        // in case our globals object or a namespace object gets passed as an IDO
        // to another language or library, where they may be looking for names
        // case-sensitively using EO's default lookup.
        void AddAssemblyNamesAndTypes()
        {
            foreach (var assm in _assemblies)
            {
                foreach (var typ in assm.GetExportedTypes())
                {
                    string[] names = typ.FullName.Split('.');
                    var table = Globals;
                    for (int i = 0; i < names.Length - 1; i++)
                    {
                        string name = names[i];
                        if (HasSite(table, name))
                        {
                            // Must be Expando since only we have put objs in the tables so far.
                            table = (ExpandoObject)(GetSite(table, name));
                        }
                        else
                        {
                            var tmp = new ExpandoObject();
                            SetSite(table, name, tmp);
                            table = tmp;
                        }
                    }
                    SetSite(table, names[names.Length - 1], new TypeModel(typ));
                }
            }
        }
    }
}
