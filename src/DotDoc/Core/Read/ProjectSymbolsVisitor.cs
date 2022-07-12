using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Read;

public class ProjectSymbolsVisitor : SymbolVisitor<DocItem>
{
    private readonly IFilter _filter;

    public ProjectSymbolsVisitor(IFilter filter)
    {
        _filter = filter;
    }
    
    public override DocItem? VisitAssembly(IAssemblySymbol symbol)
    {
        var id = VisitorUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        var item = new AssemblyDocItem()
        {
            Id = id,
            Name = symbol.Name,
            DisplayName = symbol.Name,
            XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml())
        };

        item.Namespaces = VisitDescendants(
            symbol.GlobalNamespace.GetNamespaceMembers(),
            ns => ns.GetNamespaceMembers())
            .OfType<NamespaceDocItem>().ToList();

        return item;
    }

    public override DocItem? VisitNamespace(INamespaceSymbol symbol)
    {
        var id = VisitorUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        var item = new NamespaceDocItem()
        {
            Id = id,
            Name = symbol.Name,
            DisplayName = symbol.Name,
            AssemblyId = VisitorUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml())
        };

        item.Types = VisitDescendants(
            symbol.GetTypeMembers(),
            ns => ns.GetTypeMembers())
            .OfType<TypeDocItem>().ToList();

        return item;
    }

    public override DocItem? VisitNamedType(INamedTypeSymbol symbol)
    {
        var id = VisitorUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        var item = CreateTypedTypeDocItem(symbol);
        if (item is null) return null;
        item.Id = id;
        item.Name = symbol.Name;
        item.DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingNamespace.ToDisplayString().Length + 1);
        item.NamespaceId = VisitorUtil.GetSymbolId(symbol.ContainingNamespace);
        item.XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml());
        item.AssemblyId = VisitorUtil.GetSymbolId(symbol.ContainingAssembly);

        item.Members = new();
        foreach (var member in symbol.GetMembers().Where(s => !(s is INamedTypeSymbol)))
        {
            if (member.Accept(this) is MemberDocItem mItem)
            {
                item.Members.Add(mItem);
            }
        }

        return item;
    }

    private TypeDocItem? CreateTypedTypeDocItem(INamedTypeSymbol symbol)
    {
        if (symbol.TypeKind == TypeKind.Class) return new ClassDocItem();
        if (symbol.TypeKind == TypeKind.Interface) return new ClassDocItem();
        if (symbol.TypeKind == TypeKind.Enum) return new EnumDocItem();
        if (symbol.TypeKind == TypeKind.Struct) return new StructDocItem();
        if (symbol.TypeKind == TypeKind.Delegate) return new DelegateDocItem();

        return null;
    }

    public override DocItem? VisitField(IFieldSymbol symbol)
    {
        var id = VisitorUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        var item = new FieldDocItem()
        {
            Id = id,
            Name = symbol.Name,
            DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingType.ToDisplayString().Length + 1),
            TypeId = VisitorUtil.GetSymbolId(symbol.ContainingType),
            NamespaceId = VisitorUtil.GetSymbolId(symbol.ContainingNamespace),
            AssemblyId = VisitorUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml())
        };

        return item;
    }

    public override DocItem? VisitProperty(IPropertySymbol symbol)
    {
        var id = VisitorUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        var item = new PropertyDocItem()
        {
            Id = id,
            Name = symbol.Name,
            DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingType.ToDisplayString().Length + 1),
            TypeId = VisitorUtil.GetSymbolId(symbol.ContainingType),
            NamespaceId = VisitorUtil.GetSymbolId(symbol.ContainingNamespace),
            AssemblyId = VisitorUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml())
        };

        return item;
    }

    public override DocItem? VisitMethod(IMethodSymbol symbol)
    {
        var id = VisitorUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        // if generated
        if (symbol.MethodKind is
            MethodKind.PropertyGet or
            MethodKind.PropertySet or
            MethodKind.EventAdd or
            MethodKind.EventRemove or
            MethodKind.EventRaise) return null;

        // if (symbol.IsImplicitConstructor()) return null;

        return symbol.MethodKind == MethodKind.Constructor ? VisitConstructor(symbol) : VisitPlaneMethod(symbol);
    }

    private DocItem VisitPlaneMethod(IMethodSymbol symbol)
    {
        var docInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml());
        return new MethodDocItem()
        {
            Id = VisitorUtil.GetSymbolId(symbol),
            Name = symbol.Name,
            DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingType.ToDisplayString().Length + 1),
            TypeId = VisitorUtil.GetSymbolId(symbol.ContainingType),
            NamespaceId = VisitorUtil.GetSymbolId(symbol.ContainingNamespace),
            AssemblyId = VisitorUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = docInfo,
            Parameters = RetrieveParameters(symbol, docInfo)
        };
    }

    private DocItem VisitConstructor(IMethodSymbol symbol)
    {
        var docInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml());
        return new ConstructorDocItem()
        {
            Id = VisitorUtil.GetSymbolId(symbol),
            Name = symbol.Name,
            DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingType.ToDisplayString().Length + 1),
            TypeId = VisitorUtil.GetSymbolId(symbol.ContainingType),
            NamespaceId = VisitorUtil.GetSymbolId(symbol.ContainingNamespace),
            AssemblyId = VisitorUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = docInfo,
            Parameters = RetrieveParameters(symbol, docInfo)
        };
    }

    private List<ParameterDocItem> RetrieveParameters(IMethodSymbol symbol, XmlDocInfo docInfo)
    {
        return symbol.Parameters.Select(ps =>
        {
            return new ParameterDocItem()
            {
                Name = ps.Name,
                Id = VisitorUtil.GetSymbolId(ps),
                DisplayName = ps.Name,
                TypeId = VisitorUtil.GetSymbolId(ps.Type),
                TypeName = ps.Type.Name,
                TypeDisplayName = ps.Type.ToDisplayString(),
                XmlDocText = docInfo.Parameters.OrEmpty().FirstOrDefault(p => p.Name == ps.Name)?.Text
            };
        }).ToList();
    } 

    public override DocItem? VisitEvent(IEventSymbol symbol)
    {
        var id = VisitorUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        var item = new EventDocItem()
        {
            Id = id,
            Name = symbol.Name,
            DisplayName = symbol.Name,
            TypeId = VisitorUtil.GetSymbolId(symbol.ContainingType),
            NamespaceId = VisitorUtil.GetSymbolId(symbol.ContainingNamespace),
            AssemblyId = VisitorUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml())
        };
        return item;
    }

    private List<DocItem> VisitDescendants<T>(
            IEnumerable<T> children,
            Func<T, IEnumerable<T>> getChildren)
            where T : ISymbol
    {
        var result = new List<DocItem>();
        var stack = new Stack<T>(children.Reverse());
        while (stack.Count > 0)
        {
            var child = stack.Pop();
            var item = child.Accept(this);
            if (item != null)
            {
                result.Add(item);
            }

            foreach (var m in getChildren(child).Reverse())
            {
                stack.Push(m);
            }
        }
        return result;
    }
}