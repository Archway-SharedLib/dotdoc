using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace DotDoc.Core.Read
{
    internal class DefaultFilter : IFilter
    {
        private readonly List<Regex> _excludeIdRegexs;
        private readonly List<Microsoft.CodeAnalysis.Accessibility> _includeAccessibilities;
        public DefaultFilter(DotDocEngineOptions options)
        {
            _excludeIdRegexs = (options?.ExcludeIdPatterns ?? Enumerable.Empty<string>())
                .Select(Regex.Escape)
                .Select(ConvertToRegexPattern)
                .Select(p => new Regex(p)).ToList();
            _includeAccessibilities = (options?.Accessibility ?? Enumerable.Empty<Accessibility>())
                .Where(a => a is not Accessibility.Unknown)
                .Select(a =>
                {
                    return a switch
                    {
                        Accessibility.Private => Microsoft.CodeAnalysis.Accessibility.Private,
                        Accessibility.Protected => Microsoft.CodeAnalysis.Accessibility.Protected,
                        Accessibility.Internal => Microsoft.CodeAnalysis.Accessibility.Internal,
                        Accessibility.Public => Microsoft.CodeAnalysis.Accessibility.Public,
                        Accessibility.PrivateProtected => Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal,
                        Accessibility.ProtectedInternal => Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal
                    };
                }).ToList();
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

            if (_includeAccessibilities.Any(a => a == symbol.DeclaredAccessibility)) return false;
            
            return true;
        }

        
        
    }
}
