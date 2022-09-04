using System.Text;

namespace DotDoc.Core.Write.Page;

public class EnumPage: IPage
{
    private readonly EnumDocItem _item;
    private readonly TextTransform _transform;
    private readonly DocItemContainer _itemContainer;

    public EnumPage(EnumDocItem item, TextTransform transform, DocItemContainer itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        _itemContainer = itemContainer ?? throw new ArgumentNullException(nameof(itemContainer));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
  
        AppendTitle(sb, $"{_item.DisplayName} Enum");
        AppendNamespaceAssemblyInformation(sb);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
        
        AppendDeclareCode(sb);
        AppendInheritAndImplements(sb);

        AppendFieldItemList(sb, _item.Members);
        
        return sb.ToString();
    }
    
    private void AppendTitle(StringBuilder sb, string title, int depth = 1) =>
        sb.AppendLine($"{string.Concat(Enumerable.Repeat("#", depth))} {_transform.EscapeMdText(title)}").AppendLine();
    
    private void AppendFieldItemList(StringBuilder sb, IEnumerable<IDocItem> docItems, int depth = 2)
    {
        var items = docItems.OrEmpty().OfType<FieldDocItem>();
        if (!items.Any()) return;

        AppendTitle(sb,"Fields", depth);

        sb.AppendLine("| Name | Value | Summary |");
        sb.AppendLine("|------|-------|---------|");
        
        foreach (var item in items)
        {
            var nameCellValue = _transform.EscapeMdText(item.DisplayName);

            sb.AppendLine($@"| {nameCellValue} | {GetConstantValueDisplayText(item) } | {_transform.ToMdText(_item, item, t => t.XmlDocInfo?.Summary, true).Replace("\n", "<br />").Replace("\r", "")} |");
        }

        sb.AppendLine();
    }

    private string GetConstantValueDisplayText(FieldDocItem fieldDocItem)
    {
        if (!fieldDocItem.IsConstant) return string.Empty;
        var value = fieldDocItem.ConstantValue;
        if (value is null) return "null";
        var text = value is char ? $"'{value}'" :
            value is string ? $"\"{value}\"" : value.ToString();
        return _transform.EscapeMdText(text);
    }

    private void AppendNamespaceAssemblyInformation(StringBuilder sb)
    {
        var assemDocItem = _itemContainer.Get(_item.AssemblyId);
        var nsDocItem = _itemContainer.Get(_item.NamespaceId);
        
        sb.AppendLine($"namespace: [{_transform.EscapeMdText(nsDocItem?.DisplayName)}](../{nsDocItem.ToFileName()}.md)<br />");
        sb.AppendLine($"assembly: [{_transform.EscapeMdText(assemDocItem.DisplayName)}](../../{assemDocItem.ToFileName()}.md)").AppendLine();
    }
    
    private void AppendDeclareCode(StringBuilder sb)
    {
        sb.AppendLine("```csharp");
        sb.AppendLine(_item.ToDeclareCSharpCode());
        sb.AppendLine("```");
        sb.AppendLine();
    }

    private void AppendInheritAndImplements(StringBuilder sb)
    {
        if (_item.BaseType is not null)
        {
            var bases = new List<TypeInfo>();
            var baseType = _item.BaseType;
            while (baseType is not null)
            {
                bases.Add(baseType);
                baseType = baseType.BaseType;
            }

            var links = bases.AsEnumerable().Reverse().Select(b => _transform.ToMdLink(_item, b.GetLinkTypeInfo().TypeId, b.GetLinkTypeInfo().DisplayName));
            sb.AppendLine("Inheritance: " + string.Join(" > ", links.Append(_item.DisplayName))).AppendLine();
        }
        if (_item.Interfaces.OrEmpty().Any())
        {
            sb.AppendLine("Implements: " + string.Join(", ", _item.Interfaces.Select(i => _transform.ToMdLink(_item, i.GetLinkTypeInfo().TypeId, i.GetLinkTypeInfo().DisplayName)))).AppendLine();    
        }
    }
}