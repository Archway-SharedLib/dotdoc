using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class TypeInfo
{
    public TypeInfo(ITypeSymbol symbol)
    {
        TypeId = SymbolUtil.GetSymbolId(symbol);
        DisplayName = symbol.ToDisplayString();
        Name = symbol.Name;

        if (symbol is IArrayTypeSymbol arrayType)
        {
            LinkType = arrayType.ElementType.ToTypeInfo();
        } 
        else if (symbol is INamedTypeSymbol { IsGenericType: true } namedType)
            //namedType が Type{`N} の場合は型が束縛されていないとContainingType が Type{`N} になる
            // if (namedType.IsGenericType && namedType.TypeArguments[0].ContainingType is null)
        {
            if (namedType != namedType.OriginalDefinition)
            {
                LinkType = namedType.OriginalDefinition.ToTypeInfo();
                LinkType.DisplayName = namedType.ToDisplayString();
            }
        }

        if (symbol != symbol.BaseType)
        {
            BaseType = symbol.BaseType?.ToTypeInfo();    
        }
    }
    public string TypeId { get; }

    public string Name { get;  }
        
    public string DisplayName { get; private set; }

    public TypeInfo? LinkType { get; } = null;

    public TypeInfo? BaseType { get; } = null;
}