using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public abstract class MemberDocItem : DocItem, IMemberDocItem
{
    protected MemberDocItem(ISymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        NamespaceId = SymbolUtil.GetSymbolId(symbol.ContainingNamespace);
        AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly);
        TypeId = SymbolUtil.GetSymbolId(symbol.ContainingType);
        DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingType.ToDisplayString().Length + 1);
    }
        
    public string? AssemblyId { get; protected set; }

    public string? NamespaceId { get; protected set; }

    public string? TypeId { get; protected set; }
}