using System;
using System.Collections.Generic;

namespace SanDoku.Extensions
{
    public static class ListExtensions
    {
        public static void RemoveAt<T>(this IList<T> list, Func<T, bool> predicate)
        {
            for (var i = 0; i < list.Count; i++)
            {
                if (!predicate(list[i])) continue;
                list.RemoveAt(i);
                break;
            }
        }
    }
}