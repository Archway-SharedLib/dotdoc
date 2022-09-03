using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace DotDoc.Core.Read;

public class ProjectSymbolsVisitor : SymbolVisitor<IDocItem>
{
    private readonly IFilter _filter;
    private readonly Compilation _compilation;

    public ProjectSymbolsVisitor(IFilter filter, Compilation compilation)
    {
        _filter = filter;
        _compilation = compilation;
    }
    
    public override IDocItem? VisitAssembly(IAssemblySymbol symbol)
    {
        var item = new AssemblyDocItem(symbol, _compilation);
        if (_filter.Exclude(symbol, item.Id)) return null;
        
        var namespaces = VisitDescendants(
            symbol.GlobalNamespace.GetNamespaceMembers(),
            ns => ns.GetNamespaceMembers())
            .OfType<NamespaceDocItem>().ToList();

        item.Namespaces.AddRange(namespaces);
        
        return item;
    }

    public override IDocItem? VisitNamespace(INamespaceSymbol symbol)
    {
        var item = new NamespaceDocItem(symbol, _compilation);
        if (_filter.Exclude(symbol, item.Id)) return null;
        
        var types = VisitDescendants(
            symbol.GetTypeMembers(),
            ns => ns.GetTypeMembers())
            .OfType<TypeDocItem>().ToList();
        item.Types.AddRange(types);
        
        return item;
    }

    public override IDocItem? VisitNamedType(INamedTypeSymbol symbol)
    {
        var id = SymbolUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        return CreateTypedTypeDocItem(symbol);
    }
    
    private TypeDocItem? CreateTypedTypeDocItem(INamedTypeSymbol symbol)
    {
        List<IMemberDocItem> GetMembers()
        {
            var members = new List<IMemberDocItem>();
            foreach (var member in symbol.GetMembers().Where(s => s is not INamedTypeSymbol))
            {
                if (member.Accept(this) is IMemberDocItem mItem)
                {
                    members.Add(mItem);
                }
            }
            return members;
        }
        
        if (symbol.TypeKind == TypeKind.Class)
        {
            var item = new ClassDocItem(symbol, _compilation);
            item.Members.AddRange(GetMembers());
            return item;
        }
        if (symbol.TypeKind == TypeKind.Interface)
        {
            var item = new InterfaceDocItem(symbol, _compilation);
            item.Members.AddRange(GetMembers());
            return item;
        }
        if (symbol.TypeKind == TypeKind.Enum) 
        {
            var item = new EnumDocItem(symbol, _compilation);
            item.Members.AddRange(GetMembers());
            return item;
        }
        if (symbol.TypeKind == TypeKind.Struct)
        {
            var item = new StructDocItem(symbol, _compilation);
            item.Members.AddRange(GetMembers());
            return item;
        }
        if (symbol.TypeKind == TypeKind.Delegate) 
        {
            var item = new DelegateDocItem(symbol, _compilation);
            return item;
        };
        return null;
    }

    public override IDocItem? VisitField(IFieldSymbol symbol)
    {
        var item = new FieldDocItem(symbol, _compilation);
        if (_filter.Exclude(symbol, item.Id)) return null;
        return item;
    }

    public override IDocItem? VisitProperty(IPropertySymbol symbol)
    {
        var item = new PropertyDocItem(symbol, _compilation);
        if (_filter.Exclude(symbol, item.Id)) return null;
        return item;
    }

    public override IDocItem? VisitMethod(IMethodSymbol symbol)
    {
        var id = SymbolUtil.GetSymbolId(symbol);
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

    private IDocItem VisitPlaneMethod(IMethodSymbol symbol)
    {
        var item = new MethodDocItem(symbol, _compilation);
        return item;
    }

    private IDocItem VisitConstructor(IMethodSymbol symbol)
    {
        var item = new ConstructorDocItem(symbol, _compilation);
        return item;
    }
    
    public override IDocItem? VisitEvent(IEventSymbol symbol)
    {
        var item = new EventDocItem(symbol, _compilation);
        if (_filter.Exclude(symbol, item.Id)) return null;
        return item;
    }

    private List<IDocItem> VisitDescendants<T>(
            IEnumerable<T> children,
            Func<T, IEnumerable<T>> getChildren)
            where T : ISymbol
    {
        var result = new List<IDocItem>();
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