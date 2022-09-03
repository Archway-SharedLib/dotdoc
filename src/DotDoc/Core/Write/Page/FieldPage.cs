using System.Text;

namespace DotDoc.Core.Write.Page;

public class FieldPage: IPage
{
    private readonly FieldDocItem _item;
    private readonly TextTransform _transform;
    private readonly DocItemContainer _itemContainer;

    public FieldPage(FieldDocItem item, TextTransform transform, DocItemContainer itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        _itemContainer = itemContainer ?? throw new ArgumentNullException(nameof(itemContainer));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
        AppendTitle(sb, $"{_item.DisplayName} Field");
        AppendNamespaceAssemblyInformation(sb);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
            
        AppendDeclareCode(sb);
            
        AppendTitle(sb, "Field Value", 2);

        var linkType = _item.TypeInfo.GetLinkTypeInfo();
        sb.AppendLine($"{_transform.ToMdLink(_item, linkType.TypeId, linkType.DisplayName)}").AppendLine();
        if (!string.IsNullOrEmpty(_item.XmlDocInfo?.Value))
        {
            sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Value)).AppendLine();
        }
            
        return sb.ToString();
    }
    
    private void AppendTitle(StringBuilder sb, string title, int depth = 1) =>
        sb.AppendLine($"{string.Concat(Enumerable.Repeat("#", depth))} {_transform.EscapeMdText(title)}").AppendLine();

    private void AppendNamespaceAssemblyInformation(StringBuilder sb)
    {
        var assemDocItem = _itemContainer.Get(_item.AssemblyId);
        var nsDocItem = _itemContainer.Get(_item.NamespaceId);
        
        sb.AppendLine($"namespace: [{_transform.EscapeMdText(nsDocItem?.DisplayName)}](../../{nsDocItem.ToFileName()}.md)<br />");
        sb.AppendLine($"assembly: [{_transform.EscapeMdText(assemDocItem.DisplayName)}](../../../{assemDocItem.ToFileName()}.md)").AppendLine();
    }
    
    private void AppendDeclareCode(StringBuilder sb)
    {
        sb.AppendLine("```csharp");
        sb.AppendLine(_item.ToDeclareCSharpCode());
        sb.AppendLine("```");
        sb.AppendLine();
    }
}