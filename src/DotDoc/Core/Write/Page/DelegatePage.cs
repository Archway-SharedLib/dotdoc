using System.Text;

namespace DotDoc.Core.Write.Page;

public class DelegatePage: IPage
{
    private readonly DelegateDocItem _item;
    private readonly TextTransform _transform;
    private readonly DocItemContainer _itemContainer;

    public DelegatePage(DelegateDocItem item, TextTransform transform, DocItemContainer itemContainer)
    {
        _item = item ?? throw new ArgumentNullException(nameof(item));
        _transform = transform ?? throw new ArgumentNullException(nameof(transform));
        _itemContainer = itemContainer ?? throw new ArgumentNullException(nameof(itemContainer));
    }
    
    public string Write()
    {
        var sb = new StringBuilder();
  
        AppendTitle(sb, $"{_item.DisplayName} Delegate");
        AppendNamespaceAssemblyInformation(sb);

        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Summary)).AppendLine();
        
        AppendDeclareCode(sb);
        AppendInheritAndImplements(sb);

        AppendTypeParameterList(sb, _item.TypeParameters.OrEmpty());
        AppendParameterList(sb, _item.Parameters.OrEmpty());
        
        AppendReturnValue(sb);
        
        return sb.ToString();
    }
    
    private void AppendTitle(StringBuilder sb, string title, int depth = 1) =>
        sb.AppendLine($"{string.Concat(Enumerable.Repeat("#", depth))} {_transform.EscapeMdText(title)}").AppendLine();
    
    
    private void AppendTypeParameterList(StringBuilder sb, IEnumerable<TypeParameterDocItem> typeParameters, int depth = 2)
    {
        if(typeParameters.Any())
        {
            AppendTitle(sb,"Type Parameters", depth);
            sb.AppendLine("| Name | Summary |");
            sb.AppendLine("|------|---------|");
            foreach(var param in typeParameters)
            {
                sb.AppendLine($@"| {_transform.EscapeMdText(param.DisplayName)} | {_transform.ToMdText(_item, param, t => t.XmlDocText, true)} |");
            }
            sb.AppendLine();
        }
    }
    
    private void AppendParameterList(StringBuilder sb, IEnumerable<ParameterDocItem> parameters, int depth = 2)
    {
        if(parameters.Any())
        {
            AppendTitle(sb, "Parameters", depth);
            sb.AppendLine("| Type | Name | Summary |");
            sb.AppendLine("|------|------|---------|");
            foreach(var param in parameters)
            {
                sb.AppendLine($@"| {_transform.ToMdLink(_item,  param.TypeInfo.TypeId, param.TypeInfo.DisplayName)} | {_transform.EscapeMdText(param.DisplayName)} | {_transform.ToMdText(_item, param, t => t.XmlDocText, true)} |");
            }
            sb.AppendLine();
        }
    }
    
    private void AppendReturnValue(StringBuilder sb, int depth = 2)
    {
        if (_item.ReturnValue?.TypeInfo is null) return;
            
        AppendTitle(sb, "Return Value", depth);
        sb.AppendLine(_transform.ToMdLink(_item,  _item.ReturnValue.TypeInfo.GetLinkTypeInfo().TypeId)).AppendLine();
        sb.AppendLine(_transform.ToMdText(_item, _item, t => t.XmlDocInfo?.Returns)).AppendLine();
    }

    private void AppendNamespaceAssemblyInformation(StringBuilder sb)
    {
        var assemDocItem = _itemContainer.Get(_item.AssemblyId);
        var nsDocItem = _itemContainer.Get(_item.NamespaceId);
        
        sb.AppendLine($"namespace: [{_transform.EscapeMdText(nsDocItem?.DisplayName)}](../{nsDocItem.ToFileName()}.md)<br />");
        sb.AppendLine($"assembly: [{_transform.EscapeMdText(assemDocItem.DisplayName)}](../../{assemDocItem.ToFileName()}.md)").AppendLine();
    }
    
    private void AppendDeclareCode(StringBuilder sb)
    {
        sb.AppendLine("```csharp");
        sb.AppendLine(_item.ToDeclareCSharpCode());
        sb.AppendLine("```");
        sb.AppendLine();
    }

    private void AppendInheritAndImplements(StringBuilder sb)
    {
        if (!string.IsNullOrEmpty(_item.BaseTypeId))
        {
            var bases = new List<string>();
            var baseId = _item.BaseTypeId;
            while (!string.IsNullOrEmpty(baseId))
            {
                bases.Add(baseId);
                var baseDocItem = _itemContainer.Get(baseId);
                baseId = null;
                if (baseDocItem is TypeDocItem baseTypeDoc)
                {
                    baseId = baseTypeDoc.BaseTypeId;
                }
            }

            var links = bases.AsEnumerable().Reverse().Select(id => _transform.ToMdLink(_item, id));
            sb.AppendLine("Inheritance: " + string.Join(" > ", links.Append(_item.DisplayName))).AppendLine();
        }
        if (_item.InterfaceIds.OrEmpty().Any())
        {
            sb.AppendLine("Implements: " + string.Join(", ", _item.InterfaceIds.Select(id => _transform.ToMdLink(_item, id)))).AppendLine();    
        }
    }
}