using System.Collections.Generic;
using Microsoft.Scripting.Hosting;

namespace Omnia.Hosting
{
    public static class Omnia
    {
        public static ScriptRuntime CreateRuntime()
        {
            return new ScriptRuntime(CreateRuntimeSetup(null));
        }

        static ScriptRuntimeSetup CreateRuntimeSetup(IDictionary<string, object> options)
        {
            var setup = new ScriptRuntimeSetup();
            setup.LanguageSetups.Add(CreateLanguageSetup(options));

            if (options != null)
            {
                object value;
                if (options.TryGetValue("Debug", out value) &&
                    value is bool && (bool)value)
                {
                    setup.DebugMode = true;
                }

                if (options.TryGetValue("PrivateBinding", out value) &&
                    value is bool && (bool)value)
                {
                    setup.PrivateBinding = true;
                }
            }
            return setup;
        }

        static LanguageSetup CreateLanguageSetup(IDictionary<string, object> options)
        {
            var setup = new LanguageSetup(
                typeof(OmniaContext).AssemblyQualifiedName,
                OmniaContext.OmniaDisplayName,
                OmniaContext.OmniaNames.Split(';'),
                OmniaContext.OmniaFileExtensions.Split(';')
            );

            if (options != null)
            {
                foreach (var entry in options)
                {
                    setup.Options.Add(entry.Key, entry.Value);
                }
            }
            return setup;
        }
    }
}
