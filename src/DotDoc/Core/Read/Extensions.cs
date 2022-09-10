using Microsoft.CodeAnalysis;
using TypeInfo = DotDoc.Core.Models.TypeInfo;

namespace DotDoc.Core.Read;

public static class Extensions
{
    public static TypeInfo ToTypeInfo(this ITypeSymbol source)
    {
        var result = new TypeInfo(source);
        return result;
    }

}