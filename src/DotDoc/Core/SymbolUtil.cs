using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core
{
    internal static class SymbolUtil
    {
        public static string GetSymbolId(ISymbol symbol)
        {
            return symbol switch
            {
                IAssemblySymbol assemSymbol => $"{IdPrefix.Assembly}:{assemSymbol.Name}",
                IParameterSymbol paramSymbol =>
                    $"{IdPrefix.Parameter}:{paramSymbol.ContainingSymbol.ToDisplayString()}+{paramSymbol.Name}",
                ITypeParameterSymbol typeParamSymbol =>
                    $"{IdPrefix.TypeParameter}:{typeParamSymbol.ContainingSymbol.ToDisplayString()}+{typeParamSymbol.Name}",
                _ => symbol.GetDocumentationCommentId()!
            };
        }

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
        
        
        public static List<ParameterDocItem> RetrieveParameters(IEnumerable<IParameterSymbol> symbols, XmlDocInfo? docInfo, Compilation compilation)
        {
            return symbols.Select(ps =>
            {
                return new ParameterDocItem(ps, docInfo, compilation);
            }).ToList();
        } 
    
        public static List<TypeParameterDocItem> RetrieveTypeParameters(IEnumerable<ITypeParameterSymbol> symbols, XmlDocInfo? docInfo, Compilation compilation)
        {
            var typeParamItems = new List<TypeParameterDocItem>();
            foreach (var typeParam in symbols.OrEmpty())
            {
                typeParamItems.Add(new TypeParameterDocItem(typeParam, docInfo, compilation));
            }

            return typeParamItems;
        }
    }
}
