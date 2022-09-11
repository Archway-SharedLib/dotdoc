using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public abstract class TypeDocItem : DocItem
{
    public TypeDocItem(INamedTypeSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        DisplayName = symbol.ToDisplayString()
            .Substring(symbol.ContainingNamespace.ToDisplayString().Length + 1);
        NamespaceId = SymbolUtil.GetSymbolId(symbol.ContainingNamespace);
        AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly);
        BaseType = symbol.BaseType is not null ? symbol.BaseType.ToTypeInfo() : null;
        Interfaces = symbol.Interfaces.OrEmpty().Select(i => i.ToTypeInfo()).ToList();
    }
        
    public List<IMemberDocItem> Members { get; } = new();

    public string? AssemblyId { get; protected set; }
        
    public string? NamespaceId { get; protected set; }
        
    public override IEnumerable<IDocItem> Items => Members;
        
    public TypeInfo? BaseType { get; protected set; }

    public List<TypeInfo> Interfaces { get; }

}