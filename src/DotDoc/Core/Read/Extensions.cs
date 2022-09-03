using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Read;

public static class Extensions
{
    public static TypeInfo ToTypeInfo(this ITypeSymbol source)
    {
        var result = new TypeInfo(source);
        return result;
    }

}