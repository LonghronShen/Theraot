#if LESSTHAN_NETSTANDARD13 && !PROFILE328
using System.Linq;

namespace System.Reflection
{
    public static class AssemblyExtensions
    {
        public static Type[] GetTypes(this Assembly assembly)
        {
            return assembly.DefinedTypes.Select(typeInfo => typeInfo.AsType()).ToArray();
        }
    }
}

#endif