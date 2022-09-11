using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class InterfaceDocItem : TypeDocItem
{
    public InterfaceDocItem(INamedTypeSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        TypeParameters.AddRange( SymbolUtil.RetrieveTypeParameters(symbol.TypeParameters, XmlDocInfo, compilation));
    }

    public List<TypeParameterDocItem> TypeParameters { get; } = new();

    public override string ToDeclareCSharpCode()
    {
        var result = $"{Accessiblity.ToCSharpText()} interface {DisplayName}";
        var typeParam = string.Join(" ", TypeParameters.Select(tp => tp.ToDeclareCSharpCode())).Trim();
        return $"{result}{(!string.IsNullOrEmpty(typeParam) ? " " + typeParam : "")};";
    }
        
}