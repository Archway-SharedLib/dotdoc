using System.Text;
using DotDoc.Core.Models;

namespace DotDoc.Core.Write.Page;

public class PropertyPage: BasePage, IPage
{
    private readonly PropertyDocItem _item;
    private readonly TextTransform _transform;

    public PropertyPage(PropertyDocItem item, TextTransform transform, DocItemContainer itemContainer)  : base(item, transform, itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }
    
    public string Write()
    {
        var sb = new TextBuilder();
        AppendTitle(sb, $"{_item.DisplayName} Property");
        AppendNamespaceAssemblyInformation(sb, _item.AssemblyId, _item.NamespaceId, 2);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
            
        AppendDeclareCode(sb);

        AppendTitle(sb, "Property Value", 2);
        
        var linkType = _item.TypeInfo.GetLinkTypeInfo();
        sb.AppendLine($"{_transform.ToMdLink(_item, linkType.TypeId, linkType.DisplayName)}").AppendLine();
        if (!string.IsNullOrEmpty(_item.XmlDocInfo?.Value))
        {
            sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Value)).AppendLine();
        }
        
        return sb.ToString();
    }
}