using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class ClassDocItem : TypeDocItem
{
    public ClassDocItem(INamedTypeSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        IsSealed = symbol.IsSealed;
        IsAbstract = symbol.IsAbstract;
        IsStatic = symbol.IsStatic;
        TypeParameters.AddRange( SymbolUtil.RetrieveTypeParameters(symbol.TypeParameters, XmlDocInfo, compilation));
    }

    public List<TypeParameterDocItem> TypeParameters { get; } = new ();
        
    public bool IsSealed { get; }
        
    public bool IsAbstract { get; }
        
    public bool IsStatic { get; }

    public override string ToDeclareCSharpCode()
    {
        var result =
            $"{Accessiblity.ToCSharpText()} {(IsStatic ? "static " : string.Empty)}{(IsSealed ? "sealed " : string.Empty)}{(IsAbstract ? "abstract " : string.Empty)}class {DisplayName}";
        var typeParam = string.Join(" ", TypeParameters.Select(tp => tp.ToDeclareCSharpCode())).Trim();
        
        return $"{result}{(!string.IsNullOrEmpty(typeParam) ? " " + typeParam : "")};";
    }
        
}