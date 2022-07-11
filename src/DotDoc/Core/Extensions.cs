using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core
{
    internal static class Extensions
    {
        public static IEnumerable<TItem> OrEmpty<TItem>(this IEnumerable<TItem>? source)
            => source ?? Enumerable.Empty<TItem>();
    }
}
