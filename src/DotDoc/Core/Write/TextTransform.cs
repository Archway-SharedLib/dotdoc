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
    
    public string ToMdText(DocItem rootItem, DocItem targetItem, Func<DocItem, string> getText, bool removeNewLine = false) => RemoveNewLineIf(toMdTextRegex.Replace(getText(targetItem) ?? string.Empty, m =>
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
    }), removeNewLine);

    private string RemoveNewLineIf(string text, bool removeNewLine)
    {
        return removeNewLine ? text.ReplaceLineEndings(" ") : text;
    }
    
    public string EscapeMdText(string? text)
        => (text ?? string.Empty)
            .Replace("<", "\\<")
            .Replace(">", "\\>");

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