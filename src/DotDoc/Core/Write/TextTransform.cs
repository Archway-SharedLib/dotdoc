using System.Net;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using DotDoc.Core.Models;

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

    public string ToMdText<T>(IDocItem rootItem, T targetItem, Func<T, string> getText, bool singleLine = false)
        where T : IDocItem
    {

        var text = $"<doc>{TrimEachLine(getText(targetItem), singleLine)}</doc>";
        var root = XElement.Parse(text);

        var sb = new StringBuilder();
        WriteNodes(sb, root, rootItem, singleLine);

        return sb.ToString();
    }

    private void WriteNodes(StringBuilder sb, XElement? root, IDocItem rootItem, bool singleLine = false)
    {
        if (root is null) return;
        var node = root.FirstNode;
        while (node is not null)
        {
            WriteTextNode(sb, node);
            if (node.NodeType == XmlNodeType.Element)
            {
                var elem = (XElement)node;
                if (elem is null) continue;
                var elemName = elem.Name.LocalName.ToLowerInvariant();

                WriteTypeParamRef(sb, elem, elemName);
                WriteParamRef(sb, elem, elemName);
                WriteC(sb, elem, elemName);
                WriteSee(sb, elem, elemName, rootItem);
                if (!singleLine)
                {
                    WritePara(sb, elem, elemName, rootItem);
                    WriteList(sb, elem, elemName, rootItem);
                    WriteCode(sb, elem, elemName, rootItem);
                }
            }
            node = node.NextNode;
        }
    }

    private string TrimEachLine(string text, bool removeNewLine)
    {
        if (string.IsNullOrEmpty(text)) return text;
        var trimedLines = text.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries).Select(t => t.Trim());
        return string.Join(removeNewLine ? string.Empty : Environment.NewLine, trimedLines);
    }
    
    public string EscapeMdText(string? text)
        => new[] { "*", "_", "\\", "`", "#", "+", "-", ".", "!", "{", "}", "[", "]", "(", ")", "<", ">" }.Aggregate(text ?? string.Empty, (curr, val) =>
        {
            var newText = curr ?? string.Empty;
            return newText.Replace(val, "\\" + val);
        });
    
    public string EscapeMdLinkText(string? text)
        => new[] { "`" }.Aggregate(text ?? string.Empty, (curr, val) =>
        {
            var newText = curr ?? string.Empty;
            return newText.Replace(val, "\\" + val);
        });

    public string ToMdLink(IDocItem baseItem, string? key, string? display = null, bool toCodeIfNoLink = true)
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
            var linkText = $"https://docs.microsoft.com/dotnet/api/{linkKey.Replace("`", "-").Replace("{", "").Replace("}", "")}";
            // var linkText = $"https://docs.microsoft.com/ja-jp/dotnet/api/{linkKey}";
            return $"[{EscapeMdText(display ?? linkKey)}]({linkText})";
        }

        return EscapeOrWrapBackquoteIfNeeded(toCodeIfNoLink, display ?? key.Substring(2));
    }

    private string EscapeOrWrapBackquoteIfNeeded(bool wrap, string value)
        => wrap ? $"`{value}`" : EscapeMdText(value);

    private void WriteTextNode(StringBuilder sb, XNode node)
    {
        if (node.NodeType == XmlNodeType.Text)
        {
            sb.Append(EscapeMdText(WebUtility.HtmlDecode(node.ToString())));
        }
    }
    private void WriteTypeParamRef(StringBuilder sb, XElement elem, string elementName)
    {
        if (elementName == "typeparamref")
        {
            sb.Append($"`{elem.Attribute("name")?.Value}`");
        }
    }
    
    private void WriteParamRef(StringBuilder sb, XElement elem, string elementName)
    {
        if (elementName == "paramref")
        {
            sb.Append($"`{elem.Attribute("name")?.Value}`");
        }
    }

    private void WriteC(StringBuilder sb, XElement elem, string elementName)
    {
        if (elementName == "c")
        {
            var text = elem.FirstNode is { NodeType: XmlNodeType.Text } ? elem.FirstNode.ToString().Trim() : string.Empty;
            sb.Append($"`{text}`");
        }
    }
    
    private void WriteSee(StringBuilder sb, XElement elem, string elementName, IDocItem rootItem)
    {
        if (elementName != "see")
        {
            return;
        }

        // var cref = elem.Attribute("cref")?.Value;
        if(elem.Attribute("cref")?.Value is { } cref)
        {
            var disp = elem.FirstNode is { NodeType: XmlNodeType.Text } ? elem.FirstNode.ToString().Trim() : string.Empty;
            sb.Append(ToMdLink(rootItem, cref, display: string.IsNullOrWhiteSpace(disp) ? null : disp));
            return;
        } 
        if(elem.Attribute("langword")?.Value is {} langword)
        {
            sb.Append($"`{langword}`");
            return;
        }
        if(elem.Attribute("href")?.Value is {} href)
        {
            var disp = elem.FirstNode is { NodeType: XmlNodeType.Text } ? elem.FirstNode.ToString().Trim() : string.Empty;
            if (string.IsNullOrEmpty(disp))
            {
                sb.Append($"{href}");
            }
            sb.Append($"[{EscapeMdText(disp)}]({href})");
            return;
        }
    }
    
    private void WritePara(StringBuilder sb, XElement elem, string elementName, IDocItem rootItem)
    {
        if (elementName == "para")
        {
            sb.AppendLine();
            WriteNodes(sb, elem, rootItem, false);
            sb.AppendLine();
        }
    }
    
    private void WriteList(StringBuilder sb, XElement elem, string elementName, IDocItem rootItem)
    {
        if (elementName != "list")
        {
            return;
        }

        var type = elem.Attribute("type").Value ?? "def";
        if (type == "table") WriteListTable(sb, elem, rootItem);
        if (type == "bullet") WriteListBullet(sb, elem, rootItem);
        if (type == "number") WriteListNumber(sb, elem, rootItem);
    }
    
    private void WriteListTable(StringBuilder sb, XElement elem, IDocItem rootItem)
    {
        var headerElem = elem.Element("listheader");

        var sb2 = new StringBuilder();

        sb.AppendLine("");
        sb.Append("| ");
        WriteNodes(sb, headerElem?.Element("term"), rootItem, true);
        sb.Append(" | ");
        WriteNodes(sb, headerElem?.Element("description"), rootItem, true);
        sb.AppendLine(" |");
        
        sb.AppendLine($"|--------------|--------------|");

        foreach (var itemElem in elem.Elements("item"))
        {
            sb.Append("| ");
            WriteNodes(sb, itemElem?.Element("term"), rootItem, true);
            sb.Append(" | ");
            WriteNodes(sb, itemElem?.Element("description"), rootItem, true);
            sb.AppendLine(" |");
        }

        sb.AppendLine();
    }
    
    private void WriteListBullet(StringBuilder sb, XElement elem, IDocItem rootItem)
    {
        sb.AppendLine();
        foreach (var itemElem in elem.Elements("item"))
        {
            sb.Append("- ");
            WriteNodes(sb, itemElem?.Element("description"), rootItem, true);
            sb.AppendLine();
        }
        sb.AppendLine();
    }
    
    private void WriteListNumber(StringBuilder sb, XElement elem, IDocItem rootItem)
    {
        sb.AppendLine();
        foreach (var itemElem in elem.Elements("item"))
        {
            sb.Append("1. ");
            WriteNodes(sb, itemElem?.Element("description"), rootItem, true);
            sb.AppendLine();
        }
        sb.AppendLine();
    }
    
    private void WriteCode(StringBuilder sb, XElement elem, string elementName, IDocItem rootItem)
    {
        if (elementName != "code")
        {
            return;
        }

        var lang = elem.Attribute("language")?.Value ?? "cs";
        var codeValue = elem.FirstNode is { NodeType: XmlNodeType.Text } ? elem.FirstNode.ToString().Trim() : string.Empty;
        // var codeValue = elem.Value?.Trim();
        
        sb.AppendLine();
        sb.AppendLine($"``` {lang}");
        sb.AppendLine(codeValue);
        sb.AppendLine("```");
        sb.AppendLine();
    }
}