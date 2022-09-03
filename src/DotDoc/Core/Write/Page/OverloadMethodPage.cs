using System.Text;

namespace DotDoc.Core.Write.Page;

public class OverloadMethodPage: IPage
{
    private readonly OverloadMethodDocItem _item;
    private readonly TextTransform _transform;
    private readonly DocItemContainer _itemContainer;

    public OverloadMethodPage(OverloadMethodDocItem item, TextTransform transform, DocItemContainer itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        _itemContainer = itemContainer ?? throw new ArgumentNullException(nameof(itemContainer));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
        AppendTitle(sb, $"{_item.DisplayName} Method");
        AppendNamespaceAssemblyInformation(sb);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
            
        AppendTitle(sb, "Overloads", 2);
            
        sb.AppendLine("| Name | Summary |");
        sb.AppendLine("|------|---------|");

        foreach (var childItem in _item.Methods)
        {
            var nameCellValue = 
                $"{_transform.EscapeMdText(childItem.DisplayName)}";

            sb.AppendLine($@"| {nameCellValue} | {_transform.ToMdText(childItem, childItem, t => t.XmlDocInfo?.Summary, true)} |");
        }

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
    
    private void AppendTitle(StringBuilder sb, string title, int depth = 1) =>
        sb.AppendLine($"{string.Concat(Enumerable.Repeat("#", depth))} {_transform.EscapeMdText(title)}").AppendLine();

    private void AppendNamespaceAssemblyInformation(StringBuilder sb)
    {
        var assemDocItem = _itemContainer.Get(_item.AssemblyId);
        var nsDocItem = _itemContainer.Get(_item.NamespaceId);
        
        sb.AppendLine($"namespace: [{_transform.EscapeMdText(nsDocItem?.DisplayName)}](../../{nsDocItem.ToFileName()}.md)<br />");
        sb.AppendLine($"assembly: [{_transform.EscapeMdText(assemDocItem.DisplayName)}](../../../{assemDocItem.ToFileName()}.md)").AppendLine();
    }
    
    private void AppendDeclareCode(StringBuilder sb, MethodDocItem item)
    {
        sb.AppendLine("```csharp");
        sb.AppendLine(item.ToDeclareCSharpCode());
        sb.AppendLine("```");
        sb.AppendLine();
    }
    
    private void AppendTypeParameterList(StringBuilder sb, IEnumerable<TypeParameterDocItem> typeParameters, int depth = 2)
    {
        if(typeParameters.Any())
        {
            AppendTitle(sb,"Type Parameters", depth);
            sb.AppendLine("| Name | Summary |");
            sb.AppendLine("|------|---------|");
            foreach(var param in typeParameters)
            {
                sb.AppendLine($@"| {_transform.EscapeMdText(param.DisplayName)} | {_transform.ToMdText(_item, param, t => t.XmlDocText, true)} |");
            }
            sb.AppendLine();
        }
    }
    
    private void AppendParameterList(StringBuilder sb, IEnumerable<ParameterDocItem> parameters, int depth = 2)
    {
        if(parameters.Any())
        {
            AppendTitle(sb, "Parameters", depth);
            sb.AppendLine("| Type | Name | Summary |");
            sb.AppendLine("|------|------|---------|");
            foreach(var param in parameters)
            {
                sb.AppendLine($@"| {_transform.ToMdLink(_item,  param.TypeInfo.TypeId, param.TypeInfo.DisplayName)} | {_transform.EscapeMdText(param.DisplayName)} | {_transform.ToMdText(_item, param, t => t.XmlDocText, true)} |");
            }
            sb.AppendLine();
        }
    }
    
    private void AppendReturnValue(StringBuilder sb, MethodDocItem item, int depth = 2)
    {
        if (item.ReturnValue?.TypeInfo is null) return;
            
        AppendTitle(sb, "Return Value", depth);
        sb.AppendLine(_transform.ToMdLink(item,  item.ReturnValue.TypeInfo.GetLinkTypeInfo().TypeId, item.ReturnValue.DisplayName)).AppendLine();
        sb.AppendLine(_transform.ToMdText(item, item, t => t.XmlDocInfo?.Returns)).AppendLine();
    }
}