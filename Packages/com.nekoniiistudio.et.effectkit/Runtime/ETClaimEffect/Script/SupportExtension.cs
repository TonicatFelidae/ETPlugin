using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ETClaimEffect
{
    public static class SupportExtension
    {
        public static T Find<T>(this ICollection<T> self, Func<T, bool> predicate)
        {
            using (var enumerator = self.GetEnumerator())
            {
                while (enumerator.MoveNext())
                {
                    T current = enumerator.Current;
                    if (predicate(current))
                    {
                        return current;
                    }
                }
            }

            return default(T);
        }
    }
}