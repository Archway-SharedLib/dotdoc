using DotDoc.Core.Read;

namespace DotDoc.Core.Models;

public class OverloadConstructorDocItem : IMemberDocItem
{
    public OverloadConstructorDocItem(IList<ConstructorDocItem> docItems)
    {
        var first = docItems.First();
        Id = first.Id;
        DisplayName = first.MetadataName;
        Constructors = docItems;
        AssemblyId = first.AssemblyId;
        NamespaceId = first.NamespaceId;
        TypeId = first.TypeId;
        XmlDocInfo = docItems.Where(i => i.XmlDocInfo?.Overloads is not null).FirstOrDefault()?.XmlDocInfo.Overloads ?? first.XmlDocInfo;;
    }

    public string? Id { get; }

    public string? DisplayName { get; }
        
    public IEnumerable<IDocItem>? Items { get; } = new List<IDocItem>();
        
    public IEnumerable<ConstructorDocItem> Constructors { get; }
        
    public XmlDocInfo? XmlDocInfo { get; }
        
    public string ToDeclareCSharpCode() => String.Empty;

    public string? AssemblyId { get; }
        
    public string? NamespaceId { get; }
        
    public string? TypeId { get; }
        
        
}