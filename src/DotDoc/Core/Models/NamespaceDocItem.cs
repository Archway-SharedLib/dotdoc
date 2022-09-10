using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class NamespaceDocItem : DocItem
{
    public NamespaceDocItem(INamespaceSymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly);
            
        var docSymbol = DocumentationCommentId.GetSymbolsForDeclarationId($"T:{symbol.ToDisplayString()}.{Constants.NamespaceDocumentClassName}", compilation).FirstOrDefault();
        if (docSymbol is { DeclaredAccessibility: Microsoft.CodeAnalysis.Accessibility.Internal })
        {
            XmlDocInfo = XmlDocParser.ParseString(docSymbol.GetDocumentationCommentXml());
        }
    }

    public List<TypeDocItem> Types { get; } = new();

    public string? AssemblyId { get;}
        
    public override IEnumerable<IDocItem> Items => Types;

    public override string ToDeclareCSharpCode() => $"namespace {DisplayName};";
}