using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotDoc.Core.Read
{
    internal static class SymbolUtil
    {
        public static string GetSymbolId(IAssemblySymbol symbol) => $"{IdPrefix.Assembly}:{symbol.Name}";

        public static string GetSymbolId(ISymbol symbol) => symbol.GetDocumentationCommentId()!;

        public static string GetSymbolId(IParameterSymbol symbol) => $"{IdPrefix.Parameter}:{symbol.ContainingSymbol.ToDisplayString()}+{symbol.Name}";
        
        public static string GetSymbolId(ITypeParameterSymbol symbol) => $"{IdPrefix.TypeParameter}:{symbol.ContainingSymbol.ToDisplayString()}+{symbol.Name}";

        public static Accessibility MapAccessibility(Microsoft.CodeAnalysis.Accessibility symbolAccessibility)
        {
            return symbolAccessibility switch
            {
                Microsoft.CodeAnalysis.Accessibility.Private => Accessibility.Private,
                Microsoft.CodeAnalysis.Accessibility.Protected => Accessibility.Protected,
                Microsoft.CodeAnalysis.Accessibility.Internal => Accessibility.Internal,
                Microsoft.CodeAnalysis.Accessibility.Public => Accessibility.Public,
                Microsoft.CodeAnalysis.Accessibility.ProtectedAndInternal => Accessibility.PrivateProtected,
                Microsoft.CodeAnalysis.Accessibility.ProtectedOrInternal => Accessibility.ProtectedInternal,
                _ => Accessibility.Unknown
            };
        }
    }
}
