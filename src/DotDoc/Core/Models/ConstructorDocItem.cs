using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class ConstructorDocItem : MemberDocItem
{
    public ConstructorDocItem(IMethodSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        CSharpConstructorName = symbol.ContainingType.Name;
        Parameters.AddRange(SymbolUtil.RetrieveParameters(symbol.Parameters, XmlDocInfo, compilation));
        
        DisplayName = symbol.ToDisplayString(SymbolDisplayFormats.MethodDisplayNameFormat);
    }

    public List<ParameterDocItem> Parameters { get; } = new();

    public override IEnumerable<IDocItem> Items => Parameters;
        
    public string CSharpConstructorName { get; }
        
    public override string ToDeclareCSharpCode()
    {
        var paramsText = Parameters.OrEmpty().Select(p => p.ToDeclareCSharpCode())
            .ConcatWith(" ,");

        return $"{Accessiblity.ToCSharpText()} {CSharpConstructorName}({paramsText});";
    }
}