using DotDoc.Core.Read;

namespace DotDoc.Core.Models;

public interface IDocItem
{
    string? Id { get; }
        
    string? DisplayName { get; }
        
    IEnumerable<IDocItem>? Items { get; }
        
    XmlDocInfo? XmlDocInfo { get; }
        
    string ToDeclareCSharpCode();
}