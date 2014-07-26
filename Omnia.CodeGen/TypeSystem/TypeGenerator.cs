using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Omnia.CodeGen.TypeSystem
{
    static class TypeGenerator
    {
        static readonly Lazy<ModuleBuilder> Module = new Lazy<ModuleBuilder>(() =>
        {
            var name = new AssemblyName("RuntimeTypes");
            var assembly = AppDomain.CurrentDomain.DefineDynamicAssembly(name, AssemblyBuilderAccess.Run);
            return assembly.DefineDynamicModule("RuntimeTypes");
        });

        public static Type CreateType(string name)
        {
            var type = Module.Value.DefineType(name, TypeAttributes.Public, typeof(OmniaObject));
            return type.CreateType();
        }
    }
}
