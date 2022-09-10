using System.Text;
using DotDoc.Core.Models;

namespace DotDoc.Core.Write.Page;

public class EnumPage: BasePage, IPage
{
    private readonly EnumDocItem _item;
    private readonly TextTransform _transform;

    public EnumPage(EnumDocItem item, TextTransform transform, DocItemContainer itemContainer) : base(item, transform, itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
  
        AppendTitle(sb, $"{_item.DisplayName} Enum");
        AppendNamespaceAssemblyInformation(sb, _item.AssemblyId, _item.NamespaceId);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
        
        AppendDeclareCode(sb);
        AppendInheritAndImplements(sb, _item);

        AppendFieldItemList(sb, _item.Members, needFieldLInk:false);
        
        return sb.ToString();
    }
}