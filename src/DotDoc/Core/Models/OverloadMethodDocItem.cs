using DotDoc.Core.Read;

namespace DotDoc.Core.Models;

public class OverloadMethodDocItem : IMemberDocItem
{
    public OverloadMethodDocItem(IList<MethodDocItem> docItems)
    {
        Methods = docItems;
        var first = docItems.First();
        Id = first.Id;
        DisplayName = first.MetadataName;
        AssemblyId = first.AssemblyId;
        NamespaceId = first.NamespaceId;
        XmlDocInfo = docItems.Where(i => i.XmlDocInfo?.Overloads is not null).FirstOrDefault()?.XmlDocInfo.Overloads;
        TypeId = first.TypeId;
    }

    public string? Id { get; }
    
    public string? DisplayName { get; }

    public string? AssemblyId { get; set; }

    public string? NamespaceId { get; set; }
        
    public string? TypeId { get; }

    public IEnumerable<IDocItem>? Items { get; } = new List<IDocItem>();
        
    public XmlDocInfo? XmlDocInfo { get; }
        
    public IEnumerable<MethodDocItem> Methods { get; }
        
    public string ToDeclareCSharpCode() => String.Empty;
}