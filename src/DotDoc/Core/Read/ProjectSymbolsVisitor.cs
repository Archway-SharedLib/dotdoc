using System.Collections.Immutable;
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
        var id = SymbolUtil.GetSymbolId(symbol);
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
        var id = SymbolUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        var item = new NamespaceDocItem()
        {
            Id = id,
            Name = symbol.Name,
            DisplayName = symbol.ToDisplayString(),
            AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly),
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
        var id = SymbolUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        return CreateTypedTypeDocItem(symbol);
    }
    
    private TypeDocItem? CreateTypedTypeDocItem(INamedTypeSymbol symbol)
    {
        T? CreateDocItem<T>(Action<T> additionalAction) where T : TypeDocItem, new()
        {
            var id = SymbolUtil.GetSymbolId(symbol);
            if (_filter.Exclude(symbol, id)) return null;

            var item = new T
            {
                Id = id,
                Name = symbol.Name,
                DisplayName = symbol.ToDisplayString()
                    .Substring(symbol.ContainingNamespace.ToDisplayString().Length + 1),
                NamespaceId = SymbolUtil.GetSymbolId(symbol.ContainingNamespace),
                XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml()),
                AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly),
                Accessiblity = SymbolUtil.MapAccessibility(symbol.DeclaredAccessibility)
            };
            additionalAction(item);
            return item;
        }

        List<MemberDocItem> GetMembers()
        {
            var members = new List<MemberDocItem>();
            foreach (var member in symbol.GetMembers().Where(s => s is not INamedTypeSymbol))
            {
                if (member.Accept(this) is MemberDocItem mItem)
                {
                    members.Add(mItem);
                }
            }
            return members;
        }
        
        if (symbol.TypeKind == TypeKind.Class) return CreateDocItem<ClassDocItem>(v =>
        {
            v.IsSealed = symbol.IsSealed;
            v.IsAbstract = symbol.IsAbstract;
            v.IsStatic = symbol.IsStatic;
            v.TypeParameters = RetrieveTypeParameters(symbol.TypeParameters, v.XmlDocInfo);
            v.Members = GetMembers();
        });
        if (symbol.TypeKind == TypeKind.Interface) return CreateDocItem<InterfaceDocItem>(v =>
        {
            v.TypeParameters = RetrieveTypeParameters(symbol.TypeParameters, v.XmlDocInfo);
            v.Members = GetMembers();
        });
        if (symbol.TypeKind == TypeKind.Enum) return CreateDocItem<EnumDocItem>(v =>
        {
            v.Members = GetMembers();
        });
        if (symbol.TypeKind == TypeKind.Struct) return CreateDocItem<StructDocItem>(v =>
        {
            v.TypeParameters = RetrieveTypeParameters(symbol.TypeParameters, v.XmlDocInfo);
            v.Members = GetMembers();
        });
        if (symbol.TypeKind == TypeKind.Delegate) return CreateDocItem<DelegateDocItem>(v =>
        {
            v.TypeParameters = RetrieveTypeParameters(symbol.TypeParameters, v.XmlDocInfo);
            var delegMethod = symbol.DelegateInvokeMethod;
            v.Parameters = RetrieveParameters(delegMethod.Parameters, v.XmlDocInfo);
            v.ReturnValue = delegMethod.ReturnsVoid
                ? null
                : new TypeInfo()
                {
                    TypeId = SymbolUtil.GetSymbolId(delegMethod.ReturnType),
                    Name = delegMethod.ReturnType.Name,
                    DisplayName = delegMethod.ReturnType.ToDisplayString(),
                };
        });

        return null;
    }

    public override DocItem? VisitField(IFieldSymbol symbol)
    {
        var id = SymbolUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        var item = new FieldDocItem()
        {
            Id = id,
            Name = symbol.Name,
            DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingType.ToDisplayString().Length + 1),
            TypeId = SymbolUtil.GetSymbolId(symbol.ContainingType),
            NamespaceId = SymbolUtil.GetSymbolId(symbol.ContainingNamespace),
            AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml()),
            Accessiblity = SymbolUtil.MapAccessibility(symbol.DeclaredAccessibility),
            TypeInfo = new TypeInfo()
            {
                TypeId = SymbolUtil.GetSymbolId(symbol.Type),
                DisplayName = symbol.Type.ToDisplayString(),
                Name = symbol.Type.Name
            }
        };

        return item;
    }

    public override DocItem? VisitProperty(IPropertySymbol symbol)
    {
        var id = SymbolUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;
        
        var item = new PropertyDocItem()
        {
            Id = id,
            Name = symbol.Name,
            DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingType.ToDisplayString().Length + 1),
            TypeId = SymbolUtil.GetSymbolId(symbol.ContainingType),
            NamespaceId = SymbolUtil.GetSymbolId(symbol.ContainingNamespace),
            AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml()),
            Accessiblity = SymbolUtil.MapAccessibility(symbol.DeclaredAccessibility),
            TypeInfo = new TypeInfo()
            {
                TypeId = SymbolUtil.GetSymbolId(symbol.Type),
                DisplayName = symbol.Type.ToDisplayString(),
                Name = symbol.Type.Name
            },
            HasGet = symbol.GetMethod is not null,
            HasSet = symbol.SetMethod is not null,
            IsInit = symbol.SetMethod?.IsInitOnly ?? false,
            IsStatic = symbol.IsStatic,
            IsAbstract = symbol.IsAbstract,
            IsOverride = symbol.IsOverride,
            IsVirtual = symbol.IsVirtual
        };

        return item;
    }

    public override DocItem? VisitMethod(IMethodSymbol symbol)
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

    private DocItem VisitPlaneMethod(IMethodSymbol symbol)
    {
        var docInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml());

        return new MethodDocItem()
        {
            Id = SymbolUtil.GetSymbolId(symbol),
            Name = symbol.Name,
            DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingType.ToDisplayString().Length + 1),
            TypeId = SymbolUtil.GetSymbolId(symbol.ContainingType),
            NamespaceId = SymbolUtil.GetSymbolId(symbol.ContainingNamespace),
            AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = docInfo,
            Parameters = RetrieveParameters(symbol.Parameters, docInfo),
            TypeParameters = RetrieveTypeParameters(symbol.TypeParameters, docInfo),
            Accessiblity = SymbolUtil.MapAccessibility(symbol.DeclaredAccessibility),
            ReturnValue = symbol.ReturnsVoid ? null :  new TypeInfo()
            {
                TypeId = SymbolUtil.GetSymbolId(symbol.ReturnType),
                Name = symbol.ReturnType.Name,
                DisplayName = symbol.ReturnType.ToDisplayString(),
            },
            IsStatic = symbol.IsStatic,
            IsAbstract = symbol.IsAbstract,
            IsOverride = symbol.IsOverride,
            IsVirtual = symbol.IsVirtual
        };
    }

    private DocItem VisitConstructor(IMethodSymbol symbol)
    {
        var docInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml());
        return new ConstructorDocItem()
        {
            Id = SymbolUtil.GetSymbolId(symbol),
            Name = symbol.Name,
            DisplayName = symbol.ToDisplayString().Substring(symbol.ContainingType.ToDisplayString().Length + 1),
            TypeId = SymbolUtil.GetSymbolId(symbol.ContainingType),
            NamespaceId = SymbolUtil.GetSymbolId(symbol.ContainingNamespace),
            AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = docInfo,
            Parameters = RetrieveParameters(symbol.Parameters, docInfo),
            Accessiblity = SymbolUtil.MapAccessibility(symbol.DeclaredAccessibility),
        };
    }
    
    public override DocItem? VisitEvent(IEventSymbol symbol)
    {
        var id = SymbolUtil.GetSymbolId(symbol);
        if (_filter.Exclude(symbol, id)) return null;

        var item = new EventDocItem()
        {
            Id = id,
            Name = symbol.Name,
            DisplayName = symbol.Name,
            TypeId = SymbolUtil.GetSymbolId(symbol.ContainingType),
            NamespaceId = SymbolUtil.GetSymbolId(symbol.ContainingNamespace),
            AssemblyId = SymbolUtil.GetSymbolId(symbol.ContainingAssembly),
            XmlDocInfo = XmlDocParser.Parse(symbol.GetDocumentationCommentXml()),
            Accessiblity = SymbolUtil.MapAccessibility(symbol.DeclaredAccessibility),
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
    
    private List<ParameterDocItem> RetrieveParameters(ImmutableArray<IParameterSymbol> symbols, XmlDocInfo? docInfo)
    {
        return symbols.Select(ps =>
        {
            return new ParameterDocItem()
            {
                Name = ps.Name,
                Id = SymbolUtil.GetSymbolId(ps),
                DisplayName = ps.Name,
                TypeInfo = new TypeInfo()
                {
                    TypeId = SymbolUtil.GetSymbolId(ps.Type),
                    Name = ps.Type.Name,
                    DisplayName = ps.Type.ToDisplayString(),
                },
                XmlDocText = docInfo?.Parameters.OrEmpty().FirstOrDefault(p => p.Name == ps.Name)?.Text
            };
        }).ToList();
    } 
    
    private static List<TypeParameterDocItem> RetrieveTypeParameters(ImmutableArray<ITypeParameterSymbol> symbols, XmlDocInfo? docInfo)
    {
        var typeParamItems = new List<TypeParameterDocItem>();
        foreach (var typeParam in symbols.OrEmpty())
        {
            typeParamItems.Add(new TypeParameterDocItem()
            {
                Id = SymbolUtil.GetSymbolId(typeParam),
                DisplayName = typeParam.ToDisplayString(),
                Name = typeParam.Name,
                XmlDocText = docInfo?.TypeParameters.OrEmpty().FirstOrDefault(p => p.Name == typeParam.Name)?.Text
            });
        }

        return typeParamItems;
    }
}