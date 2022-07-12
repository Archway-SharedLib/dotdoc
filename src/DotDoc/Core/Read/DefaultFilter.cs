using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotDoc.Core.Read
{
    internal class DefaultFilter : IFilter
    {
        private readonly List<Regex> _excludeIdRegexs;

        public DefaultFilter(IEnumerable<string>? excludeIdPatters)
        {
            _excludeIdRegexs = (excludeIdPatters ?? Enumerable.Empty<string>())
                .Select(Regex.Escape)
                .Select(ConvertToRegexPattern)
                .Select(p => new Regex(p)).ToList();
        }

        private string ConvertToRegexPattern(string pattern) => $"^{pattern.Replace("\\*", "(.*?)")}$";

        public bool Exclude(ISymbol symbol, string id)
        {
            if (_excludeIdRegexs.Any(r => r.IsMatch(id))) return true;

            if (symbol is IAssemblySymbol)
            {
                return false;
            }

            // コンパイラが生成したものは Exclude
            if (symbol.IsImplicitlyDeclared) return true;

            if (symbol.DeclaredAccessibility is Accessibility.Public or Accessibility.Protected or Accessibility.ProtectedOrInternal)
            {
                return false;
            }
            return true;
        }
    }
}
