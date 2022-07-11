using Microsoft.CodeAnalysis;

namespace DotDoc.Core;

public static class SymbolExtensions
{
    public static bool IsImplicitConstructor(this ISymbol symbol) => 
        symbol is IMethodSymbol { MethodKind: MethodKind.Constructor } mSymbol 
            && mSymbol.Locations[0].GetLineSpan() == mSymbol.ContainingType.Locations[0].GetLineSpan();
}