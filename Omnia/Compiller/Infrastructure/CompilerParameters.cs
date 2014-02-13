using System.Collections.Generic;
using System.Reflection;

namespace Omnia.Compiller.Infrastructure
{
    class CompilerParameters
    {
        public List<Assembly> Assemblies { get; private set; }
        public CompilerOutputType OutputType { get; private set; }

        public CompilerParameters(CompilerOutputType outputType)
        {
            Assemblies = new List<Assembly>();
            OutputType = outputType;

            string dllPath = typeof(object).Assembly.Location;
            var assembly = Assembly.LoadFile(dllPath);
            Assemblies.Add(assembly);
        }
    }
}
