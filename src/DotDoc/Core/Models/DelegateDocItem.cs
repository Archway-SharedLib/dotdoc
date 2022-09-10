using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class DelegateDocItem : TypeDocItem
{
    public DelegateDocItem(INamedTypeSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        TypeParameters.AddRange( SymbolUtil.RetrieveTypeParameters(symbol.TypeParameters, XmlDocInfo, compilation));
        var delegMethod = symbol.DelegateInvokeMethod!;
        Parameters.AddRange(SymbolUtil.RetrieveParameters(delegMethod.Parameters, XmlDocInfo, compilation));
        ReturnValue = delegMethod.ReturnsVoid
            ? null
            : new ReturnItem(delegMethod);
    }

    public List<TypeParameterDocItem> TypeParameters { get; } = new();

    public List<ParameterDocItem> Parameters { get; } = new();

    public ReturnItem? ReturnValue { get; }
        
    public override string ToDeclareCSharpCode() =>
        $"{Accessiblity.ToCSharpText()} delegate {ReturnValue?.ToDeclareCSharpCode() ?? "void"} {DisplayName}({string.Join(", ", Parameters.OrEmpty().Select(p => p.ToDeclareCSharpCode()))});";

}