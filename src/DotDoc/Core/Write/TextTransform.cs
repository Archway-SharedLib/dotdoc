using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Immutable;
using System.Net;
using System.Text;
using System.Text.Encodings.Web;

namespace DotDoc.Core.Write;

public class TextTransform
{
    private readonly static Regex toMdTextRegex = new (@"(?<see>\<see\s+?cref\s*?=\s*?""(?<seecref>.+?)""\s*?/\>)|(?<c>\<c\>(.*?)\<\/c\>)|(?<text>.+?)", 
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
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
        var text = TrimEachLine(getText(targetItem), removeNewLine);

        return toMdTextRegex.Replace(text ?? string.Empty, m =>
        {
            var result = m.Value;
            if (m.Groups["see"].Success)
            {
                var seecrefGroup = m.Groups["seecref"];
                if (!seecrefGroup.Success) return result;
                
                var text = WebUtility.HtmlDecode(seecrefGroup.Value);
                // TODO: touri
                return ToMdLink(rootItem, text);
            }

            var cGroup = m.Groups["c"];
            if (cGroup.Success)
            {
                return $"`{cGroup.ValueSpan[3..^4]}`";
            }
            return EscapeMdText(result);
        });
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
    //public string ToMdLink()
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