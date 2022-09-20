using System.Text;
using DotDoc.Core.Models;

namespace DotDoc.Core.Write.Page;

public class StructPage: BasePage, IPage
{
    private readonly StructDocItem _item;
    private readonly TextTransform _transform;

    public StructPage(StructDocItem item, TextTransform transform, DocItemContainer itemContainer)  : base(item, transform, itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }
    
    public string Write()
    {
        var sb = new TextBuilder();
  
        AppendTitle(sb, $"{_item.DisplayName} Struct");
        AppendNamespaceAssemblyInformation(sb, _item.AssemblyId, _item.NamespaceId);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
        
        AppendDeclareCode(sb);
        AppendInheritAndImplements(sb, _item);

        AppendItemList<ConstructorDocItem>(sb,"Constructors", _item.Members);
        AppendItemList<MethodDocItem>(sb,"Methods", _item.Members);
        AppendItemList<PropertyDocItem>(sb,"Properties", _item.Members);
        AppendFieldItemList(sb, _item.Members);
        AppendItemList<EventDocItem>(sb,"Events", _item.Members);

        AppendTypeParameterList(sb, _item.TypeParameters);
        
        return sb.ToString();
    }
}