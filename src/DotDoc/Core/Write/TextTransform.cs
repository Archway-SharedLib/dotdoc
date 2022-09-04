using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;

namespace DotDoc.Core.Write;

public class TextTransform
{
    private readonly DocItemContainer _items;
    private readonly IFileSystemOperation _fileSystemOperation;
    private readonly ILogger _logger;

    public TextTransform(DocItemContainer items, IFileSystemOperation fileSystemOperation, ILogger logger)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
        _fileSystemOperation = fileSystemOperation ?? throw new ArgumentNullException(nameof(fileSystemOperation));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    public string ToMdText<T>(IDocItem rootItem, T targetItem, Func<T, string> getText, bool removeNewLine = false) where T : IDocItem
    {
        var text = $"<doc>{TrimEachLine(getText(targetItem), removeNewLine)}</doc>";
        var root = XElement.Parse(text);
        var sb = new StringBuilder();
        var node = root.FirstNode;
        while (node is not null)
        {
            if (node.NodeType == XmlNodeType.Text)
            {
                sb.Append(EscapeMdText(WebUtility.HtmlDecode(node.ToString())));
            }
            if (node.NodeType == XmlNodeType.Element)
            {
                var elem = (XElement)node;
                if(elem is null) continue;
                var elemName = elem.Name.LocalName.ToLowerInvariant();
                if (elemName is "typeparamref" or "paramref")
                {
                    sb.Append($"`{elem.Attribute("name")?.Value}`");
                }
                
                if (elemName is "para")
                {
                    if (node.PreviousNode is not { NodeType: XmlNodeType.Element } ||
                        ((XElement)node.PreviousNode).Name != "para")
                    {
                        sb.AppendLine();
                        sb.AppendLine();
                    }

                    sb.AppendLine(elem.Value);
                    sb.AppendLine();
                }
                if (elemName is "c")
                {
                    sb.Append($"`{elem.Value}`");
                }
                if (elemName is "see")
                {
                    // var cref = elem.Attribute("cref")?.Value;
                    if(elem.Attribute("cref")?.Value is { } cref)
                    {
                        var disp = elem.Value;
                        sb.Append(ToMdLink(rootItem, cref, display: string.IsNullOrWhiteSpace(disp) ? null : disp));
                    } 
                    else if(elem.Attribute("langword")?.Value is {} langword)
                    {
                        sb.Append($"`{langword}`");
                    }
                }
            }
            node = node.NextNode;
        }

        return sb.ToString();
    }

    private string TrimEachLine(string text, bool removeNewLine)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var trimedLines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim());
        return string.Join(removeNewLine ? string.Empty : Environment.NewLine, trimedLines);
    }
    
    public string EscapeMdText(string text)
        => new[] { "*", "_", "\\", "`", "#", "+", "-", ".", "!", "{", "}", "[", "]", "(", ")", "<", ">" }.Aggregate(text, (curr, val) =>
        {
            var newText = curr ?? string.Empty;
            return newText.Replace(val, "\\" + val);
        });

    public string ToMdLink(IDocItem baseItem, string key, string? display = null, bool toCodeIfNoLink = true)
    {
        if (key is null)
        {
            _logger.Trace($"{nameof(ToMdLink)}: key is null : {baseItem.Id}");
            return EscapeOrWrapBackquoteIfNeeded( toCodeIfNoLink, display ?? "!no value!");
        }

        if (_items.TryGet(key, out var destItem))
        {
            return $"[{EscapeMdText(display ?? destItem.DisplayName)}]({_fileSystemOperation.GetRelativeLink(baseItem, destItem)})";
        }
        
        if (key.StartsWith("!:", StringComparison.InvariantCultureIgnoreCase))
        {
            return EscapeOrWrapBackquoteIfNeeded(toCodeIfNoLink, display ?? key.Substring(2));
        }
        
        if(key.StartsWith("T:Microsoft.", StringComparison.InvariantCultureIgnoreCase) || 
           key.StartsWith("T:System.", StringComparison.InvariantCultureIgnoreCase) ||
           key.StartsWith("N:Microsoft.", StringComparison.InvariantCultureIgnoreCase) ||
           key.StartsWith("N:System.", StringComparison.InvariantCultureIgnoreCase))
        {
            var linkKey = key.Substring(2);
            var linkText = $"https://docs.microsoft.com/ja-jp/dotnet/api/{linkKey.Replace("`", "-").Replace("{", "").Replace("}", "")}";
            // var linkText = $"https://docs.microsoft.com/ja-jp/dotnet/api/{linkKey}";
            return $"[{EscapeMdText(display ?? linkKey)}]({linkText})";
        }

        return EscapeOrWrapBackquoteIfNeeded(toCodeIfNoLink, display ?? key.Substring(2));
    }

    private string EscapeOrWrapBackquoteIfNeeded(bool wrap, string value)
        => wrap ? $"`{value}`" : EscapeMdText(value);
}