using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class StructDocItem : TypeDocItem
{
    public StructDocItem(INamedTypeSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        TypeParameters.AddRange( SymbolUtil.RetrieveTypeParameters(symbol.TypeParameters, XmlDocInfo, compilation));
    }

    public List<TypeParameterDocItem> TypeParameters { get; } = new();
        
    public override string ToDeclareCSharpCode() =>
        $"{Accessiblity.ToCSharpText()} struct {DisplayName};";
}