using System.Text;
using DotDoc.Core.Models;
using Microsoft.CodeAnalysis;
using TypeInfo = DotDoc.Core.Models.TypeInfo;

namespace DotDoc.Core.Write.Page;

public abstract class BasePage
{
    private readonly IDocItem _docItem;
    private readonly TextTransform _transform;
    private readonly DocItemContainer _itemContainer;

    public BasePage(IDocItem docItem, TextTransform transform, DocItemContainer itemContainer)
    {
        _docItem = docItem;
        _transform = transform;
        _itemContainer = itemContainer;
    }

    protected virtual void AppendTitle(StringBuilder sb, string title, int depth = 1) =>
        sb.AppendLine($"{string.Concat(Enumerable.Repeat("#", depth))} {_transform.EscapeMdText(title)}").AppendLine();

    protected virtual void AppendExample(StringBuilder sb, IDocItem root, IDocItem target, int depth = 2)
    {
        if (!string.IsNullOrEmpty(target.XmlDocInfo?.Example))
        {
            AppendTitle(sb, "Example", depth);
            sb.AppendLine(_transform.ToMdText(root, target, t => t.XmlDocInfo?.Example)).AppendLine();
        }
    }
    
    protected virtual void AppendRemarks(StringBuilder sb, IDocItem root, IDocItem target, int depth = 2)
    {
        if (!string.IsNullOrEmpty(target.XmlDocInfo?.Remarks))
        {
            AppendTitle(sb, "Remarks", depth);
            sb.AppendLine(_transform.ToMdText(root, target, t => t.XmlDocInfo?.Remarks)).AppendLine();
        }
    }
    
    protected virtual void AppendItemList<T>(StringBuilder sb, string title, IEnumerable<IDocItem> docItems, int depth = 2) where T : IDocItem
    {
        var items = docItems.OrEmpty().OfType<T>();
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
    
    protected virtual void AppendFieldItemList(StringBuilder sb, IEnumerable<IDocItem> docItems, int depth = 2, bool needFieldLInk = true)
    {
        var items = docItems.OrEmpty().OfType<FieldDocItem>();
        if (!items.Any()) return;

        AppendTitle(sb,"Fields", depth);

        sb.AppendLine("| Name | Value | Summary |");
        sb.AppendLine("|------|-------|---------|");
        
        foreach (var item in items)
        {
            var nameCellValue = needFieldLInk ?
                $"[{_transform.EscapeMdText(item.DisplayName)}](./{_docItem.ToFileName()}/{item.ToFileName()})" :
                _transform.EscapeMdText(item.DisplayName);

            sb.AppendLine($@"| {nameCellValue} | {GetConstantValueDisplayText(item) } | {_transform.ToMdText(_docItem, item, t => t.XmlDocInfo?.Summary, true).Replace("\n", "<br />").Replace("\r", "")} |");
        }

        sb.AppendLine();
    }

    private string GetConstantValueDisplayText(FieldDocItem fieldDocItem)
    {
        if (!fieldDocItem.IsConstant) return string.Empty;
        var value = fieldDocItem.ConstantValue;
        if (value is null) return "null";
        var text = value is char ? $"'{value}'" :
            value is string ? $"\"{value}\"" : value.ToString();
        return _transform.EscapeMdText(text);
    }
    
    protected virtual void AppendTypeParameterList(StringBuilder sb, IEnumerable<TypeParameterDocItem> typeParameters, int depth = 2)
    {
        if(typeParameters.Any())
        {
            AppendTitle(sb,"Type Parameters", depth);
            
            foreach(var param in typeParameters)
            {
                var constraints = TypeParameterConstraintsMd(param);
                sb.Append($@"__{_transform.EscapeMdText(param.DisplayName)}__");
                if (constraints.Any())
                {
                    sb.Append(" : ").AppendJoin(", ", TypeParameterConstraintsMd(param));
                }
                sb.AppendLine()
                    .AppendLine()
                    .AppendLine(
                        $"{_transform.ToMdText(_docItem, param, t => t.XmlDocText)}")
                    .AppendLine();
            }
            
            // sb.AppendLine("| Name | Summary |");
            // sb.AppendLine("|------|---------|");
            // foreach(var param in typeParameters)
            // {
            //     sb.AppendLine($@"| {_transform.EscapeMdText(param.DisplayName)} | {_transform.ToMdText(_docItem, param, t => t.XmlDocText, true).Replace("\n", "<br />").Replace("\r", "")} |");
            // }
            // sb.AppendLine();
        }
    }

    private List<string> TypeParameterConstraintsMd(TypeParameterDocItem typeParam)
    {
        var result = typeParam.ConstraintTypes.Select(t =>
            _transform.ToMdLink(_docItem, t.GetLinkTypeInfo().TypeId, t.GetLinkTypeInfo().DisplayName)).ToList();

        if(typeParam.HasConstructorConstraint) result.Add("`new()`");
        if(typeParam.HasReferenceTypeConstraint) result.Add($"`class{(typeParam.HasReferenceTypeConstraintNullableAnnotation ? "?" : "")}`");
        if(typeParam.HasValueTypeConstraint) result.Add($"`struct`");
        if(typeParam.HasNotNullConstraint) result.Add($"`notnull`");
        if(typeParam.HasUnmanagedTypeConstraint) result.Add($"`unmanaged`");

        return result;
    }
    
    protected virtual void AppendDeclareCode(StringBuilder sb)
    {
        sb.AppendLine("```csharp");
        sb.AppendLine(_docItem.ToDeclareCSharpCode());
        sb.AppendLine("```");
        sb.AppendLine();
    }
    
    protected virtual void AppendNamespaceAssemblyInformation(StringBuilder sb, string assemblyId, string namespaceId, int indent = 1)
    {
        var nsDocItem = _itemContainer.Get(namespaceId);
        
        sb.AppendLine($"namespace: [{_transform.EscapeMdText(nsDocItem?.DisplayName)}]({string.Concat(Enumerable.Repeat("../", indent))}{nsDocItem.ToFileName()}.md)<br />");
        AppendAssemblyInformation(sb, assemblyId, indent + 1);
    }
    
    protected virtual void AppendAssemblyInformation(StringBuilder sb, string assemblyId, int indent = 1)
    {
        var assemDocItem = _itemContainer.Get(assemblyId);
        sb.AppendLine($"assembly: [{_transform.EscapeMdText(assemDocItem.DisplayName)}]({string.Concat(Enumerable.Repeat("../", indent))}{assemDocItem.ToFileName()}.md)").AppendLine();
    }
    
    protected virtual void AppendInheritAndImplements(StringBuilder sb, TypeDocItem typeDocItem)
    {
        if (typeDocItem.BaseType is not null)
        {
            var bases = new List<TypeInfo>();
            var baseType = typeDocItem.BaseType;
            while (baseType is not null)
            {
                bases.Add(baseType);
                baseType = baseType.BaseType;
            }

            var links = bases.AsEnumerable().Reverse().Select(b => _transform.ToMdLink(typeDocItem, b.GetLinkTypeInfo().TypeId, b.GetLinkTypeInfo().DisplayName));
            sb.AppendLine("Inheritance: " + string.Join(" > ", links.Append(typeDocItem.DisplayName))).AppendLine();
        }
        if (typeDocItem.Interfaces.OrEmpty().Any())
        {
            sb.AppendLine("Implements: " + string.Join(", ", typeDocItem.Interfaces.Select(i => _transform.ToMdLink(typeDocItem, i.GetLinkTypeInfo().TypeId, i.GetLinkTypeInfo().DisplayName)))).AppendLine();    
        }
    }
    
    protected virtual void AppendParameterList(StringBuilder sb, IEnumerable<ParameterDocItem> parameters, int depth = 2)
    {
        var paramList = parameters.ToList();
        if(paramList.Any())
        {
            AppendTitle(sb, "Parameters", depth);
            
            foreach(var param in paramList)
            {
                var linkType = param.TypeInfo.GetLinkTypeInfo();
                sb.AppendLine($@"__{_transform.EscapeMdText(param.DisplayName)}__ : {_transform.ToMdLink(_docItem,  linkType.TypeId, linkType.DisplayName)}")
                    .AppendLine()
                    .AppendLine($"{_transform.ToMdText(_docItem, param, t => t.XmlDocText)}")
                    .AppendLine();
            }
            
            // sb.AppendLine("| Type | Name | Summary |");
            // sb.AppendLine("|------|------|---------|");
            // foreach(var param in paramList)
            // {
            //     var linkType = param.TypeInfo.GetLinkTypeInfo();
            //     sb.AppendLine($@"| {_transform.ToMdLink(_docItem,  linkType.TypeId, linkType.DisplayName)} | {_transform.EscapeMdText(param.DisplayName)} | {_transform.ToMdText(_docItem, param, t => t.XmlDocText, true).Replace("\n", "<br />").Replace("\r", "")} |");
            // }
            // sb.AppendLine();
            
            
        }
    }
    
    protected virtual void AppendReturnValue(StringBuilder sb, ReturnItem returnItem, int depth = 2)
    {
        if (returnItem?.TypeInfo is null) return;
            
        AppendTitle(sb, "Return Value", depth);
        var linkType = returnItem.TypeInfo.GetLinkTypeInfo();
        sb.AppendLine(_transform.ToMdLink(_docItem,  linkType.TypeId, linkType.DisplayName)).AppendLine();
        sb.AppendLine(_transform.ToMdText(_docItem, _docItem, t => t.XmlDocInfo?.Returns)).AppendLine();
    }
}