using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Read;

public static class Extensions
{
    public static TypeInfo ToTypeInfo(this ITypeSymbol source)
    {
        var result = new TypeInfo()
        {
            TypeId = SymbolUtil.GetSymbolId(source),
            DisplayName = source.ToDisplayString(),
            Name = source.Name
        };
        if (source is IArrayTypeSymbol arrayType)
        {
            result.LinkType = arrayType.ElementType.ToTypeInfo();
        } 
        else if (source is INamedTypeSymbol namedType)
        {
            //namedType が Type{`N} の場合は型が束縛されていないとContainingType が Type{`N} になる
            if (namedType.IsGenericType && namedType.TypeArguments[0].ContainingType is null)
            {
                result.LinkType = namedType.OriginalDefinition.ToTypeInfo();
            }
                
        }

        return result;
    }

    public static TypeInfo GetLinkTypeInfo(this TypeInfo info)
    {
        while (info.LinkType is not null)
        {
            info = info.LinkType;
        }
        return info;
    }
}