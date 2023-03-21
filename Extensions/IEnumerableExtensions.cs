using System.Collections.Generic;
using System.Linq;

namespace TownOfHost.Extensions
{
    public static class IEnumerableExtensions
    {
        public static T PickRandom<T>(this IEnumerable<T> candidates)
        {
            return candidates.ToArray().PickRandom();
        }
        public static T PickRandom<T>(this T[] candidates)
        {
            return candidates[IRandom.Instance.Next(candidates.Length)];
        }
    }
}
