using System.Text;
using DotDoc.Core.Models;

namespace DotDoc.Core.Write.Page;

public class ConstructorPage: BasePage, IPage
{
    private readonly OverloadConstructorDocItem _item;
    private readonly TextTransform _transform;
    private readonly DocItemContainer _itemContainer;

    public ConstructorPage(OverloadConstructorDocItem item, TextTransform transform, DocItemContainer itemContainer) : base(item, transform, itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        _itemContainer = itemContainer ?? throw new ArgumentNullException(nameof(itemContainer));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
        AppendTitle(sb,$"{_itemContainer.Get(_item.TypeId).DisplayName} Constructor");
            
        AppendNamespaceAssemblyInformation(sb, _item.AssemblyId, _item.NamespaceId, 2);
            
        if (_item.Constructors.Count() == 1)
        {
            var ctor = _item.Constructors.First();
            sb.AppendLine(_transform.ToMdText(ctor, ctor, t => t.XmlDocInfo?.Summary)).AppendLine();

            AppendDeclareCode(sb, ctor);

            AppendParameterList(sb, ctor.Parameters.OrEmpty());
            return sb.ToString();    
        }

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
            
        AppendTitle(sb, "Overloads", 2);

        sb.AppendLine("| Name | Summary |");
        sb.AppendLine("|------|---------|");

        foreach (var childItem in _item.Constructors)
        {
            var nameCellValue = 
                $"{_transform.EscapeMdText(childItem.DisplayName)}";

            sb.AppendLine($@"| {nameCellValue} | {_transform.ToMdText(childItem, childItem, t => t.XmlDocInfo?.Summary, true).Replace("\n", "<br />").Replace("\r", "")} |");
        }

        sb.AppendLine();
            
        foreach (var childItem in _item.Constructors)
        {
            AppendTitle(sb, childItem.DisplayName, 2);
                
            sb.AppendLine(_transform.ToMdText(childItem, childItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
            AppendDeclareCode(sb, childItem);
            AppendParameterList(sb, childItem.Parameters.OrEmpty(), 3);
        }

        return sb.ToString();
    }

    private void AppendDeclareCode(StringBuilder sb, ConstructorDocItem item)
    {
        sb.AppendLine("```csharp");
        sb.AppendLine(item.ToDeclareCSharpCode());
        sb.AppendLine("```");
        sb.AppendLine();
    }
}