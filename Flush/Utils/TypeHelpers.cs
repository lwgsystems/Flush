using System.Reflection;
using System.Linq;
using System;

namespace Flush.Utils
{
    public static class TypeHelpers
    {
        public static Type Search(string typeName)
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Name == typeName)
                .Single();
        }
    }
}
