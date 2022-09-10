using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class MethodDocItem : MemberDocItem
{
    public MethodDocItem(IMethodSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        ReturnValue = symbol.ReturnsVoid
            ? null
            : new ReturnItem(symbol);
        IsStatic = symbol.IsStatic;
        IsAbstract = symbol.IsAbstract;
        IsOverride = symbol.IsOverride;
        IsVirtual = symbol.IsVirtual;
            
        TypeParameters.AddRange( SymbolUtil.RetrieveTypeParameters(symbol.TypeParameters, XmlDocInfo, compilation));
        Parameters.AddRange(SymbolUtil.RetrieveParameters(symbol.Parameters, XmlDocInfo, compilation));
    }

    public List<ParameterDocItem> Parameters { get; } = new();

    public override IEnumerable<IDocItem> Items => Parameters;

    public List<TypeParameterDocItem> TypeParameters { get; } = new();
        
    public ReturnItem? ReturnValue { get; }
        
    public bool IsStatic { get; }
        
    public bool IsOverride { get; }
        
    public bool IsVirtual { get; }
        
    public bool IsAbstract { get; }
        
    public override string ToDeclareCSharpCode()
    {
        var modifiersText = new List<string>();
        if(IsAbstract) modifiersText.Add("abstract ");
        if(IsStatic) modifiersText.Add("static ");
        if(IsOverride) modifiersText.Add("override ");
        if(IsVirtual) modifiersText.Add("virtual ");

        var returnValueText = ReturnValue is not null ? ReturnValue.ToDeclareCSharpCode() : "void";
        var typeParamsText = TypeParameters.OrEmpty().Select(p => p.Name)
            .ConcatWith(" ,")
            .SurroundsWith("<", ">", v => !string.IsNullOrEmpty(v));
        var paramsText = Parameters.OrEmpty().Select(p => p.ToDeclareCSharpCode())
            .ConcatWith(" ,");

        return $"{Accessiblity.ToCSharpText()} {string.Join("", modifiersText)}{returnValueText} {Name}{typeParamsText}({paramsText});";
    }
}