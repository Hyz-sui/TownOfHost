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
            var length = candidates.Length;
            if (length <= 0)
            {
                return default;
            }
            if (length <= 1)
            {
                return candidates[0];
            }
            return candidates[IRandom.Instance.Next(length)];
        }
    }
}
