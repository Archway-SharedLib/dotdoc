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

        public static string ToCSharpText(this Accessibility item)
         => item switch
            {
                Accessibility.Private => "private",
                Accessibility.Protected => "protected",
                Accessibility.Internal => "internal",
                Accessibility.Public => "public",
                Accessibility.PrivateProtected => "private protected",
                Accessibility.ProtectedInternal => "protected internal",
                _ => string.Empty
            };
        
        public static string ConcatWith<T>(this IEnumerable<T> source, string separator)
            => string.Join(separator, source);

        public static string SurroundsWith(this string source, string value)
            => value + source + value;
        
        public static string SurroundsWith(this string source, string start, string end)
            => start + source + end;
        
        public static string SurroundsWith(this string source, string value, Func<string, bool> predicate)
            => predicate(source) ? value + source + value : source;

        public static string SurroundsWith(this string source, string start, string end, Func<string, bool> predicate)
            => predicate(source) ? start + source + end : source;

    }
}
