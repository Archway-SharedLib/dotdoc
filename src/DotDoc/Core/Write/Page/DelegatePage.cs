using System.Text;
using DotDoc.Core.Models;

namespace DotDoc.Core.Write.Page;

public class DelegatePage: BasePage, IPage
{
    private readonly DelegateDocItem _item;
    private readonly TextTransform _transform;

    public DelegatePage(DelegateDocItem item, TextTransform transform, DocItemContainer itemContainer) : base(item, transform, itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }
    
    public string Write()
    {
        var sb = new TextBuilder();
  
        AppendTitle(sb, $"{_item.DisplayName} Delegate");
        AppendNamespaceAssemblyInformation(sb, _item.AssemblyId, _item.NamespaceId);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
        
        AppendDeclareCode(sb);
        AppendInheritAndImplements(sb, _item);

        AppendTypeParameterList(sb, _item.TypeParameters.OrEmpty());
        AppendParameterList(sb, _item.Parameters.OrEmpty());
        
        AppendReturnValue(sb, _item.ReturnValue);
        
        return sb.ToString();
    }
}