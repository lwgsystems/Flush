using System;
using System.Collections.Generic;
using System.Linq;

namespace Flush.Extensions
{
    public static class IEnumerableExtensions
    {
        public static TResult MinOrDefault<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> func)
        {
            return enumerable.Any() ? enumerable.Min(func) : default(TResult);
        }

        public static TResult MaxOrDefault<T, TResult>(this IEnumerable<T> enumerable, Func<T, TResult> func)
        {
            return enumerable.Any() ? enumerable.Max(func) : default(TResult);
        }
    }
}
