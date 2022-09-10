using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class EnumDocItem : TypeDocItem
{
    public EnumDocItem(INamedTypeSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
    }
    public override string ToDeclareCSharpCode() =>
        $"{Accessiblity.ToCSharpText()} enum {DisplayName};";
}