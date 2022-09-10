using DotDoc.Core.Read;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Models;

public class AssemblyDocItem : DocItem
{
    public AssemblyDocItem(IAssemblySymbol symbol, Compilation compilation) : base(symbol, compilation)
    {
        DisplayName = symbol.Name;
        var docSymbol = DocumentationCommentId.GetSymbolsForDeclarationId($"T:{symbol.Name}.{Constants.AssemblyDocumentClassName}", compilation).FirstOrDefault();
        if (docSymbol is { DeclaredAccessibility: Microsoft.CodeAnalysis.Accessibility.Internal })
        {
            XmlDocInfo = XmlDocParser.ParseString(docSymbol.GetDocumentationCommentXml());
        }
    }

    public List<NamespaceDocItem> Namespaces { get; } = new();

    public override IEnumerable<IDocItem> Items => Namespaces;

    public override string ToDeclareCSharpCode() => string.Empty;
}