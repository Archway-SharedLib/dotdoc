using System.Text;
using DotDoc.Core.Models;

namespace DotDoc.Core.Write.Page;

public class NamespacePage: BasePage, IPage
{
    private readonly NamespaceDocItem _item;
    private readonly TextTransform _transform;

    public NamespacePage(NamespaceDocItem item, TextTransform transform, DocItemContainer itemContainer) : base(item, transform, itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }
    
    public string Write()
    {
        var sb = new TextBuilder();
  
        AppendTitle(sb, $"{_item.DisplayName} Namespace");
        AppendAssemblyInformation(sb, _item.AssemblyId);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
        
        AppendDeclareCode(sb);
            
        AppendItemList<ClassDocItem>(sb,"Classes", _item.Types);
        AppendItemList<StructDocItem>(sb,"Structs", _item.Types);
        AppendItemList<InterfaceDocItem>(sb,"Interfaces", _item.Types);
        AppendItemList<EnumDocItem>(sb,"Enums", _item.Types);
        AppendItemList<DelegateDocItem>(sb,"Delegates", _item.Types);

        return sb.ToString();
    }
    
}