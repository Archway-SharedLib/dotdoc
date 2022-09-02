using System.Text;

namespace DotDoc.Core.Write.Page;

public class AssemblyPage: IPage
{
    private readonly AssemblyDocItem _item;
    private readonly TextTransform _transform;

    public AssemblyPage(AssemblyDocItem item, TextTransform transform)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
        
        AppendTitle(sb, $"{_item.DisplayName} Assembly");
        AppendItemList<NamespaceDocItem>(sb, "Namespaces", _item.Namespaces);

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
}