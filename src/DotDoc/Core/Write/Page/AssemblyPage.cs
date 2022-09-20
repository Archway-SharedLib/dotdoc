using System.Text;
using DotDoc.Core.Models;

namespace DotDoc.Core.Write.Page;

public class AssemblyPage: BasePage, IPage
{
    private readonly AssemblyDocItem _docItem;
    private readonly TextTransform _transform;
    private readonly DotDocEngineOptions _options;

    public AssemblyPage(AssemblyDocItem item, TextTransform transform, DocItemContainer itemContainer , DotDocEngineOptions options) : base(item, transform, itemContainer)
    {
        _docItem = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        _options = options ?? throw new ArgumentNullException(nameof(options));
    }
    
    public string Write()
    {
        var sb = new TextBuilder();
        
        AppendTitle(sb, $"{_docItem.DisplayName} Assembly");
        
        sb.AppendLine(_transform.ToMdText(_docItem, _docItem, t => t.XmlDocInfo?.Summary)).AppendLine();
        
        AppendItemList<NamespaceDocItem>(sb, "Namespaces", _docItem.Namespaces);

        return sb.ToString();
    }

    protected override void AppendItemList<T>(TextBuilder sb, string title, IEnumerable<IDocItem> docItems, int depth = 2)
    {
        var items = docItems.OrEmpty().OfType<T>().Where(i => !(_options.IgnoreEmptyNamespace && !i.Items.Any())).ToList();
        if (!items.Any()) return;
        
        AppendTitle(sb, title, depth);

        sb.AppendLine("| Name | Summary |");
        sb.AppendLine("|------|---------|");

        foreach (var item in items)
        {
            var nameCellValue = 
                $"[{_transform.EscapeMdText(item.DisplayName)}](./{_docItem.ToFileName()}/{item.ToFileName()}.md)";

            sb.AppendLine($@"| {nameCellValue} | {_transform.ToMdText(_docItem, item, t => t.XmlDocInfo?.Summary, true).Replace("\n", "<br />").Replace("\r", "")} |");
        }

        sb.AppendLine();
    }
}