using System;
using System.Linq;
using System.Reflection;
namespace Flush.Extensions
{
    public static class TypeExtensions
    {
        /// <summary>
        /// Find a type within the same assembly as <paramref name="type"/>.
        /// </summary>
        /// <param name="type">The type to search relative to.</param>
        /// <param name="name">The type to find.</param>
        /// <returns>A type representing <paramref name="name"/></returns>
        public static Type FindOtherInAssembly(this Type type, string name)
        {
            return Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.Name == name)
                .Single();
        }
    }
}
