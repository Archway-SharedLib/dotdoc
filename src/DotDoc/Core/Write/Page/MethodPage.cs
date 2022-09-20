using System.Text;
using DotDoc.Core.Models;

namespace DotDoc.Core.Write.Page;

public class MethodPage: BasePage, IPage
{
    private readonly MethodDocItem _item;
    private readonly TextTransform _transform;

    public MethodPage(MethodDocItem item, TextTransform transform, DocItemContainer itemContainer)  : base(item, transform, itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }
    
    public string Write()
    {
        var sb = new TextBuilder();
        AppendTitle(sb, $"{_item.DisplayName} Method");
        AppendNamespaceAssemblyInformation(sb, _item.AssemblyId, _item.NamespaceId, 2);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
            
        AppendDeclareCode(sb);
            
        AppendTypeParameterList(sb, _item.TypeParameters.OrEmpty());
        AppendParameterList(sb,_item.Parameters.OrEmpty());
        AppendReturnValue(sb, _item.ReturnValue);

        return sb.ToString();
    }
}