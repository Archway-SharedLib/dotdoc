using System.Data;
using System.Text.RegularExpressions;
using System.Collections.Immutable;

namespace DotDoc.Core.Write;

internal class TextTransform
{
    private readonly static Regex toMdTextRegex = new (@"(?<see>\<see\s+?cref\s*?=\s*?""(?<seecref>.+?)""\s*?/\>)|(?<text>.+?)", 
        RegexOptions.Compiled | RegexOptions.Multiline | RegexOptions.ExplicitCapture | RegexOptions.IgnoreCase);
    private readonly ImmutableDictionary<string,DocItem> _items;
    private readonly IFileSystemOperation _fileSystemOperation;

    public TextTransform(ImmutableDictionary<string,DocItem> items, IFileSystemOperation fileSystemOperation)
    {
        _items = items ?? throw new ArgumentNullException(nameof(items));
        _fileSystemOperation = fileSystemOperation ?? throw new ArgumentNullException(nameof(fileSystemOperation));
    }

    public string ToMdText<T>(DocItem rootItem, T targetItem, Func<T, string> getText, bool removeNewLine = false) where T : DocItem
    {
        var text = TrimEachLine(getText(targetItem), removeNewLine);

        return toMdTextRegex.Replace(text ?? string.Empty, m =>
        {
            var result = m.Value;
            if (m.Groups["see"].Success)
            {
                var seecrefGroup = m.Groups["seecref"];
                if (!seecrefGroup.Success) return result;
                // TODO: touri
                return ToMdLink(rootItem, seecrefGroup.Value);
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
        => new[] { "*", "_", "\\", "`", "#", "+", "-", ".", "!", "{", "}", "[", "]", "(", ")" }.Aggregate(text, (curr, val) =>
        {
            var newText = curr ?? string.Empty;
            return newText.Replace(val, "\\" + val);
        });

    internal string ToMdLink(DocItem baseItem, string key)
    {
        var linkText = key;
        var displayText = key;
        if (_items.ContainsKey(key))
        {
            var destItem = _items[key];
            linkText = _fileSystemOperation.GetRelativeLink(baseItem, destItem);
            displayText = destItem.DisplayName;
        }
        if(key.StartsWith("T:Microsoft.", StringComparison.InvariantCultureIgnoreCase) || key.StartsWith("T:System.", StringComparison.InvariantCultureIgnoreCase))
        {
            displayText = key.Substring(2);
            linkText = $"https://docs.microsoft.com/ja-jp/dotnet/api/{displayText})";
        }

        return $"[{EscapeMdText(displayText)}]({linkText})";
    }
}