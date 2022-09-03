using System.Text;

namespace DotDoc.Core.Write.Page;

public class StructPage: IPage
{
    private readonly StructDocItem _item;
    private readonly TextTransform _transform;
    private readonly DocItemContainer _itemContainer;

    public StructPage(StructDocItem item, TextTransform transform, DocItemContainer itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        _itemContainer = itemContainer ?? throw new ArgumentNullException(nameof(itemContainer));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
  
        AppendTitle(sb, $"{_item.DisplayName} Struct");
        AppendNamespaceAssemblyInformation(sb);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
        
        AppendDeclareCode(sb);
        AppendInheritAndImplements(sb);

        AppendItemList<ConstructorDocItem>(sb,"Constructors", _item.Members);
        AppendItemList<MethodDocItem>(sb,"Methods", _item.Members);
        AppendItemList<PropertyDocItem>(sb,"Properties", _item.Members);
        AppendFieldItemList(sb, _item.Members);
        AppendItemList<EventDocItem>(sb,"Events", _item.Members);

        AppendTypeParameterList(sb, _item.TypeParameters);
        
        return sb.ToString();
    }
    
    private void AppendTitle(StringBuilder sb, string title, int depth = 1) =>
        sb.AppendLine($"{string.Concat(Enumerable.Repeat("#", depth))} {_transform.EscapeMdText(title)}").AppendLine();
    
    private void AppendItemList<T>(StringBuilder sb, string title, IEnumerable<IDocItem> docItems, int depth = 2) where T : IDocItem
    {
        var items = docItems.OrEmpty().OfType<T>();
        if (!items.Any()) return;

        AppendTitle(sb, title, depth);

        sb.AppendLine("| Name | Summary |");
        sb.AppendLine("|------|---------|");

        foreach (var item in items)
        {
            var nameCellValue = 
                $"[{_transform.EscapeMdText(item.DisplayName)}](./{_item.ToFileName()}/{item.ToFileName()}.md)";

            sb.AppendLine($@"| {nameCellValue} | {_transform.ToMdText(_item, item, t => t.XmlDocInfo?.Summary, true)} |");
        }

        sb.AppendLine();
    }
    
    private void AppendFieldItemList(StringBuilder sb, IEnumerable<IDocItem> docItems, int depth = 2)
    {
        var items = docItems.OrEmpty().OfType<FieldDocItem>();
        if (!items.Any()) return;

        AppendTitle(sb,"Fields", depth);

        sb.AppendLine("| Name | Value | Summary |");
        sb.AppendLine("|------|-------|---------|");
        
        foreach (var item in items)
        {
            var nameCellValue = 
                $"[{_transform.EscapeMdText(item.DisplayName)}](./{_item.ToFileName()}/{item.ToFileName()})";

            sb.AppendLine($@"| {nameCellValue} | {GetConstantValueDisplayText(item) } | {_transform.ToMdText(_item, item, t => t.XmlDocInfo?.Summary, true)} |");
        }

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
        if (!string.IsNullOrEmpty(_item.BaseTypeId))
        {
            var bases = new List<string>();
            var baseId = _item.BaseTypeId;
            while (!string.IsNullOrEmpty(baseId))
            {
                bases.Add(baseId);
                var baseDocItem = _itemContainer.Get(baseId);
                baseId = null;
                if (baseDocItem is TypeDocItem baseTypeDoc)
                {
                    baseId = baseTypeDoc.BaseTypeId;
                }
            }

            var links = bases.AsEnumerable().Reverse().Select(id => _transform.ToMdLink(_item, id));
            sb.AppendLine("Inheritance: " + string.Join(" > ", links.Append(_item.DisplayName))).AppendLine();
        }
        if (_item.InterfaceIds.OrEmpty().Any())
        {
            sb.AppendLine("Implements: " + string.Join(", ", _item.InterfaceIds.Select(id => _transform.ToMdLink(_item, id)))).AppendLine();    
        }
    }
}