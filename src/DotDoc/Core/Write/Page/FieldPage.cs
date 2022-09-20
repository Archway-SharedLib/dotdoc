using System.Text;
using DotDoc.Core.Models;

namespace DotDoc.Core.Write.Page;

public class FieldPage: BasePage, IPage
{
    private readonly FieldDocItem _item;
    private readonly TextTransform _transform;

    public FieldPage(FieldDocItem item, TextTransform transform, DocItemContainer itemContainer) : base(item, transform, itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }
    
    public string Write()
    {
        var sb = new TextBuilder();
        AppendTitle(sb, $"{_item.DisplayName} Field");
        AppendNamespaceAssemblyInformation(sb, _item.AssemblyId, _item.NamespaceId, 2);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
            
        AppendDeclareCode(sb);
            
        AppendTitle(sb, "Field Value", 2);

        var linkType = _item.TypeInfo.GetLinkTypeInfo();
        sb.AppendLine($"{_transform.ToMdLink(_item, linkType.TypeId, linkType.DisplayName)}").AppendLine();
        if (!string.IsNullOrEmpty(_item.XmlDocInfo?.Value))
        {
            sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Value)).AppendLine();
        }
            
        return sb.ToString();
    }
}