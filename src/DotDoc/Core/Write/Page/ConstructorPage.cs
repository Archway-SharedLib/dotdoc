using System.Text;

namespace DotDoc.Core.Write.Page;

public class ConstructorPage: IPage
{
    private readonly OverloadConstructorDocItem _item;
    private readonly TextTransform _transform;
    private readonly DocItemContainer _itemContainer;

    public ConstructorPage(OverloadConstructorDocItem item, TextTransform transform, DocItemContainer itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        _itemContainer = itemContainer ?? throw new ArgumentNullException(nameof(itemContainer));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
        AppendTitle(sb,$"{_itemContainer.Get(_item.TypeId).DisplayName} Constructor");
            
        AppendNamespaceAssemblyInformation(sb);
            
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

            sb.AppendLine($@"| {nameCellValue} | {_transform.ToMdText(childItem, childItem, t => t.XmlDocInfo?.Summary, true)} |");
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
    
    private void AppendTitle(StringBuilder sb, string title, int depth = 1) =>
        sb.AppendLine($"{string.Concat(Enumerable.Repeat("#", depth))} {_transform.EscapeMdText(title)}").AppendLine();

    private void AppendNamespaceAssemblyInformation(StringBuilder sb)
    {
        var assemDocItem = _itemContainer.Get(_item.AssemblyId);
        var nsDocItem = _itemContainer.Get(_item.NamespaceId);
        
        sb.AppendLine($"namespace: [{_transform.EscapeMdText(nsDocItem?.DisplayName)}](../../{nsDocItem.ToFileName()}.md)<br />");
        sb.AppendLine($"assembly: [{_transform.EscapeMdText(assemDocItem.DisplayName)}](../../../{assemDocItem.ToFileName()}.md)").AppendLine();
    }
    
    private void AppendDeclareCode(StringBuilder sb, ConstructorDocItem item)
    {
        sb.AppendLine("```csharp");
        sb.AppendLine(item.ToDeclareCSharpCode());
        sb.AppendLine("```");
        sb.AppendLine();
    }
    
    private void AppendParameterList(StringBuilder sb, IEnumerable<ParameterDocItem> parameters, int depth = 2)
    {
        var paramList = parameters.ToList();
        if(paramList.Any())
        {
            AppendTitle(sb, "Parameters", depth);
            sb.AppendLine("| Type | Name | Summary |");
            sb.AppendLine("|------|------|---------|");
            foreach(var param in paramList)
            {
                var linkType = param.TypeInfo.GetLinkTypeInfo();
                sb.AppendLine($@"| {_transform.ToMdLink(_item,  linkType.TypeId, linkType.DisplayName)} | {_transform.EscapeMdText(param.DisplayName)} | {_transform.ToMdText(_item, param, t => t.XmlDocText, true)} |");
            }
            sb.AppendLine();
        }
    }
}