using System.Text;
using DotDoc.Core.Models;

namespace DotDoc.Core.Write.Page;

public class OverloadMethodPage: BasePage, IPage
{
    private readonly OverloadMethodDocItem _item;
    private readonly TextTransform _transform;

    public OverloadMethodPage(OverloadMethodDocItem item, TextTransform transform, DocItemContainer itemContainer)  : base(item, transform, itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
        AppendTitle(sb, $"{_item.DisplayName} Method");
        AppendNamespaceAssemblyInformation(sb, _item.AssemblyId, _item.NamespaceId, 2);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
            
        AppendTitle(sb, "Overloads", 2);
            
        sb.AppendLine("| Name | Summary |");
        sb.AppendLine("|------|---------|");

        foreach (var childItem in _item.Methods)
        {
            var nameCellValue = 
                $"{_transform.EscapeMdText(childItem.DisplayName)}";

            sb.AppendLine($@"| {nameCellValue} | {_transform.ToMdText(childItem, childItem, t => t.XmlDocInfo?.Summary, true).Replace("\n", "<br />").Replace("\r", "")} |");
        }

        sb.AppendLine();

        foreach (var memberDocItem in _item.Methods)
        {
            AppendTitle(sb, memberDocItem.DisplayName, 2);
                
            sb.AppendLine(_transform.ToMdText(memberDocItem, memberDocItem, t => t.XmlDocInfo?.Summary)).AppendLine();
            
            AppendDeclareCode(sb, memberDocItem);
            
            AppendTypeParameterList(sb, memberDocItem.TypeParameters.OrEmpty(), 3);
            AppendParameterList(sb, memberDocItem.Parameters.OrEmpty(), 3);
            AppendReturnValue(sb, memberDocItem, 3);
        }

        return sb.ToString();
    }

    private void AppendDeclareCode(StringBuilder sb, MethodDocItem item)
    {
        sb.AppendLine("```csharp");
        sb.AppendLine(item.ToDeclareCSharpCode());
        sb.AppendLine("```");
        sb.AppendLine();
    }
    
    private void AppendReturnValue(StringBuilder sb, MethodDocItem item, int depth = 2)
    {
        if (item.ReturnValue?.TypeInfo is null) return;
            
        AppendTitle(sb, "Return Value", depth);
        var linkType = item.ReturnValue.TypeInfo.GetLinkTypeInfo();
        sb.AppendLine(_transform.ToMdLink(item,  linkType.TypeId, linkType.DisplayName)).AppendLine();
        sb.AppendLine(_transform.ToMdText(item, item, t => t.XmlDocInfo?.Returns)).AppendLine();
    }
}