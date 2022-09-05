using System.Text;

namespace DotDoc.Core.Write.Page;

public class AssemblyPage: IPage
{
    private readonly AssemblyDocItem _item;
    private readonly TextTransform _transform;
    private readonly DotDocEngineOptions _options;

    public AssemblyPage(AssemblyDocItem item, TextTransform transform, DotDocEngineOptions options)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
        
        AppendTitle(sb, $"{_item.DisplayName} Assembly");
        
        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
        
        AppendItemList<NamespaceDocItem>(sb, "Namespaces", _item.Namespaces);

        return sb.ToString();
    }
    
    private void AppendTitle(StringBuilder sb, string title, int depth = 1) =>
        sb.AppendLine($"{string.Concat(Enumerable.Repeat("#", depth))} {_transform.EscapeMdText(title)}").AppendLine();
    
    private void AppendItemList<T>(StringBuilder sb, string title, IEnumerable<IDocItem> docItems, int depth = 2) where T : IDocItem
    {
        var items = docItems.OrEmpty().OfType<T>().Where(i => !(_options.IgnoreEmptyNamespace && !i.Items.Any())).ToList();
        if (!items.Any()) return;
        
        AppendTitle(sb, title, depth);

        sb.AppendLine("| Name | Summary |");
        sb.AppendLine("|------|---------|");

        foreach (var item in items)
        {
            
            var nameCellValue = 
                $"[{_transform.EscapeMdText(item.DisplayName)}](./{_item.ToFileName()}/{item.ToFileName()}.md)";

            sb.AppendLine($@"| {nameCellValue} | {_transform.ToMdText(_item, item, t => t.XmlDocInfo?.Summary, true).Replace("\n", "<br />").Replace("\r", "")} |");
        }

        sb.AppendLine();
    }
}