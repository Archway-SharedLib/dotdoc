using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class ParameterDocItem: DocItem
{
    public ParameterDocItem(IParameterSymbol symbol, XmlDocInfo docInfo, Compilation compilation) : base(symbol, compilation)
    {
        DisplayName = symbol.Name;
        TypeInfo = symbol.Type.ToTypeInfo();
        XmlDocText = docInfo?.Parameters.OrEmpty().FirstOrDefault(p => p.Name == symbol.Name)?.Text;
        RefKind = symbol.RefKind == Microsoft.CodeAnalysis.RefKind.In ? ValueRefKind.In :
            symbol.RefKind == Microsoft.CodeAnalysis.RefKind.Out ? ValueRefKind.Out :
            symbol.RefKind == Microsoft.CodeAnalysis.RefKind.None ? ValueRefKind.None :
            symbol.RefKind == Microsoft.CodeAnalysis.RefKind.Ref ? ValueRefKind.Ref : ValueRefKind.RefReadoly;
        HasThis = IsExtensionParameter(symbol);
        IsParams = symbol.IsParams;
        HasDefaultValue = symbol.HasExplicitDefaultValue;
        DefaultValue = symbol.HasExplicitDefaultValue ? symbol.ExplicitDefaultValue : null;
    }

    private bool IsExtensionParameter(IParameterSymbol symbol)
        => symbol.ContainingSymbol is IMethodSymbol {IsExtensionMethod:true} methodSymbol && methodSymbol.Parameters[0] == symbol;

    public TypeInfo TypeInfo { get; }
    public string? XmlDocText { get; }
    public bool HasThis { get; }
    public bool IsParams { get; }
    public ValueRefKind RefKind { get; }
    public bool HasDefaultValue { get; }
        
    public object? DefaultValue { get; }

    public override string ToDeclareCSharpCode()
    {
        var refKindString = (RefKind == ValueRefKind.RefReadoly || RefKind == ValueRefKind.None)
            ? string.Empty
            : (RefKind.ToString().ToLower() + " ");
        var thisString = HasThis ? "this " : string.Empty;
        var paramsString = IsParams ? "params " : string.Empty;
        return $"{refKindString}{thisString}{paramsString}{TypeInfo.DisplayName} {Name}{(HasDefaultValue ? " = " + GetConstantValueDisplayText(DefaultValue) : "")}";
    }
        
    private string GetConstantValueDisplayText(object? value)
    {
        if (value is null) return "null";
        return value is char ? $"'{value}'" :
            value is string ? $"\"{value}\"" : value.ToString();
    }
}