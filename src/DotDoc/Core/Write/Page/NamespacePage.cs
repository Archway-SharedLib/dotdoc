using System.Text;

namespace DotDoc.Core.Write.Page;

public class NamespacePage: IPage
{
    private readonly NamespaceDocItem _item;
    private readonly TextTransform _transform;
    private readonly DocItemContainer _itemContainer;

    public NamespacePage(NamespaceDocItem item, TextTransform transform, DocItemContainer itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        _itemContainer = itemContainer ?? throw new ArgumentNullException(nameof(itemContainer));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
  
        AppendTitle(sb, $"{_item.DisplayName} Namespace");
        AppendAssemblyInformation(sb);

        AppendDeclareCode(sb);
            
        AppendItemList<ClassDocItem>(sb,"Classes", _item.Types);
        AppendItemList<StructDocItem>(sb,"Structs", _item.Types);
        AppendItemList<InterfaceDocItem>(sb,"Interfaces", _item.Types);
        AppendItemList<EnumDocItem>(sb,"Enums", _item.Types);
        AppendItemList<DelegateDocItem>(sb,"Delegates", _item.Types);

        return sb.ToString();
    }
    
    private void AppendTitle(StringBuilder sb, string title, int depth = 1) =>
        sb.AppendLine($"{string.Concat(Enumerable.Repeat("#", depth))} {_transform.EscapeMdText(title)}").AppendLine();
    
    private void AppendItemList<T>(StringBuilder sb, string title, IEnumerable<DocItem> docItems, int depth = 2) where T : DocItem
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
    
    private void AppendAssemblyInformation(StringBuilder sb)
    {
        var assemDocItem = _itemContainer.Get(_item.AssemblyId);
        sb.AppendLine($"assembly: [{_transform.EscapeMdText(assemDocItem.DisplayName)}](../{assemDocItem.ToFileName()}.md)").AppendLine();
    }
    
    private void AppendDeclareCode(StringBuilder sb)
    {
        sb.AppendLine("```csharp");
        sb.AppendLine(_item.ToDeclareCSharpCode());
        sb.AppendLine("```");
        sb.AppendLine();
    }
}