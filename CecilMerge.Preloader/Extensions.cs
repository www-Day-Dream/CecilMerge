using System;
using System.Collections.Generic;

namespace CecilMerge
{
    internal static class Extensions
    {
        internal static void ForEach<T>(this IEnumerable<T> enumerable, Action<T> action)
        {
            foreach (var x1 in enumerable)
                action(x1);
        }
    }
}