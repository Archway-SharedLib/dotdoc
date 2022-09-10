using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class TypeParameterDocItem : DocItem
{
    public TypeParameterDocItem(ITypeParameterSymbol symbol, XmlDocInfo docInfo, Compilation compilation) : base(symbol, compilation)
    {
        XmlDocText = docInfo?.TypeParameters.OrEmpty().FirstOrDefault(p => p.Name == symbol.Name)?.Text;
    }
        
    public string? XmlDocText { get; }
        
    public override string ToDeclareCSharpCode() => string.Empty;
}