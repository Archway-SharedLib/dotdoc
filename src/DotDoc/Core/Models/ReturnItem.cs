using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class ReturnItem
{
    public ReturnItem(IMethodSymbol symbol)
    {
        TypeInfo = symbol.ReturnType.ToTypeInfo();
        RefKind = symbol.ReturnsByRefReadonly ? ValueRefKind.RefReadoly : ValueRefKind.None;
    }
        
    public TypeInfo TypeInfo { get; }

    public ValueRefKind RefKind { get; }

    public string ToDeclareCSharpCode()
    {
        var refKindString = RefKind == ValueRefKind.RefReadoly ? "ref readonly "
            : string.Empty;
        return $"{refKindString}{TypeInfo.DisplayName}";
    }
}