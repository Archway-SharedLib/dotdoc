using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public abstract class DocItem: IDocItem
{
    private readonly ISymbol _symbol;
    private readonly Compilation _compilation;

    protected DocItem(ISymbol symbol, Compilation compilation)
    {
        _symbol = symbol;
        _compilation = compilation;
        Id = SymbolUtil.GetSymbolId(symbol);
        Name = symbol.Name;
        DisplayName = symbol.ToDisplayString();
        MetadataName = symbol.MetadataName;
        XmlDocInfo = XmlDocParser.Parse(symbol, compilation);
        Accessiblity = SymbolUtil.MapAccessibility(symbol.DeclaredAccessibility);
    }

    public string? Id { get; protected set; }

    public string? Name { get; protected set; }

    public string? DisplayName { get; protected set; }
        
    public string? MetadataName { get; protected set; }

    public XmlDocInfo? XmlDocInfo { get; protected set; }

    protected ISymbol Symbol => _symbol;
        
    protected Compilation Compilation => _compilation;

    public virtual IEnumerable<IDocItem> Items { get; } = Enumerable.Empty<IDocItem>();

    public Accessibility Accessiblity { get; protected set; } = Accessibility.Unknown;

    public abstract string ToDeclareCSharpCode();

}